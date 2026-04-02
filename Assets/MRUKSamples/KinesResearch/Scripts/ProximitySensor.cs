using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximitySensor : MonoBehaviour
{
    public GameObject player;
    [SerializeField] List<GameObject> managed_objects;
    [SerializeField] GameObject bounds_marker;
    [SerializeField] private float world_z_boundary;
    [SerializeField] private bool z_forward = true;


    [SerializeField] private float distance_threshold = 5.0f;
    [SerializeField] private float sensor_update_interval = 0.10f;

    public bool player_based_distance = false;

    // Start is called before the first frame update
    void Awake()
    {
        // the main camera is the player
        player = Camera.main.gameObject;

        StartCoroutine(ProximityCheckCoroutine());
    }

    IEnumerator ProximityCheckCoroutine()
    {
        while (true)
        {
            if (player_based_distance) ProximityCheck();
            else BoundaryCheck();

            yield return new WaitForSeconds(sensor_update_interval);
        }
    }

    public void ProximityCheck()
    {
        foreach (GameObject obj in managed_objects)
        {
            if (Vector3.Distance(player.transform.position, obj.transform.position) < distance_threshold)
            {
                obj.SetActive(true);
            }
            else
            {
                obj.SetActive(false);
            }
        }
    }

    //WARNING MAGIC NUMBERS BE HERE
    public void BoundaryCheck()
    {
        foreach (GameObject obj in managed_objects)
        {
            if (z_forward)
            {
                if (player.transform.position.z > world_z_boundary)
                {
                    obj.SetActive(true);
                }
                else
                {
                    obj.SetActive(false);
                }
            }
            else
            {
                if (player.transform.position.z < world_z_boundary)
                {
                    obj.SetActive(true);
                }
                else
                {
                    obj.SetActive(false);
                }
            }
        }
    }
}
