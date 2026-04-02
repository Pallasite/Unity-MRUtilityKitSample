using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogTrainer : MonoBehaviour
{
    //This class will take the dog game object and move it a certain amount forward on the z axis. this will be using the lerp function to make it smooth. while the dog is moving a boolean will be set to true and when it gets to the end it will be set to false.

    public GameObject Dog = null;

    public float Distance = 1;

    public float Speed= 1;

    public bool Moving = false;

    private Vector3 Start_position { get; set; }
    private Vector3 Start_position_world { get; set; }

    // Animation controller for the dog
    public Animator DogAnimator;

    //The dog will move forward on the z axis by the distance variable using the lerp function over the time of speed
    public void MoveDog()
    {
        Start_position = Dog.transform.localPosition;

        Vector3 end_position = Start_position + new Vector3(0, 0, Distance);

        StartCoroutine(MoveDogCoroutine(end_position));
    }
    private IEnumerator MoveDogCoroutine(Vector3 end_position)
    {
        Moving = true;
        DogAnimator.SetBool("moving", Moving);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / Speed;
            Dog.transform.localPosition = Vector3.Lerp(Start_position, end_position, t);
            yield return null;
        }

        Moving = false;
        DogAnimator.SetBool("moving", Moving);
    }

    public void MoveDogWorld(Vector3 startpoint, Vector3 endpoint)
    {
        Start_position_world = startpoint;
        StartCoroutine(MoveDogWorldCoroutine(endpoint));
    }

    private IEnumerator MoveDogWorldCoroutine(Vector3 end_position)
    {
        Moving = true;
        DogAnimator.SetBool("moving", Moving);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / Speed;
            Dog.transform.position = Vector3.Lerp(Start_position_world, end_position, t);
            yield return null;
        }

        Moving = false;
        DogAnimator.SetBool("moving", Moving);
    }

    //The dog will be reset to 0,0,0
    public void ResetDog()
    {
        Dog.transform.localPosition = new Vector3(0, 0, 0);
    }
}
