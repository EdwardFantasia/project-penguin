using System;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI;

interface DestroyableParent
{
    public void DestroyParentObject(Rigidbody rb)
    {
        GameObject rbGameObject = rb.gameObject;
        DestructionManager.destructionManagerInstance.destroyTheChildren(rb);
    }
}
