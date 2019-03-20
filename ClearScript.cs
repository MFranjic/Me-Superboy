using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ClearScript : MonoBehaviour {

    public GameObject tile;
    public int frameNum = 0;
    public bool showBackground = true;

    public int moveX = 0;
    public int moveY = 0;
    public Button resetButton;
    public Text labels;
    public int drawingNumber;
    private GameObject[,] grid = new GameObject[16, 32];
    private bool[,] visited = new bool[16, 32];
    private Camera inputCamera;
    private int lineNum;
    private GameObject spriteObject;
    private LineRenderer lineRenderer;
    private LinkedList<LineRenderer> lines = new LinkedList<LineRenderer>();
    private Vector3 mousePosition;
    private List<Vector3> nodes = new List<Vector3>();
    private int frameCount = 0;
    private bool mousePressed = false;
    private Color backgroundColor;
    private int xMax = 0;
    private int xMin = 32;
    private int yMax = 0;
    private int yMin = 16;
    private Text tempScale;
    private Text tempSquare;
    private Text tempCircle;
    private Text tempWhatIsIt;
    private Button tempButton;
    private int num;

    // Use this for initialization
    void Start()
    {
        inputCamera = GameObject.Find("Camera").GetComponent<Camera>();

        //spriteObject = GameObject.Find("Color");
        backgroundColor = new Color(0, 0, 0, 0);
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                //GameObject newTile = new GameObject("Color " + x + ", " + y);
                GameObject newTile = Instantiate(tile, new Vector3(moveX + y, moveY + x, 1), Quaternion.identity);
                newTile.transform.parent = GameObject.Find("ClearingRenderer").GetComponent<Transform>();
                newTile.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
                grid[x, y] = newTile;
                visited[x, y] = false;
            }
        }

        Image puddle = GameObject.Find("Canvas").GetComponent<Transform>().Find("PuddleImage").GetComponent<Image>();
        puddle.color = new Color32(255, 255, 255, 225);

        tempButton = Instantiate(resetButton, new Vector3(0, 0, 1), Quaternion.identity) as Button;
        tempButton.name = "CloseButton" + drawingNumber;
        tempButton.transform.parent = GameObject.Find("Canvas").GetComponent<Transform>();
        tempButton.transform.localScale = new Vector3((float)0.3, (float)0.3, 0);
        tempButton.transform.localPosition = new Vector3(-140 + (float)5.5 * moveX, -55 + (float)5.5 * moveY, 0);
        tempButton.onClick.AddListener(TaskOnClick);
    }

    // Update is called once per frame
    void Update()
    {
        mousePosition = inputCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        if (mousePosition.x > moveX && mousePosition.x < moveX + 32 && mousePosition.y > moveY && mousePosition.y < moveY + 16)
        {
            if (Input.GetMouseButtonDown(0))
            {
                mousePressed = true;
                lineNum++;
                GameObject lineObject = new GameObject("NewLine" + lineNum);
                lineRenderer = lineObject.AddComponent<LineRenderer>();
                lineRenderer.startColor = new Color32(255, 221, 135, 200);
                lineRenderer.endColor = new Color32(255, 221, 135, 100);
                lineRenderer.startWidth = (float)2;
                lineRenderer.endWidth = (float)2;
                lineRenderer.numCapVertices = 20;
                lineRenderer.numCornerVertices = 20;
                lineRenderer.material = new Material(Shader.Find("Particles/Alpha Blended"));
                lineRenderer.transform.parent = GameObject.Find("ClearingRenderer").GetComponent<Transform>();
                lines.AddLast(lineRenderer);

                nodes.Clear();
                Debug.Log("Mouse pressed");
            }
            else if (Input.GetMouseButtonUp(0))
            {
                mousePressed = false;
                Debug.Log("Mouse released");
            }
        }
        else if (mousePressed)/*if (Input.GetMouseButtonUp(0))*/
        {
            mousePressed = false;
            Debug.Log("Mouse released");
        }

        //Debug.Log(mousePressed + "  " + frameCount);
        if (mousePressed)
        {
            if (frameCount == frameNum)
            {
                frameCount = -1;
                if (mousePosition.x > xMax)
                    xMax = (int)mousePosition.x;
                if (mousePosition.x < xMin)
                    xMin = (int)mousePosition.x;
                if (mousePosition.y > yMax)
                    yMax = (int)mousePosition.y;
                if (mousePosition.y < yMin)
                    yMin = (int)mousePosition.y;
                int x = xMax - xMin;
                int y = yMax - yMin;
                //tempScale.text = x + ", " + y;
                //Debug.Log(x + ", " + y);
                //Debug.Log(xMax + ",  " + yMax + ",  " + xMin + ",  " + yMin);
                if (!nodes.Contains(mousePosition))
                {
                    nodes.Add(mousePosition);
                    Vector3[] nodesArray = nodes.ToArray();
                    lineRenderer.positionCount = nodesArray.Length;
                    lineRenderer.SetPositions(nodesArray);
                    //Debug.Log(mousePosition);
                }
            }
            frameCount++;
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    if (insideSprite(mousePosition, grid[x, y]))
                    {
                        //Debug.Log(x + " " + y);
                        if (!visited[x, y])
                        {
                            visited[x, y] = true;
                            num++;
                            //tempCircle.text = "Circle: " + avgC;
                            //tempSquare.text = "Square: " + avgS;
                        }


                    }
                }
            }
        }



    }

    bool insideSprite(Vector3 position, GameObject sprite)
    {
        if (position.x > sprite.transform.position.x && position.y > sprite.transform.position.y)
            if (position.x < sprite.transform.position.x + 1 && position.y < sprite.transform.position.y + 1)
                return true;
        return false;
    }

    public void TaskOnClick()
    {
        Debug.Log("CLicked!");
        foreach (LineRenderer line in lines)
        {
            Destroy(line);
        }
        foreach (GameObject sprite in grid)
        {
            sprite.GetComponent<SpriteRenderer>().color = backgroundColor;
        }
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                visited[x, y] = false;
            }
        }
        lineNum = 0;
        num = 0;
        frameCount = 0;
        mousePressed = false;
        xMax = 0;
        xMin = 32;
        yMax = 0;
        yMin = 16;
        //tempScale.text = "x, y";
        //tempCircle.text = "circle avg";
        //tempSquare.text = "square avg";
        //tempWhatIsIt.text = "What could go here?";
    }

    public void DisableClearing()
    {
        foreach (GameObject tile in grid)
        {
            tile.GetComponent<SpriteRenderer>().enabled = false;
        }
        //tempButton.enabled = false;
        //tempButton.GetComponent<Image>().enabled = false;
        GameObject button = GameObject.Find("CloseButton" + drawingNumber);
        button.SetActive(false);
        //tempWhatIsIt.enabled = false;

        Image puddle = GameObject.Find("Canvas").GetComponent<Transform>().Find("PuddleImage").GetComponent<Image>();
        puddle.color = new Color32(255, 255, 255, 100);
    }

    public void EnableClearing()
    {
        foreach (GameObject tile in grid)
        {
            tile.GetComponent<SpriteRenderer>().enabled = true;
        }

        GameObject button = null;
        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            if (go.name == "CloseButton" + drawingNumber)
                button = go;
        }
        button.SetActive(true);

        Image puddle = GameObject.Find("Canvas").GetComponent<Transform>().Find("PuddleImage").GetComponent<Image>();
        puddle.color = new Color32(255, 255, 255, 225);
    }
}
