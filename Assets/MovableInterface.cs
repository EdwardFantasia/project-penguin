using System;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI;
interface Movable
{
    public Vector3 calculateTargetVelocity(float vector_XInput, float vector_YInput, float vector_ZInput)
    {
        return new Vector3(vector_XInput, vector_YInput, vector_ZInput); // rbForce is a normalized vector (between -1 and 1) and these vector values are multiplied by move speed to get the velocity of the player
    }

    public Vector3 calculateSmoothedVelocity(Vector3 currentVelocity, Vector3 targetVelocity, float intermedVelMod)
    {
        return Vector3.MoveTowards(
            currentVelocity, //current velocity
            new Vector3(targetVelocity.x, currentVelocity.y, targetVelocity.z), //target velocity and maintain currentVelocity (playerbody) y value
            intermedVelMod // value to create intermediate velocity with, (acceleration is dependent on seconds passed) 
        );
    }
}
