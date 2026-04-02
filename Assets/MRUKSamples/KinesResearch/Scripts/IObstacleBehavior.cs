using UnityEngine;

/// <summary>
/// Interface for custom obstacle movement and reset behaviors.
/// Implement this on any obstacle that needs non-standard behavior.
/// </summary>
public interface IObstacleBehavior
{
    /// <summary>
    /// Move the obstacle based on trial parameters.
    /// </summary>
    /// <param name="obstacle">The obstacle transform to move</param>
    /// <param name="player">The player/camera transform for direction calculation</param>
    /// <param name="distance">Distance to move (from trial CSV)</param>
    /// <param name="towards_user">If true, move towards player; if false, move away</param>
    void Move(Transform obstacle, Transform player, float distance, bool towards_user);

    /// <summary>
    /// Reset the obstacle to its starting position.
    /// </summary>
    /// <param name="obstacle">The obstacle transform to reset</param>
    void Reset(Transform obstacle);
}
