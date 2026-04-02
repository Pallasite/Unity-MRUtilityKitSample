using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTouch : MonoBehaviour
{
    public GameObject obstacles;

    public GameObject centimeter_height;
    public GameObject milimeter_height;
    public GameObject centimeter_width;
    public GameObject milimeter_width;

    private Transform untuned_position;
    private Transform tuned_position;

    private bool is_mm = false;
    private bool is_cm = true;

    public void ResetTuners()
    {
        centimeter_height.transform.localPosition = new Vector3(0, 0, 0);
        milimeter_height.transform.localPosition = new Vector3(0, 0, 0);
        centimeter_width.transform.localPosition = new Vector3(0, 0, 0);
        milimeter_width.transform.localPosition = new Vector3(0, 0, 0);
    }

    public void SetCentimeter()
    {
        is_cm = true;
        is_mm = false;
    }

    public void SetMilimeter()
    {
        is_mm = true;
        is_cm = false;
    }

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
    }

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
    }

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
    }

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
    }

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
    }

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
    }

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
    }

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
    }

    public void LevelObject()
    {
        if (obstacles.activeSelf)
        {
            obstacles.transform.rotation = Quaternion.Euler(0, obstacles.transform.rotation.eulerAngles.y, 0);
        }
    }

    private void NudgeZCentimeterUp()
    {
        if (obstacles.activeSelf)
        {
            obstacles.transform.position = new Vector3(obstacles.transform.position.x, obstacles.transform.position.y, obstacles.transform.position.z + 0.01f);
        }
    }

    private void NudgeZCentimeterDown()
    {
        if (obstacles.activeSelf)
        {
            obstacles.transform.position = new Vector3(obstacles.transform.position.x, obstacles.transform.position.y, obstacles.transform.position.z - 0.01f);
        }
    }

    private void NudgeZMilimeterUp()
    {
        if (obstacles.activeSelf)
        {
            obstacles.transform.position = new Vector3(obstacles.transform.position.x, obstacles.transform.position.y, obstacles.transform.position.z + 0.001f);
        }
    }

    private void NudgeZMilimeterDown()
    {
        if (obstacles.activeSelf)
        {
            obstacles.transform.position = new Vector3(obstacles.transform.position.x, obstacles.transform.position.y, obstacles.transform.position.z - 0.001f);
        }
    }

    private void NudgeXCentimeterUp()
    {
        if (obstacles.activeSelf)
        {
            obstacles.transform.position = new Vector3(obstacles.transform.position.x + 0.01f, obstacles.transform.position.y, obstacles.transform.position.z);
        }
    }

    private void NudgeXCentimeterDown()
    {
        if (obstacles.activeSelf)
        {
            obstacles.transform.position = new Vector3(obstacles.transform.position.x - 0.01f, obstacles.transform.position.y, obstacles.transform.position.z);
        }
    }

    private void NudgeXMilimeterUp()
    {
        if (obstacles.activeSelf)
        {
            obstacles.transform.position = new Vector3(obstacles.transform.position.x + 0.001f, obstacles.transform.position.y, obstacles.transform.position.z);
        }
    }

    private void NudgeXMilimeterDown()
    {
        if (obstacles.activeSelf)
        {
            obstacles.transform.position = new Vector3(obstacles.transform.position.x - 0.001f, obstacles.transform.position.y, obstacles.transform.position.z);
        }
    }

    private void NudgeYCentimeterUp()
    {
        if (obstacles.activeSelf)
        {
            obstacles.transform.position = new Vector3(obstacles.transform.position.x, obstacles.transform.position.y + 0.01f, obstacles.transform.position.z);
        }
    }

    private void NudgeYCentimeterDown()
    {
        if (obstacles.activeSelf)
        {
            obstacles.transform.position = new Vector3(obstacles.transform.position.x, obstacles.transform.position.y - 0.01f, obstacles.transform.position.z);
        }
    }

    private void NudgeYMilimeterUp()
    {
        if (obstacles.activeSelf)
        {
            obstacles.transform.position = new Vector3(obstacles.transform.position.x, obstacles.transform.position.y + 0.001f, obstacles.transform.position.z);
        }
    }

    private void NudgeYMilimeterDown()
    {
        if (obstacles.activeSelf)
        {
            obstacles.transform.position = new Vector3(obstacles.transform.position.x, obstacles.transform.position.y - 0.001f, obstacles.transform.position.z);
        }
    }

    private void RotateYOneUp()
    {
        if (obstacles.activeSelf)
        {
            obstacles.transform.Rotate(0, 1, 0);
        }
    }

    private void RotateYUpSmall()
    {
        if (obstacles.activeSelf)
        {
            obstacles.transform.Rotate(0, 0.1f, 0);
        }
    }

    private void RotateYOneDown()
    {
        if (obstacles.activeSelf)
        {
            obstacles.transform.Rotate(0, -1, 0);
        }
    }

    private void RotateYDownSmall()
    {
        if (obstacles.activeSelf)
        {
            obstacles.transform.Rotate(0, -0.1f, 0);
        }
    }

    public void UpdateHeightOffsetCentimeter()
    {
        float height_offset_centimeter = centimeter_height.transform.localPosition.z;

        if (obstacles.activeSelf)
        {
            obstacles.transform.position = new Vector3(obstacles.transform.position.x, obstacles.transform.position.y + height_offset_centimeter, obstacles.transform.position.z);
        }
    }

    public void UpdateHeightOffsetMilimeter()
    {
        float height_offset_milimeter = milimeter_height.transform.localPosition.z / 10f;
        if (obstacles.activeSelf)
        {
            obstacles.transform.position = new Vector3(obstacles.transform.position.x, obstacles.transform.position.y + height_offset_milimeter, obstacles.transform.position.z);
        }
    }

    public void UpdateWidthOffsetCentimeter()
    {
        float width_offset_centimeter = centimeter_width.transform.localPosition.x;
        if (obstacles.activeSelf)
        {
            obstacles.transform.position = new Vector3(obstacles.transform.position.x + width_offset_centimeter, obstacles.transform.position.y, obstacles.transform.position.z);
        }
    }

    public void UpdateWidthOffsetMilimeter()
    {
        float width_offset_milimeter = milimeter_width.transform.localPosition.x / 10f;
        if (obstacles.activeSelf)
        {
            obstacles.transform.position = new Vector3(obstacles.transform.position.x + width_offset_milimeter, obstacles.transform.position.y, obstacles.transform.position.z);
        }
    }
}
