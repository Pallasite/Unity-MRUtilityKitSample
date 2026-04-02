using UnityEngine;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;

public class DecoratorLaunch : MonoBehaviour
{
    public List<FindSpawnPositions> decorators;

    public void StartDecorating()
    {
        foreach (FindSpawnPositions decorator in decorators)
        {
            decorator.StartSpawn();
        }
    }
}
