using System;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI;

//TODO: need to implement some sort of "turnaround" drag where quickly flicking from left to right or vice versa
//TODO: implement jumping (tmp done, some tweaking may need to be done to adjust via how long space is held)
//TODO: implement grapple
//TODO: implement slide (tmp done)
//TODO: implement stomp (tmp done)
//TODO: implement boost (tmp done)
//TODO: implement chain attack
//TODO: implement wall jump implementation

//TODO: implement camera toggle between perspective and orthographic (strictly 2D) camera
//TODO: implement camera movement when character is no longer within frame

public class PlayerScript : MonoBehaviour
{
    public Rigidbody playerbody;
    private PlayerInputActions playerInputActions;
    
    private bool canJump;
    private bool isMidair;
    private bool isStomping;
    private bool isSliding;
    private bool isBoosting;

    private Vector2 rbForce;

    public float moveSpeed = 13.5f;
    public float acceleration = 14f;
    public float slideDrag = 12f;
    public float jumpForce = 7.5f;
    public float stompForce = 8f;
    public float boostSpeedMult = 1.75f;
    public float boostAccelMult = 1.25f;
    public float curBoostResource; //can be restored through destruction of enemies
    public float origBoostMax = 100f; //can be increased through collection of collectibles in game
    public int curCollectibles;
    /*public float nonBoostMax = 100f;
    public float boostMax = 250f;*/

    void Awake() {
        this.playerInputActions = new PlayerInputActions();
        this.curCollectibles = 0;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.playerbody = GetComponent<Rigidbody>();
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
    }
    private void OnSlideHoldStart(InputAction.CallbackContext context) { //pressing and holding slide cancels boost and slides
        this.isBoosting = false;
        this.isSliding = true;
        if (this.isMidair){ //can stomp while midair
            this.isStomping = true;
        }
    }

    private void OnSlideHoldEnd(InputAction.CallbackContext context) { //no longer holding slide cancels slide
        this.isSliding = false;
    }
    private void OnBoostHoldStart(InputAction.CallbackContext context) { //pressing and holding boost activates boost and deactivates sliding and stomping
        this.isBoosting = true;
        this.isSliding = false;
        this.isStomping = false;
    }

    private void OnBoostHoldEnd(InputAction.CallbackContext context) { //no longer holding boost cancels boost
        this.isBoosting = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.isStomping) { //placing this here fixed the bug where player model would clip into floor when stomping
            this.playerbody.AddForce(Vector2.down * (this.stompForce / 2), ForceMode.Impulse);
            this.setPlayerLinearVelocity(new Vector3(0, this.playerbody.linearVelocity.y, 0));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        print("test collision");
    }

    public void processTrigger(string triggerName, Collider otherCollisionData) {
        if(triggerName == "Footbox") {
            if (otherCollisionData.name == "Ground") {
                bool wasStomping = this.isStomping;
                this.isStomping = false;
                print("footbox touched ground");
                /*TODO: X-axis movement is impacted too negatively, decrease how much player "stops" on landing, include setting of playerVel here*/
                this.canJump = true;
                this.isMidair = false;
            }
        }
    }

    void OnMove2(InputValue value) {
        this.rbForce = value.Get<Vector2>();
        this.rbForce.y = 0; //IMPORTANT: used to remove any influence pressing up on control stick has on player while jumping, may need to remove in order to do a move in the future
    }

    void OnJump() {
        if (canJump && !isMidair) {
            this.playerbody.AddForce(Vector2.up * jumpForce, ForceMode.Impulse);
            this.isMidair = true;
            this.canJump = false;
        }
    }

    public void setCanJump(bool canJumpValue) { //for objects like enemies to call
        this.canJump = canJumpValue;
    }

    public Vector3 calculatePlayerTargetVelocity(float vector_XInput, float vector_YInput, float vector_ZInput) {
        return new Vector3(vector_XInput, vector_YInput, vector_ZInput); // rbForce is a normalized vector (between -1 and 1) and these vector values are multiplied by move speed to get the velocity of the player
    }

    public Vector3 calculatePlayerSmoothedVelocity(Vector3 currentVelocity, Vector3 targetVelocity, float intermedVelMod) {
        return Vector3.MoveTowards(
            currentVelocity, //current velocity
            new Vector3(targetVelocity.x, currentVelocity.y, targetVelocity.z), //target velocity and maintain currentVelocity (playerbody) y value
            intermedVelMod // value to create intermediate velocity with, (acceleration is dependent on seconds passed) 
        );
    }

    public void setPlayerLinearVelocity(Vector3 smoothedVelocity) {
        this.playerbody.linearVelocity = smoothedVelocity; // set new velocity with acceleration applied (smoothed velocity) to player's linear velocity
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

            Vector3 targetVelocity = calculatePlayerTargetVelocity(vector_XInput: vectorX, vector_YInput: this.playerbody.linearVelocity.y, vector_ZInput: 0);

            Vector3 smoothedVelocity = calculatePlayerSmoothedVelocity(this.playerbody.linearVelocity, targetVelocity, intermedVelMod);

            Debug.Log($"Player target lin vel: {smoothedVelocity}");

            this.setPlayerLinearVelocity(smoothedVelocity);
        }
    }
}