using UnityEngine;

public class HurtBox : MonoBehaviour
{
    public BoxCollider hurtbox;
    public PlayerScript playerScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hurtbox = GetComponent<BoxCollider>();
        playerScript = GetComponentInParent<PlayerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        print("test collision");
    }
}
