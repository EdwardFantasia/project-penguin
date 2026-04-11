using UnityEngine;

public class EnemyScript : MonoBehaviour, Movable
{
    public bool isPacing;
    public bool canMove;
    public Vector3 minVec;
    public Vector3 maxVec;
    public float moveSpeed;
    public Rigidbody enemyBody;
    public BoxCollider enemyFrictBox;
    public BoxCollider enemyHurtBox;
    public BoxCollider enemyHitBox;
    private string enemyType;
    private int xDirection;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start(){
        this.enemyType = this.gameObject.tag;
    }

    // Update is called once per frame
    void Update(){
        if((enemyFrictBox.bounds.min.x <= this.minVec.x) || (enemyFrictBox.bounds.max.x >= this.maxVec.x)) { //flip direction of enemy upon reaching of bounds
            this.xDirection = xDirection * -1;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }

    public void setPaceFields(Collision collision){
        if (collision.gameObject.tag == "Platform") {
            this.minVec = collision.collider.bounds.min;
            this.maxVec = collision.collider.bounds.max;
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
         
    }

}
