using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Summon : MonoBehaviour
{
    public List<GameObject> objects_to_summon;
    public GameObject summon_attractor;

    //objects in the list are attracted to the attractor and move towards it and stop 50cm away from it
    public void SummonObjects()
    {
        foreach (GameObject obj in objects_to_summon)
        {
            Vector3 direction = summon_attractor.transform.position - obj.transform.position;
            float distance = direction.magnitude;
            if (distance > 0.5f)
            {
                //This is on a frame step basis, so it won't move a lot, but getting extra loops will make it move faster
                for (int i = 0; i < 5; i++)
                {
                    obj.transform.position += direction.normalized * Time.deltaTime;
                }
            }
        }
    }
}
