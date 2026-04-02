using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSVLoader : MonoBehaviour
{
    private string file_path;
    private Dictionary<int, TrialCondition> trial_data; // Data structure to hold trial conditions

    private ObstacleManager obstacle_manager;

    public TMPro.TextMeshPro trial_loading_status;

    private int total_trials = 0;
    private int _current_trial = 0;

    public bool missing_trial_data = false;

    public int current_trial_number
    {
        get { return _current_trial; }
        set { _current_trial = value; }
    }

    void Start()
    {
        obstacle_manager = FindFirstObjectByType<ObstacleManager>();

        if (obstacle_manager == null)
        {
            Debug.LogError("ObstacleManager not found in the scene.");
        }

        trial_data = new Dictionary<int, TrialCondition>();

        // Set the file path
        file_path = Path.Combine(Application.persistentDataPath, "trial_conditions.csv");

        TryLoadingCSV();

        LoadTrial(current_trial_number);
    }

    public void TryLoadingCSV()
    {
        if (File.Exists(file_path))
        {
            string csv_data = File.ReadAllText(file_path);
            ParseCSV(csv_data);
            // Set the text in trial_loading_status to the total number of trials and the file path
            if (trial_loading_status != null)
            {
                trial_loading_status.text = $"Total Trials: {total_trials}\n{file_path}";
            }
        }
        else
        {
            Debug.LogError($"CSV file not found at: {file_path}");
            if (trial_loading_status != null)
            {
                trial_loading_status.text = $"CSV file not found at: {file_path}";
            }
        }
    }

    // The CSV will have 5 elements per line, separated by a comma:
    // a trial number, boolean value for obstacle_movement, boolean for movement direction,
    // float for movement trigger distance, and float for perturbation distance
    void ParseCSV(string csv_data)
    {
        string[] lines = csv_data.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            string[] values = line.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (values.Length >= 5)
            {
                if (int.TryParse(values[0], out int trial_number) &&
                    bool.TryParse(values[1], out bool is_active) &&
                    bool.TryParse(values[2], out bool move_with_user) &&
                    float.TryParse(values[3], out float move_trigger_distance) &&
                    float.TryParse(values[4], out float perturbation_distance))
                {
                    trial_data[trial_number] = new TrialCondition
                    {
                        IsActive = is_active,
                        MoveWithUser = move_with_user,
                        MoveTriggerDistance = move_trigger_distance,
                        PerturbationDistance = perturbation_distance
                    };

                    total_trials++;
                }
                else
                {
                    Debug.LogWarning($"Invalid data on line: {line}");
                }
            }
            else
            {
                Debug.LogWarning($"Incomplete data on line: {line}");
            }
        }
    }

    // Function to load a trial based on the trial number, only is loading the activity state right now, needs to add distance for trigger and movement
    private void LoadTrial(int trial_number)
    {
        _current_trial = trial_number;

        if (trial_data.TryGetValue(_current_trial, out TrialCondition condition))
        {
            if (obstacle_manager != null)
            {
                obstacle_manager.SetTrialData(condition.IsActive, condition.MoveWithUser, condition.MoveTriggerDistance, condition.PerturbationDistance);
            }

            string condition_text = condition.IsActive ? "Active" : "Inactive";

            if (trial_loading_status != null)
            {
                trial_loading_status.text = $"Trial: {_current_trial}, obstacle is {condition_text}";
            }

            missing_trial_data = false;
        }
        else
        {
            if (trial_loading_status != null)
            {
                trial_loading_status.text = $"No data for trial {_current_trial}";
            }

            missing_trial_data = true;
        }
    }

    // Function to get the next trial
    public void GetNextTrial()
    {
        LoadTrial(_current_trial + 1);
    }

    // Function to get the previous trial
    public void GetPreviousTrial()
    {
        LoadTrial(_current_trial - 1);
    }

    // Function to enable or disable the obstacle movement based on a boolean value
    public void SetObstacleMovementActive(bool move)
    {
        if (obstacle_manager != null)
        {
            obstacle_manager.ArmObstacle(move);
        }
    }
}

public class TrialCondition
{
    public bool IsActive { get; set; }
    public bool MoveWithUser { get; set; }
    public float MoveTriggerDistance { get; set; }
    public float PerturbationDistance { get; set; }
}
