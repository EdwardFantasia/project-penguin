using Unity.VisualScripting;
using UnityEngine;

public class BoostScript : MonoBehaviour
{
    public CapsuleCollider capsuleCollider;
    public MeshRenderer meshRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake() {
        capsuleCollider = GetComponent<CapsuleCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
        this.disableBoostElements();
    }
    
    void Start()
    {
        
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
}
