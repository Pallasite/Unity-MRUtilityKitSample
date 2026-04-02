using System.Collections.Generic;
using UnityEngine;

public class Aligner : MonoBehaviour
{
    // Public references to the calibration cubes and the obstacle

    // Start cubes form a start line for the walkable path
    public GameObject calibration_cube_start_one;
    public GameObject calibration_cube_start_two;

    // End cubes form an end line for the walkable path
    public GameObject calibration_cube_end_one;
    public GameObject calibration_cube_end_two;

    public List<GameObject> obstacles;

    // Public method to align the obstacle's rotation
    public void AlignObstacle()
    {
        if (calibration_cube_start_one == null || calibration_cube_end_one == null || obstacles == null)
        {
            Debug.LogError("Calibration cubes or obstacle list not assigned.");
            return;
        }

        foreach (GameObject obstacle in obstacles)
        {
            // Calculate the direction vector from the start cube to the end cube
            Vector3 direction = calibration_cube_end_one.transform.position - calibration_cube_start_one.transform.position;

            // Calculate the rotation to align the obstacle's forward vector along the direction
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

            // Level the X and Z rotations (set them to zero)
            Vector3 eulerAngles = targetRotation.eulerAngles;
            eulerAngles.x = 0f;
            eulerAngles.z = 0f;
            targetRotation = Quaternion.Euler(eulerAngles);

            // Apply the rotation to the obstacle without changing its position
            obstacle.transform.rotation = targetRotation;
        }
    }

    // A second method that now takes into account the second pair of calibration cubes, the cube one pairs and the cube two pairs will form two almost parallel lines. We want to take the average of the two lines to increase the precision of the alignment and apply that rotation to the obstacle
    public void AlignObstacleWithTwoPairs()
    {
        if (calibration_cube_start_one == null || calibration_cube_end_one == null || calibration_cube_start_two == null || calibration_cube_end_two == null || obstacles == null)
        {
            Debug.LogError("Calibration cubes or obstacle not assigned.");
            return;
        }

        foreach (GameObject obstacle in obstacles)
        {
            // Calculate the direction vector from the start cube to the end cube
            Vector3 direction_one = calibration_cube_end_one.transform.position - calibration_cube_start_one.transform.position;
            Vector3 direction_two = calibration_cube_end_two.transform.position - calibration_cube_start_two.transform.position;

            // Calculate the average direction vector
            Vector3 direction = (direction_one + direction_two) / 2f;

            // Calculate the rotation to align the obstacle's forward vector along the direction
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

            // Level the X and Z rotations (set them to zero)
            Vector3 eulerAngles = targetRotation.eulerAngles;
            eulerAngles.x = 0f;
            eulerAngles.z = 0f;
            targetRotation = Quaternion.Euler(eulerAngles);

            // Apply the rotation to the obstacle without changing its position
            obstacle.transform.rotation = targetRotation;
        }
    }
}
