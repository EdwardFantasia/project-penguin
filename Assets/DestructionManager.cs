using System;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI;
public sealed class DestructionManager : UnityEngine.Object
{
    private static readonly Lazy<DestructionManager> _lazy =
        new Lazy<DestructionManager>(() => new DestructionManager());

    public static DestructionManager destructionManagerInstance => _lazy.Value;

    private DestructionManager() { }

    public void destroyTheChildren(Rigidbody rb) {
        foreach (Transform child in rb.gameObject.GetComponentsInChildren<Transform>())
        {
            Debug.Log($"child name: {child.name}");
            Destroy(child.gameObject);
        }
    }
}
