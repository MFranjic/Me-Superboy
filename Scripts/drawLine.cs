using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drawLine : MonoBehaviour {

    public int frameNum = 1;
    private Camera inputCamera;
    private GameObject spriteObject;
    private LineRenderer lineRenderer;
    private SpriteRenderer sprite;
    private Vector3 mousePosition;
    private List<Vector3> nodes = new List<Vector3>();
    private int frameCount = 0;
    private bool mousePressed;

    void Start()
    {
        lineRenderer = GameObject.Find("Line").GetComponent<LineRenderer>();
        inputCamera = GameObject.Find("Camera").GetComponent<Camera>();

        spriteObject = GameObject.Find("Color");
        sprite = spriteObject.GetComponent<SpriteRenderer>();

        mousePressed = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mousePressed = true;
            //Debug.Log("Mouse pressed");
        }           
        else if (Input.GetMouseButtonUp(0))
        {
            mousePressed = false;
            //Debug.Log("Mouse released");
        }

        //Debug.Log(mousePressed + "  " + frameCount);
        if(mousePressed)
        {
            if (frameCount == frameNum)
            {
                frameCount = -1;
                mousePosition = inputCamera.ScreenToWorldPoint(Input.mousePosition);
                mousePosition.z = 0;
                if (!nodes.Contains(mousePosition))
                {
                    nodes.Add(mousePosition);
                    Vector3[] nodesArray = nodes.ToArray();
                    lineRenderer.positionCount = nodesArray.Length;
                    lineRenderer.SetPositions(nodesArray);
                    Debug.Log(mousePosition);
                }
            }
            frameCount++;
        }
    }
}
