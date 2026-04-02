using UnityEngine;

/// <summary>
/// Dog-specific obstacle behavior: smooth lerp movement with animation.
/// Attach this to the Dog obstacle to enable dog-specific movement.
/// </summary>
public class DogObstacleBehavior : MonoBehaviour, IObstacleBehavior
{
    [SerializeField]
    [Tooltip("Reference to the DogTrainer component that handles animation and movement")]
    private DogTrainer dog_trainer;

    private void Awake()
    {
        // Try to find DogTrainer if not assigned
        if (dog_trainer == null)
        {
            dog_trainer = GetComponent<DogTrainer>();
            if (dog_trainer == null)
            {
                dog_trainer = GetComponentInChildren<DogTrainer>();
            }
        }

        if (dog_trainer == null)
        {
            Debug.LogError("DogObstacleBehavior: No DogTrainer found. Please assign one in the inspector.");
        }
    }

    public void Move(Transform obstacle, Transform player, float distance, bool towards_user)
    {
        if (dog_trainer == null)
        {
            Debug.LogError("DogObstacleBehavior: Cannot move - DogTrainer is not assigned.");
            return;
        }

        // Determine if the player is in front or behind the object on the z-axis
        float direction = (player.position.z > obstacle.position.z) ? -1f : 1f;

        // Default is to move away from user; invert if towards_user is true
        if (towards_user)
        {
            direction = -direction;
        }

        // Calculate start and end positions for smooth movement
        Vector3 start_position = obstacle.position;
        Vector3 forward = obstacle.TransformDirection(Vector3.forward);
        Vector3 end_position = obstacle.position + forward * (distance * direction);

        Debug.Log("DogObstacleBehavior: Moving dog");
        dog_trainer.MoveDogWorld(start_position, end_position);
    }

    public void Reset(Transform obstacle)
    {
        if (dog_trainer == null)
        {
            Debug.LogError("DogObstacleBehavior: Cannot reset - DogTrainer is not assigned.");
            return;
        }

        dog_trainer.ResetDog();
    }
}
