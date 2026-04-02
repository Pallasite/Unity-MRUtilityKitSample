using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public Transform player; // Reference to the player's camera transform
    public float trigger_distance = 1f; // Distance at which the obstacle moves
    public float move_distance = .5f; // Distance the obstacle moves along the z-axis
    public float reset_distance = 3f; // Distance at which the obstacle resets
    public float occlusion_distance = 1f;

    public GameObject staff_of_science;

    public GameObject obstacle; // Reference to the obstacle object

    public GameObject obstacle_anchor; // Reference to the obstacle anchor object, this is the parent of the obstacle and will be used to reset the obstacle position in its local space

    public List<GameObject> obstacle_visuals;
    private int current_visual_index = 0;

    private Transform original_obstacle_transform; // Reference to the obstacle's transform
    private Vector3 original_obstacle_position; // Store the original position of the obstacle
    private Quaternion original_rotation;
    private bool has_moved = false; // Track if the obstacle has moved

    [SerializeField]
    private bool is_armed = false;
    private bool auto_reset = false;
    //some trials will not have the movement enabled, so this is a flag to check if the obstacle is active in the trial
    private bool active_in_trial = false;

    //Command Staff Visual indicators
    public GameObject active_indicator;
    public GameObject auto_indicator;

    public GameObject auto_and_active_indicator;

    public float time_since_move = 0f;

    public float time_buffer = 2f;

    public Material depth_occlusion_material = null;
    public Material no_occlusion_material = null;

    private Vector3 player_XZ_position;

    //public List<GameObject> diagnostic_visuals;
    public List<GameObject> setup_visual;

    public CSVLoader csv_loader;

    public bool trial_sequence_active = false;

    // Obstacle behavior abstraction - allows different obstacle types (default, dog, etc.)
    private IObstacleBehavior _obstacle_behavior;
    private DefaultObstacleBehavior _default_behavior;

    private float perturbation_distance;
    private bool move_towards_user;

    public void SetTrialData(bool active_in_trial, bool move_towards_user, float trigger_distance, float perturbation_distance)
    {
        this.active_in_trial = active_in_trial;
        this.move_towards_user = move_towards_user;
        this.perturbation_distance = perturbation_distance;
        this.trigger_distance = trigger_distance;
    }

    void Start()
    {
        player = Camera.main.transform;

        // Initialize default behavior (used when obstacle has no custom behavior)
        _default_behavior = gameObject.AddComponent<DefaultObstacleBehavior>();
        RefreshObstacleBehavior();
    }

    /// <summary>
    /// Refreshes the obstacle behavior reference. Call this when the obstacle changes.
    /// Checks if obstacle has a custom IObstacleBehavior, otherwise uses default.
    /// </summary>
    public void RefreshObstacleBehavior()
    {
        if (obstacle != null)
        {
            // Check for custom behavior on the obstacle (e.g., DogObstacleBehavior)
            _obstacle_behavior = obstacle.GetComponent<IObstacleBehavior>() ?? _default_behavior;
        }
        else
        {
            _obstacle_behavior = _default_behavior;
        }
    }

    void Update()
    {
        // Project the player's position onto the xz-plane at the obstacle's height
        player_XZ_position = new Vector3(player.position.x, obstacle.transform.position.y, player.position.z);

        //this case covers both the armed and moving and not moving cases, the occlusion needs to be updated in both cases because the player will step over the object. If it's not active in the trial it won't move though we will call it moved so the occlusion and auto reset work correctly
        if (is_armed && !has_moved)
        {
            // Calculate the distance between the player and the obstacle CURRENT position
            float current_distance_to_player = Vector3.Distance(player_XZ_position, obstacle.transform.position);

            // Check if the player is within the trigger distance and the obstacle hasn't moved yet
            if (current_distance_to_player <= trigger_distance)
            {
                // Move the obstacle along the z-axis away from the player
                if (active_in_trial)
                {
                    //MoveObstacle();
                    MoveObstacle(perturbation_distance, move_towards_user);

                }
                else has_moved = true; //if it's not active in the trial, we still need to set this to true so the occlusion is updated
            }
        }
        else if (is_armed && auto_reset)
        {
            auto_and_active_indicator.SetActive(true);

            //if more than buffer seconds have passed since the obstacle moved
            if (time_since_move > time_buffer)
            {
                // Calculate the distance between the player and the obstacle STARTING position
                float starting_distance_to_player = Vector3.Distance(player.position, original_obstacle_position);

                // Check if the player is outside the reset distance and the obstacle has moved
                if (starting_distance_to_player >= reset_distance && has_moved)
                {
                    // Reset the obstacle to its original position
                    ResetObstacle();

                    //DogTrainer.ResetDog();
                }

            }
            else
            {
                time_since_move += Time.deltaTime;
            }

        }
        else
        {
            auto_and_active_indicator.SetActive(false);
        }
    }



    public void ToggleExperimenterControls()
    {
        staff_of_science.SetActive(!staff_of_science.activeSelf);
    }


    // Set the obstacle gameobject as the next object in the obstacle_visuals list, if the end of the list is reached, start from the beginning, deactivate all the game objects in the list and activate the next one. every time the function is called it advances to the next object in the list
    public void SetNextObstacleVisual()
    {
        if (obstacle_visuals.Count == 0)
        {
            Debug.LogError("No obstacle visuals assigned.");
            return;
        }

        foreach (GameObject visual in obstacle_visuals)
        {
            visual.SetActive(false);
        }

        // Advance to the next index
        current_visual_index++;

        // If the end of the list is reached, start from the beginning
        if (current_visual_index >= obstacle_visuals.Count)
        {
            current_visual_index = 0;
        }

        // Set the obstacle GameObject as the next object in the list
        // obstacle = obstacle_visuals[current_visual_index];
        // not necessary now that we're using the obstacles parent object to move the obstacle

        GameObject obstacle_visual = obstacle_visuals[current_visual_index];

        obstacle_visual.SetActive(true);
    }

    public void SetObstacleOrigin()
    {
        SetObstacleOrigin(obstacle.transform);
    }

    // Set a new original position and rotation for the obstacle
    public void SetObstacleOrigin(Transform new_transform)
    {
        original_obstacle_position = new_transform.position;
        original_rotation = new_transform.rotation;
    }

    // Function to enable or disable the obstacle capability of movement based on a boolean value
    public void ArmObstacle(bool move)
    {
        is_armed = move;

        if (active_indicator != null)
        {
            active_indicator.SetActive(move);
        }
    }

    // Function to enable or disable the obstacle actvity in a trial based on a boolean value
    public void ObstacleMovesInTrial(bool active)
    {
        active_in_trial = active;
    }

    // function to toggle is_armed
    public void ToggleObstacleMovement()
    {
        is_armed = !is_armed;

        //I think this should be always true when we arm the obstacle, i think if it wasn't there we could hit the situation of a trial loading with the obstacle armed but not active, and then if the obstacle is armed but not active, the object won't move until another CSV file is loaded

        //active_in_trial = true; //problem of object moving on first trial when not supposed to

        if (active_indicator != null)
        {
            active_indicator.SetActive(is_armed);
        }

        if (is_armed)
        {
            //SetObstacleOrigin(obstacle.transform);
        }
    }

    public void ToggleHelperVisuals()
    {
        foreach (var visual in setup_visual)
        {
            visual.SetActive(!visual.activeSelf);
        }
    }

    public void ToggleSetupVisuals()
    {
        foreach (var visual in setup_visual)
        {
            visual.SetActive(!visual.activeSelf);
        }
    }

    // function to toggle auto_reset
    public void ToggleAutoReset()
    {
        auto_reset = !auto_reset;

        if (auto_indicator != null)
        {
            auto_indicator.SetActive(auto_reset);
        }
    }

    // Draw a gizmo to visualize the trigger distance
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(obstacle.transform.position, trigger_distance);
    }

    // Draw a gizmo to visualize the move distance
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(obstacle.transform.position, move_distance);
    }

    //Stores the obstacle's local transform relative to its obstacle_anchor
    public void StoreLocalObstacleTransform()
    {
        if (obstacle != null)
        {
            original_obstacle_position = obstacle.transform.localPosition;
            original_rotation = obstacle.transform.localRotation;
        }
        else
        {
            Debug.LogError("Obstacle or Obstacle Anchor is not assigned.");
        }
    }

    // Reset the obstacle to its original local position and rotation relative to its anchor
    public void ResetObstacle()
    {
        // Delegate to the behavior (handles dog animation, physics reset, etc.)
        _obstacle_behavior.Reset(obstacle.transform);

        has_moved = false;

        if (trial_sequence_active) AdvanceTrial();
    }



    public void AdvanceTrial()
    {
        csv_loader.GetNextTrial();
    }

    /// <summary>
    /// Move the obstacle using the default move_distance.
    /// </summary>
    public void MoveObstacle()
    {
        MoveObstacle(move_distance, false);
    }

    /// <summary>
    /// Move obstacle along the z-axis by the specified distance.
    /// </summary>
    /// <param name="trial_distance">Distance to move (from trial CSV)</param>
    /// <param name="towards_user">If true, move towards user; if false, move away</param>
    public void MoveObstacle(float trial_distance, bool towards_user)
    {
        // Delegate movement to the behavior (handles dog animation, instant move, etc.)
        _obstacle_behavior.Move(obstacle.transform, player, trial_distance, towards_user);

        has_moved = true;
        time_since_move = 0f;
    }

    public void ToggleTrialSequenceActive()
    {
        trial_sequence_active = !trial_sequence_active;
    }
}
