using System.Threading;
using UnityEngine;

public class EnemyScript : MonoBehaviour, Movable, DestroyableParent
{
    public bool isPacing;
    public bool cannotMove;

    public Vector3 minVec;
    public Vector3 maxVec;
    public float moveSpeed = 7.5f;
    private string enemyType;
    private int xDirection;
    private float lastBound;
    public float acceleration = 10f;

    public Rigidbody enemyBody;
    public BoxCollider enemyFrictBox;
    public BoxCollider enemyHurtBox;
    public BoxCollider enemyHitBox;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start(){
        this.enemyType = this.gameObject.tag;
        if (this.enemyBody.rotation.y == -90){
            this.xDirection = -1;
        }
        else {
            this.xDirection = 1;
        }
    }

    // Update is called once per frame
    void Update(){
        if (this.isPacing){ //if pacing...
            float? curBound = null;
            if (this.enemyFrictBox.bounds.min.x <= (this.minVec.x + .3f)) {
                curBound = this.minVec.x;
            }
            else if (this.enemyFrictBox.bounds.max.x >= (this.maxVec.x - .2f)) {
                curBound = this.maxVec.x;
            }

            if (curBound.HasValue) {
                if(this.lastBound != curBound) {
                    this.lastBound = (float)curBound;
                    this.xDirection = this.xDirection * -1;
                    this.enemyBody.rotation = Quaternion.Euler(0, this.xDirection * 90, 0);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (this.isPacing) {
            this.setPaceFields(collision);
            this.enemyBody.rotation = Quaternion.Euler(0, this.xDirection * 90, 0);
        }
    }

    public void setPaceFields(Collision collision){
        if (collision.gameObject.tag == "Platform") {
            this.minVec = collision.collider.bounds.min;
            //this.minVec.x += .1f;
            this.maxVec = collision.collider.bounds.max;
            //this.maxVec.x -= .1f;
        }
    }

    public float pace(){
        //if minXVec, get the min of the (difference btwn curX and either min or max X vec depending on facing direction) and (move speed) then use that X value in the linVel calcs
        float enemyFrictBoxX = this.xDirection == -1 ? this.enemyFrictBox.bounds.min.x : this.enemyFrictBox.bounds.max.x;
        float groundXBound = this.xDirection == -1 ? this.minVec.x : this.maxVec.x;
        float remainingDist = this.xDirection == -1 ? (enemyFrictBoxX - groundXBound) : (groundXBound - enemyFrictBoxX); //if going left, get difference of frict x - groundXBound, if going right, get difference of groundXBound - frict x
        return Mathf.Min(remainingDist, this.moveSpeed);
    }

    void FixedUpdate() //used for physics changes
    {
        if (!this.cannotMove) {
            this.enemyBody.AddForce(new Vector3(this.xDirection, 0, 0));

            float forwardVel = this.isPacing ? this.pace() : this.moveSpeed;

            Vector3 targetVelocity = ((Movable)this).calculateTargetVelocity(vector_XInput: forwardVel * this.xDirection, vector_YInput: this.enemyBody.linearVelocity.y, vector_ZInput:0);

            Vector3 smoothedVelocity = ((Movable)this).calculateSmoothedVelocity(this.enemyBody.linearVelocity, targetVelocity, this.acceleration * Time.deltaTime);
            
            Debug.Log($"Player target lin vel: {smoothedVelocity}");

            this.setEnemyLinearVelocity(smoothedVelocity);
        }
    }

    public void setEnemyLinearVelocity(Vector3 linearVelocity) {
        this.enemyBody.linearVelocity = linearVelocity;
    }
}
