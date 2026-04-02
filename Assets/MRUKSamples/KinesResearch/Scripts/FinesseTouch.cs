using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinesseTouch : MonoBehaviour
{
    public List<GameObject> obstacles;

    // The local z value of the centimeter_height object position is the height offset in centimeters
    public GameObject centimeter_height;
    // The local z value of the milimeter_height object position is the height offset in milimeter
    public GameObject milimeter_height;
    // The local x value of the centimeter_width object position is the width offset in centimeters
    public GameObject centimeter_width;
    // The local x value of the milimeter_width object position is the width offset in milimeter
    public GameObject milimeter_width;

    private Transform untuned_position;
    private Transform tuned_position;

    private bool is_mm = false;
    private bool is_cm = true;

    //sets the poisiton of all the height and widths objects to 0,0,0
    public void ResetTuners()
    {
        centimeter_height.transform.localPosition = new Vector3(0, 0, 0);
        milimeter_height.transform.localPosition = new Vector3(0, 0, 0);
        centimeter_width.transform.localPosition = new Vector3(0, 0, 0);
        milimeter_width.transform.localPosition = new Vector3(0, 0, 0);
    }

    //sets the centimeter boolean true and the milimeter boolean false
    public void SetCentimeter()
    {
        is_cm = true;
        is_mm = false;
    }

    //sets the milimeter boolean true and the centimeter boolean false
    public void SetMilimeter()
    {
        is_mm = true;
        is_cm = false;
    }

    //wrapper function for the nudges up
    public void NudgeZUp()
    {
        if (is_cm)
        {
            NudgeZCentimeterUp();
        }
        else if (is_mm)
        {
            NudgeZMilimeterUp();
        }
        LevelObject();
    }

    //wrapper function for the nudges down
    public void NudgeZDown()
    {
        if (is_cm)
        {
            NudgeZCentimeterDown();
        }
        else if (is_mm)
        {
            NudgeZMilimeterDown();
        }
        LevelObject();
    }

    //wrapper function for the nudges positive y
    public void NudgeYUp()
    {
        if (is_cm)
        {
            NudgeYCentimeterUp();
        }
        else if (is_mm)
        {
            NudgeYMilimeterUp();
        }
        LevelObject();
    }

    //wrapper function for the nudges negative y
    public void NudgeYDown()
    {
        if (is_cm)
        {
            NudgeYCentimeterDown();
        }
        else if (is_mm)
        {
            NudgeYMilimeterDown();
        }
        LevelObject();
    }

    //wrapper function for the nudges positive x

    public void NudgeXUp()
    {
        if (is_cm)
        {
            NudgeXCentimeterUp();
        }
        else if (is_mm)
        {
            NudgeXMilimeterUp();
        }
        LevelObject();
    }

    //wrapper function for the nudges negative x
    public void NudgeXDown()
    {
        if (is_cm)
        {
            NudgeXCentimeterDown();
        }
        else if (is_mm)
        {
            NudgeXMilimeterDown();
        }
        LevelObject();
    }

    //wrapper function for the rotate positive y
    public void RotateYUp()
    {
        if (is_cm)
        {
            RotateYOneUp();
        }
        else if (is_mm)
        {
            RotateYUpSmall();
        }
        //LevelObject();
    }

    //wrapper function for the rotate negative y
    public void RotateYDown()
    {
        if (is_cm)
        {
            RotateYOneDown();
        }
        else if (is_mm)
        {
            RotateYDownSmall();
        }
        //LevelObject();
    }

    public void LevelObject()
    {
        //Sets the X and Z rotations of the object to zero
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                float y_rotation = obstacle.transform.rotation.eulerAngles.y;

                obstacle.transform.eulerAngles = new Vector3(0, y_rotation, 0);
            }
        }

    }


    //Nudges each of the obstacles objects up by 1 cm in the z direction
    private void NudgeZCentimeterUp()
    {
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                obstacle.transform.position = new Vector3(obstacle.transform.position.x, obstacle.transform.position.y, obstacle.transform.position.z + 0.01f);
            }
        }
    }

    //Nudges each of the obstacles objects down by 1 cm in the z direction
    private void NudgeZCentimeterDown()
    {
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                obstacle.transform.position = new Vector3(obstacle.transform.position.x, obstacle.transform.position.y, obstacle.transform.position.z - 0.01f);
            }
        }
    }

    //Nudges each of the obstacles objects up by 1 mm in the z direction
    private void NudgeZMilimeterUp()
    {
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                obstacle.transform.position = new Vector3(obstacle.transform.position.x, obstacle.transform.position.y, obstacle.transform.position.z + 0.001f);
            }
        }
    }

    //Nudges each of the obstacles objects down by 1 mm in the z direction
    private void NudgeZMilimeterDown()
    {
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                obstacle.transform.position = new Vector3(obstacle.transform.position.x, obstacle.transform.position.y, obstacle.transform.position.z - 0.001f);
            }
        }
    }

    //Nudges each of the obstacles objects up by 1 cm in the x direction
    private void NudgeXCentimeterUp()
    {
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                obstacle.transform.position = new Vector3(obstacle.transform.position.x + 0.01f, obstacle.transform.position.y, obstacle.transform.position.z);
            }
        }
    }

    //Nudges each of the obstacles objects down by 1 cm in the x direction
    private void NudgeXCentimeterDown()
    {
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                obstacle.transform.position = new Vector3(obstacle.transform.position.x - 0.01f, obstacle.transform.position.y, obstacle.transform.position.z);
            }
        }
    }

    //Nudges each of the obstacles objects up by 1 mm in the x direction
    private void NudgeXMilimeterUp()
    {
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                obstacle.transform.position = new Vector3(obstacle.transform.position.x + 0.001f, obstacle.transform.position.y, obstacle.transform.position.z);
            }
        }
    }

    //Nudges each of the obstacles objects down by 1 mm in the x direction
    private void NudgeXMilimeterDown()
    {
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                obstacle.transform.position = new Vector3(obstacle.transform.position.x - 0.001f, obstacle.transform.position.y, obstacle.transform.position.z);
            }
        }
    }

    //Nuudges each of the obstacles objects up by 1 cm in the y direction
    private void NudgeYCentimeterUp()
    {
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                obstacle.transform.position = new Vector3(obstacle.transform.position.x, obstacle.transform.position.y + 0.01f, obstacle.transform.position.z);
            }
        }
    }

    //Nudges each of the obstacles objects down by 1 cm in the y direction
    private void NudgeYCentimeterDown()
    {
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                obstacle.transform.position = new Vector3(obstacle.transform.position.x, obstacle.transform.position.y - 0.01f, obstacle.transform.position.z);
            }
        }
    }

    //Nudges each of the obstacles objects up by 1 mm in the y direction
    private void NudgeYMilimeterUp()
    {
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                obstacle.transform.position = new Vector3(obstacle.transform.position.x, obstacle.transform.position.y + 0.001f, obstacle.transform.position.z);
            }
        }
    }

    //Nudges each of the obstacles objects down by 1 mm in the y direction
    private void NudgeYMilimeterDown()
    {
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                obstacle.transform.position = new Vector3(obstacle.transform.position.x, obstacle.transform.position.y - 0.001f, obstacle.transform.position.z);
            }
        }
    }

    //Rotates the obstacles objects by 1 degree in the positive y direction
    private void RotateYOneUp()
    {
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                obstacle.transform.Rotate(0, 1, 0);
            }
        }
    }

    //Rotates the obstacles objects by .1 degree in the positive y direction
    private void RotateYUpSmall()
    {
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                obstacle.transform.Rotate(0, 0.1f, 0);
            }
        }
    }

    //Rotates the obstacles objects by 1 degree in the negative y direction
    private void RotateYOneDown()
    {
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                obstacle.transform.Rotate(0, -1, 0);
            }
        }
    }

    //Rotates the obstacles objects by .1 degree in the negative y direction
    private void RotateYDownSmall()
    {
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                obstacle.transform.Rotate(0, -0.1f, 0);
            }
        }
    }

    // The local z value of the centimeter_height object is added to the obstacle's y position
    public void UpdateHeightOffsetCentimeter()
    {
        float height_offset_centimeter = centimeter_height.transform.localPosition.z;

        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                obstacle.transform.position = new Vector3(obstacle.transform.position.x, obstacle.transform.position.y + height_offset_centimeter, obstacle.transform.position.z);
            }
        }
    }

    public void UpdateHeightOffsetMilimeter()
    {
        float height_offset_milimeter = milimeter_height.transform.localPosition.z / 10f;
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                obstacle.transform.position = new Vector3(obstacle.transform.position.x, obstacle.transform.position.y + height_offset_milimeter, obstacle.transform.position.z);
            }
        }
    }

    public void UpdateWidthOffsetCentimeter()
    {
        float width_offset_centimeter = centimeter_width.transform.localPosition.x;
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                obstacle.transform.position = new Vector3(obstacle.transform.position.x + width_offset_centimeter, obstacle.transform.position.y, obstacle.transform.position.z);
            }
        }
    }

    public void UpdateWidthOffsetMilimeter()
    {
        float width_offset_milimeter = milimeter_width.transform.localPosition.x / 10f;
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.activeSelf)
            {
                obstacle.transform.position = new Vector3(obstacle.transform.position.x + width_offset_milimeter, obstacle.transform.position.y, obstacle.transform.position.z);
            }
        }
    }
}
