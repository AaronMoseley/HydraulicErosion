using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Transform startPos;

    public Transform[] terrain1Pos;
    public Transform[] terrain2Pos;
    public Transform[] terrain3Pos;

    public float lerpFactor;
    public float marginOfError;

    private int focused = 0;
    private int index = 0;

    private Transform target;

    private GameObject cam;
    UIManager ui;


    void Start()
    {
        cam = Camera.main.gameObject;
        ui = gameObject.GetComponent<UIManager>();

        startPos.gameObject.transform.position = cam.transform.position;
        startPos.gameObject.transform.rotation = cam.transform.rotation;
    }

    private void FixedUpdate()
    {
        if (target != null)
        {
            Vector3 camPos = cam.transform.position;
            Vector3 camRot = cam.transform.rotation.eulerAngles;
            Vector3 newPos = new Vector3(Mathf.Lerp(camPos.x, target.position.x, lerpFactor), Mathf.Lerp(camPos.y, target.position.y, lerpFactor), Mathf.Lerp(camPos.z, target.position.z, lerpFactor));
            Vector3 newRotation = new Vector3(Mathf.Lerp(camRot.x, target.rotation.eulerAngles.x, lerpFactor), Mathf.Lerp(camRot.y, target.rotation.eulerAngles.y, lerpFactor), Mathf.Lerp(camRot.z, target.rotation.eulerAngles.z, lerpFactor));

            cam.transform.position = newPos;
            cam.transform.rotation = Quaternion.Euler(newRotation);

            if (Vector3.Distance(camPos, target.position) <= marginOfError)
            {
                cam.transform.position = target.position;
                cam.transform.rotation = target.rotation;

                target = null;
            }
        }
    }

    void Update()
    {
        if (!ui.UIFocused())
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Focus(1, index);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Focus(2, index);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Focus(3, index);
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                index = 0;
                Focus(0, -1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                index -= 2;
                if (index < 0)
                    index = 3 - (Mathf.Abs(index) - 1);
                if (index > 3)
                    index = 0 + (index - 4);
                Focus(focused, index);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                index++;
                if (index < 0)
                    index = 3 - (Mathf.Abs(index) - 1);
                if (index > 3)
                    index = 0 + (index - 4);
                Focus(focused, index);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                index += 2;
                if (index < 0)
                    index = 3 - (Mathf.Abs(index) - 1);
                if (index > 3)
                    index = 0 + (index - 4);
                Focus(focused, index);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                index--;
                if (index < 0)
                    index = 3 - (Mathf.Abs(index) - 1);
                if (index > 3)
                    index = 0 + (index - 4);
                Focus(focused, index);
            }
        }
    }

    void Focus(int terrainNum, int indexNum)
    {
        focused = terrainNum;

        switch(terrainNum)
        {
            case 0:
                target = startPos;
                break;
            case 1:
                target = terrain1Pos[indexNum];
                break;
            case 2:
                target = terrain2Pos[indexNum];
                break;
            case 3:
                target = terrain3Pos[indexNum];
                break;
        }

        if(Vector3.Distance(target.rotation.eulerAngles, cam.transform.rotation.eulerAngles) > 180)
        {
            cam.transform.rotation = Quaternion.Euler(cam.transform.rotation.eulerAngles.x, cam.transform.rotation.eulerAngles.y - 360, cam.transform.rotation.eulerAngles.z);
        }
    }
}
