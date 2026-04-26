using UnityEngine;

public class JumpBox : MonoBehaviour
{
    public BoxCollider jumpbox;
    public PlayerScript playerScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        jumpbox = GetComponent<BoxCollider>();
        playerScript = GetComponentInParent<PlayerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        print("footbox name: " + this.name);
        print("other name: " + other.name);
        playerScript.processTrigger(this.name, other);

    }
}
