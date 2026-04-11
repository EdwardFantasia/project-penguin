using System;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI;

//TODO: need to implement some sort of "turnaround" drag where quickly flicking from left to right or vice versa
//TODO: implement jumping (tmp done, some tweaking may need to be done to adjust via how long space is held)
//TODO: implement grapple (cut for now)
//TODO: implement slide (tmp done)
//TODO: implement stomp (tmp done)
//TODO: implement boost (tmp done)
//TODO: implement homing attack
//TODO: implement wall jump implementation (cut for now)
//TODO: model shield for shield enemy
//TODO: make it so boosting without physically moving character moves character ingame in currently-facing direction

//TODO: implement camera toggle between perspective and orthographic (strictly 2D) camera
//TODO: implement camera movement when character is no longer within frame
public class PlayerScript : MonoBehaviour, Movable
{
    public Rigidbody playerbody;
    private PlayerInputActions playerInputActions;
    public BoostScript boostScript;
    public BoxCollider frictBox;
    public BoxCollider hurtBox;
    public BoxCollider footBox;
    private RaycastHit objectHit;
    public float raycastDist = 8.5f;

    private bool canJump;
    private bool isMidair;
    private bool isStomping;
    private bool isSliding;
    private bool isBoosting;
    private bool passthrough = false;

    private Vector2 rbForce;

    public float moveSpeed = 13.5f;
    public float acceleration = 14f;
    public float slideDrag = 12f;
    public float jumpForce = 8f;
    public float stompForce = 9f;
    public float boostSpeedMult = 1.75f;
    public float boostAccelMult = 1.25f;
    public float curBoostResource; //can be restored through destruction of enemies
    public float origBoostMax = 100f; //can be increased through collection of collectibles in game
    public int curCollectibles;
    public float boxcastBoxSize = 10f;
    public float boxcastCastDistance = 50f;
    public int coyoteFrames = 6; //may need to be increased to 8 & TODO: make a counter for curCoyoteFrames instead of only using coyoteFrames
    /*public float nonBoostMax = 100f;
    public float boostMax = 250f;*/

    public float footboxMinY;
    public float objectHitMaxY;


    void Awake() {
        this.playerInputActions = new PlayerInputActions();
        this.curCollectibles = 0;
        this.boostScript = GetComponentInChildren<BoostScript>();
        this.playerbody = GetComponent<Rigidbody>();
        
        this.footBox = GetComponentInChildren<BoxCollider>();
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    private void OnEnable() {
        this.playerInputActions.PlayerActionMap.Enable();

        this.playerInputActions.PlayerActionMap.Slide.started += OnSlideHoldStart;
        this.playerInputActions.PlayerActionMap.Slide.canceled += OnSlideHoldEnd;

        this.playerInputActions.PlayerActionMap.Boost.started += OnBoostHoldStart;
        this.playerInputActions.PlayerActionMap.Boost.canceled += OnBoostHoldEnd;
    }

    private void OnDisable()
    {
        this.playerInputActions.PlayerActionMap.Disable();

        this.playerInputActions.PlayerActionMap.Slide.started -= OnSlideHoldStart;
        this.playerInputActions.PlayerActionMap.Slide.canceled -= OnSlideHoldEnd;

        this.playerInputActions.PlayerActionMap.Boost.started -= OnBoostHoldStart;
        this.playerInputActions.PlayerActionMap.Boost.canceled -= OnBoostHoldEnd;
    }
    private void OnSlideHoldStart(InputAction.CallbackContext context) { //pressing and holding slide cancels boost and slides
        this.endBoost();
        this.isSliding = true;
        if (this.isMidair){ //can stomp while midair
            this.isStomping = true;
        }
    }

    private void OnSlideHoldEnd(InputAction.CallbackContext context) { //no longer holding slide cancels slide
        this.isSliding = false;
    }
    private void OnBoostHoldStart(InputAction.CallbackContext context) { //pressing and holding boost activates boost and deactivates sliding and stomping
        this.startBoost();
        this.isSliding = false;
        this.isStomping = false;
    }

    private void OnBoostHoldEnd(InputAction.CallbackContext context) { //no longer holding boost cancels boost
        this.endBoost();
    }

    void disablePlayerPhysicsColliders(Collider otherCollider) { //disable frictBox and hurtBox collisions with otherCollider
        Physics.IgnoreCollision(this.frictBox, otherCollider);
        Physics.IgnoreCollision(this.hurtBox, otherCollider);
    }

    void enablePlayerPhysicsColliders(Collider otherCollider) { //enables frictBox and hurtBox collisions with otherCollider
        Physics.IgnoreCollision(this.frictBox, otherCollider, false);
        Physics.IgnoreCollision(this.hurtBox, otherCollider, false);
    }

    // Update is called once per framedd
    void Update()
    {
        if(this.coyoteFrames < 6) { //if player is falling from ground, have 5 coyote frames to jump and each frame decrease by 1
            Debug.Log($"coyoteFrames: {this.coyoteFrames}");
            this.coyoteFrames -= 1; //decrease cur coyoteFrames by 1
        }

        if (this.isStomping) { //placing this here fixed the bug where player model would clip into floor when stomping
            this.playerbody.AddForce(Vector2.down * (this.stompForce / 2), ForceMode.Impulse);
            this.setPlayerLinearVelocity(new Vector3(0, this.playerbody.linearVelocity.y, 0));
        }

        if (this.passthrough) { //if passing through bottom of platform...
            Physics.SyncTransforms(); //sync physics transforms
            this.footboxMinY = this.footBox.bounds.min.y;
            this.objectHitMaxY = this.objectHit.collider.bounds.max.y;
            if((footboxMinY - objectHitMaxY) >= 0){ //if footboxMinY is greater than platform's collider's max y, reenable the physics collision between platform and player
                this.enablePlayerPhysicsColliders(this.objectHit.collider);
            }
            Debug.DrawLine(new Vector3(-5, this.footboxMinY, 0), new Vector3(5, this.footboxMinY, 0), Color.green);
            Debug.DrawLine(new Vector3(-5, this.objectHitMaxY, 0), new Vector3(5, this.objectHitMaxY, 0), Color.red);
            Debug.Log($"Plat test - footboxMinY: ${footboxMinY}");
            Debug.Log($"Plat test - objectHitMaxY: ${this.objectHitMaxY}");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("test collision");
    }

    private void OnCollisionExit(Collision collision)
    {
        if (this.playerbody.linearVelocity.y < 0 && collision.collider.name.Contains("Ground")) { //if player falling and the player was last standing on a ground object...
            Debug.Log("exiting ground collision and falling");
            this.coyoteFrames -= 1; //start decreasing coyoteFrames
        }
    }

    public void processTrigger(string triggerName, Collider otherCollisionData) {
        if(triggerName == "Footbox") {
            if (otherCollisionData.name.Contains("Ground")) {
                this.coyoteFrames = 6;
                bool wasStomping = this.isStomping;
                this.isStomping = false;
                print("footbox touched ground");
                this.canJump = true;
                this.isMidair = false;
                if (!wasStomping && this.rbForce.x != 0) {
                    Debug.Log("landed after not stomping and moving along X still");
                    /*TODO: X-axis movement is impacted too negatively upon landing after not stomping, decrease how much player "stops" on landing, include setting of playerVel here*/
                }
            }
        }
    }

    void OnMove2(InputValue value) {
        this.rbForce = value.Get<Vector2>();
        if(this.rbForce.x != 0) {
            this.playerbody.rotation = Quaternion.Euler(0, (this.rbForce.x > 0 ? 0 : 180), 0);
        }
        this.rbForce.y = 0; //IMPORTANT: used to remove any influence pressing up on control stick has on player while jumping, may need to remove in order to do a move in the future

    }

    void OnJump() {
        if (canJump && !isMidair && (this.coyoteFrames > 0)) {
            this.passthrough = Physics.BoxCast(center: new Vector3(transform.position.x - .05f, (transform.position.y + this.frictBox.size.y) - .5f, transform.position.z), halfExtents: new Vector3(.5f, .5f, 0), direction: transform.up, out this.objectHit, orientation: transform.rotation, maxDistance: this.raycastDist, layerMask: 1 << 6);
            if (this.passthrough) {
                this.disablePlayerPhysicsColliders(this.objectHit.collider);
            }
            this.playerbody.AddForce(Vector2.up * jumpForce, ForceMode.Impulse);
            this.isMidair = true;
            this.canJump = false;
        }
    }

    public void setCanJump(bool canJumpValue) { //for objects like enemies to call
        this.canJump = canJumpValue;
    }

    public void setPlayerLinearVelocity(Vector3 smoothedVelocity) {
        this.playerbody.linearVelocity = smoothedVelocity; // set new velocity with acceleration applied (smoothed velocity) to player's linear velocity
    }

    public void endBoost() {
        this.isBoosting = false;
        this.boostScript.disableBoostElements();
    }

    public void startBoost() {
        this.isBoosting = true;
        this.boostScript.enableBoostElements();
    }

    void FixedUpdate() //used for physics changes
    {
        this.playerbody.AddForce(this.rbForce);
        if (!this.isStomping) { 
            // IMPORTANT: velocity is speed with a direction, acceleration is the rate of change of that velocity
            //ex. velocity is 60 mph and acceleration is 0 to 60mph in 3.2s

            float accelMult = this.isBoosting ? this.boostAccelMult : 1;

            float intermedVelMod = this.isSliding ? this.slideDrag * Time.deltaTime : ((this.acceleration * accelMult) * Time.deltaTime);

            float boostMultiplier = this.isBoosting ? this.boostSpeedMult : 1;

            float vectorX = this.isSliding ? 0 : (this.rbForce.x * (this.moveSpeed * boostMultiplier));

            Vector3 targetVelocity = ((Movable)this).calculateTargetVelocity(vector_XInput: vectorX, vector_YInput: this.playerbody.linearVelocity.y, vector_ZInput: 0);

            Vector3 smoothedVelocity = ((Movable)this).calculateSmoothedVelocity(this.playerbody.linearVelocity, targetVelocity, intermedVelMod);

            Debug.Log($"Player target lin vel: {smoothedVelocity}");

            this.setPlayerLinearVelocity(smoothedVelocity);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = this.passthrough ? Color.green : Color.red;
        // Set matrix to match the object's rotation and position
        Gizmos.matrix = transform.localToWorldMatrix;

        Debug.DrawLine(new Vector3(-5, this.footboxMinY, 0), new Vector3(5, this.footboxMinY, 0), Color.green);
        Debug.DrawLine(new Vector3(-5, this.objectHitMaxY, 0), new Vector3(5, this.objectHitMaxY, 0), Color.red);
    }
}