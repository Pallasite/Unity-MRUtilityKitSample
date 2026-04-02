using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Default obstacle behavior: instant position change along Z-axis.
/// Used when no custom IObstacleBehavior is attached to an obstacle.
/// </summary>
public class DefaultObstacleBehavior : MonoBehaviour, IObstacleBehavior
{
    public void Move(Transform obstacle, Transform player, float distance, bool towards_user)
    {
        // Determine if the player is in front or behind the object on the z-axis
        float direction = (player.position.z > obstacle.position.z) ? -1f : 1f;

        // Default is to move away from user; invert if towards_user is true
        if (towards_user)
        {
            direction = -direction;
        }

        // Move along the obstacle's forward direction
        Vector3 forward = obstacle.TransformDirection(Vector3.forward);
        obstacle.position += forward * (distance * direction);
    }

    public void Reset(Transform obstacle)
    {
        // Stop any physics on child rigidbodies
        List<Rigidbody> child_physics = new List<Rigidbody>(obstacle.GetComponentsInChildren<Rigidbody>());
        foreach (var rigid_body in child_physics)
        {
            rigid_body.angularVelocity = Vector3.zero;
            rigid_body.linearVelocity = Vector3.zero;
        }

        // Reset to local origin (relative to parent anchor)
        obstacle.localPosition = Vector3.zero;
    }
}
