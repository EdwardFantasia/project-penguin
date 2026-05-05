using Unity.VisualScripting;
using UnityEngine;

public class BoostScript : MonoBehaviour
{
    public CapsuleCollider capsuleCollider;
    public MeshRenderer meshRenderer;
    public PlayerScript playerScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake() {
        capsuleCollider = GetComponent<CapsuleCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
        this.disableBoostElements();
    }
    
    void Start()
    {
        playerScript = GetComponentInParent<PlayerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void disableBoostElements() { 
        this.capsuleCollider.enabled = false;
        this.meshRenderer.enabled = false;
    }

    public void enableBoostElements() {
        this.capsuleCollider.enabled = true;
        this.meshRenderer.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        print("footbox name: " + this.name);
        print("other name: " + other.name);
        if (this.capsuleCollider.enabled){
            playerScript.processTrigger(this.name, other);
        }

    }

    /*private void OnTriggerStay(Collider other)
    {
        playerScript.processTrigger(this.name, other);
    }*/
}
