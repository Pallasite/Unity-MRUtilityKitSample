using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialCounter : MonoBehaviour
{
    //reference to the textmeshpro component on this object
    public List<TMPro.TextMeshPro> trial_counter_text;

    // ??? Not sure why this caused a compiler error
    // private bool active_trial = false;

    //reference to the CSVLoader script on this object
    public CSVLoader csv_loader;

    // reference to the ObstacleManager script on this object
    public ObstacleManager obstacle_manager;

    private void Start()
    {
        //set the reference to the CSVLoader script on this object
        if (csv_loader == null) csv_loader = FindFirstObjectByType<CSVLoader>();

        //set the reference to the ObstacleManager script on this object
        if (obstacle_manager == null) obstacle_manager = FindFirstObjectByType<ObstacleManager>();


        // find all the game objects in the scene with the tag "TrialNumber" and add their TMPro component to the list
        //if (trial_counter_text.Count == 0)
        {
            trial_counter_text = new List<TMPro.TextMeshPro>();

            Object[] found_text_mesh_objects = FindObjectsByType(typeof(TMPro.TextMeshPro), FindObjectsInactive.Include, FindObjectsSortMode.None);

            //add the TMPro component of each object to the list trial_counter_objects, if they have the tag "TrialNumber"
            foreach (Object obj in found_text_mesh_objects)
            {
                if ((obj as TMPro.TextMeshPro).tag == "TrialNumber")
                {
                    trial_counter_text.Add(obj as TMPro.TextMeshPro);
                }
            }

        }

        //wait for a half a second
        // start the coroutine that updates the text
        StartCoroutine(UpdateText());
    }

    //couroutine that prints a message to the text panel every quarter second
    IEnumerator UpdateText()
    {
        while (true)
        {
            //check if csv_loader is not null before accessing its properties
            if (csv_loader != null)
            {

                //set the text to the current trial number
                foreach (var text in trial_counter_text)
                {
                    if (text.text != null) text.text = csv_loader.current_trial_number.ToString();
                }

                if (obstacle_manager != null)
                {
                    // if the trial sequence is active, change the text color to green, if not, change it to red, if no data exsists the number will be magenta
                    foreach (var text in trial_counter_text)
                    {
                        if (obstacle_manager.trial_sequence_active)
                        {
                            text.color = Color.green;
                        }
                        else
                        {
                            text.color = Color.red;
                        }

                        if (csv_loader.missing_trial_data)
                        {
                            text.color = Color.magenta;
                        }
                    }
                }
            }
            yield return new WaitForSeconds(0.25f);
        }
    }

    //
}
