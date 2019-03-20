using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MouseClick : MonoBehaviour {

    public GameObject tile;
    public int frameNum = 0;
    //public bool testSquare = true;
    //public bool testCircle = false;
    public bool showBackground = true;
    
    public int moveX = 0;
    public int moveY = 0;
    public Button resetButton;
    public Text labels;
    public int drawingNumber;
    public static int res = 128;
    public float scale = 1;
    public float rotationStep = 5;

    private static int resX = res;
    private static int resY = res;
    private static int buttonsPos = res / 32 * 100;

    private GameObject[,] grid = new GameObject[resX, resY];
    private int[,] visited = new int[resX, resY];
    private Camera inputCamera;
    private int lineNum;
    private GameObject spriteObject;
    private LineRenderer lineRenderer;
    private Vector3[] linePositions = null;
    private LinkedList<LineRenderer> lines = new LinkedList<LineRenderer>();
    private LinkedList<Vector3[]> originalPositions = new LinkedList<Vector3[]>();
    private Vector3 mousePosition;
    private List<Vector3> nodes = new List<Vector3>();
    private int frameCount = 0;
    private bool mousePressed = false;
    private Color backgroundColor;
    private int xMax = 0;
    private int xMin = resY;
    private int yMax = 0;
    private int yMin = resX;
    private Text tempScale;
    private Text tempSquare;
    private Text tempCircle;
    private Text tempWhatIsIt;
    private Button tempButton;
    private int num;
    private double PIDiv180 = Math.PI / 180;
    private float rotationAngle = 0;
    private int rotationLineNum = 0;

    // Use this for initialization
    void Start () {
        inputCamera = GameObject.Find("Camera").GetComponent<Camera>();

        //spriteObject = GameObject.Find("Color");
        float newScale = 0.4f * scale;
        backgroundColor = tile.GetComponent<SpriteRenderer>().color;
        tile.GetComponent<Transform>().localScale = new Vector3(newScale, newScale, 1);
        float tileRes = tile.GetComponent<SpriteRenderer>().bounds.size.x; 

        for (int x = 0; x < resX; x++)
        {
            for (int y = 0; y < resY; y++)
            {
                //GameObject newTile = new GameObject("Color " + x + ", " + y);
                GameObject newTile = Instantiate(tile, new Vector3(moveX + x  * scale, moveY + y * scale, 1), Quaternion.identity);
                newTile.transform.SetParent(GameObject.Find("DrawingRenderer").GetComponent<Transform>());
                grid[x, y] = newTile;             
                visited[x, y] = 0;
            }
        }

        tempButton = Instantiate(resetButton, new Vector3(0, 0, 1), Quaternion.identity) as Button;
        tempButton.name = "ScaleButton" + drawingNumber;
        tempButton.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>());
        tempButton.transform.localScale = new Vector3((float)0.3, (float)0.3, 0);
        tempButton.transform.localPosition = new Vector3(-140 + (float)5.5 * moveX + scale * buttonsPos, -55 + (float)5.5 * moveY, 0);
        tempButton.onClick.AddListener(Scale);
        tempButton.GetComponentInChildren<Text>().text = "S";

        tempButton = Instantiate(resetButton, new Vector3(0, 0, 1), Quaternion.identity) as Button;
        tempButton.name = "RotationButton" + drawingNumber;
        tempButton.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>());
        tempButton.transform.localScale = new Vector3((float)0.3, (float)0.3, 0);
        tempButton.transform.localPosition = new Vector3(-140 + (float)5.5 * moveX + scale * buttonsPos + 30, -55 + (float)5.5 * moveY, 0);
        tempButton.onClick.AddListener(Rotate);
        tempButton.GetComponentInChildren<Text>().text = "R";

        tempButton = Instantiate(resetButton, new Vector3(0, 0, 1), Quaternion.identity) as Button;
        tempButton.name = "CloseButton" + drawingNumber;
        tempButton.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>());
        tempButton.transform.localScale = new Vector3((float)0.3, (float)0.3, 0);
        tempButton.transform.localPosition = new Vector3(-140 + (float)5.5 * moveX + scale * buttonsPos + 60, -55 + (float)5.5 * moveY, 0);
        tempButton.onClick.AddListener(TaskOnClick);

        tempButton = Instantiate(resetButton, new Vector3(0, 0, 1), Quaternion.identity) as Button;
        tempButton.name = "BackgroundButton" + drawingNumber;
        tempButton.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>());
        tempButton.transform.localScale = new Vector3((float)0.3, (float)0.3, 0);
        tempButton.transform.localPosition = new Vector3(-140 + (float)5.5 * moveX + scale * buttonsPos, -55 + (float)5.5 * moveY - 30, 0);
        tempButton.onClick.AddListener(ToggleVisited);
        tempButton.GetComponentInChildren<Text>().text = "B";

        tempButton = Instantiate(resetButton, new Vector3(0, 0, 1), Quaternion.identity) as Button;
        tempButton.name = "InitCNNButton" + drawingNumber;
        tempButton.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>());
        tempButton.transform.localScale = new Vector3((float)0.3, (float)0.3, 0);
        tempButton.transform.localPosition = new Vector3(-140 + (float)5.5 * moveX + scale * buttonsPos + 30, -55 + (float)5.5 * moveY - 30, 0);
        tempButton.onClick.AddListener(InitializeCNN);
        tempButton.GetComponentInChildren<Text>().text = "I";

        tempButton = Instantiate(resetButton, new Vector3(0, 0, 1), Quaternion.identity) as Button;
        tempButton.name = "TrainCNNButton" + drawingNumber;
        tempButton.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>());
        tempButton.transform.localScale = new Vector3((float)0.3, (float)0.3, 0);
        tempButton.transform.localPosition = new Vector3(-140 + (float)5.5 * moveX + scale * buttonsPos + 60, -55 + (float)5.5 * moveY - 30, 0);
        tempButton.onClick.AddListener(StartTraining);
        tempButton.GetComponentInChildren<Text>().text = "T";

        /*tempScale = Instantiate(labels, new Vector3(0, 0, 1), Quaternion.identity) as Text;
        tempScale.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
        tempScale.transform.localScale = new Vector3((float)0.3, (float)0.3, 0);
        tempScale.transform.localPosition = new Vector3(tempButton.transform.localPosition.x - 60, tempButton.transform.localPosition.y, 0);

        tempSquare = Instantiate(labels, new Vector3(0, 0, 1), Quaternion.identity) as Text;
        tempSquare.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
        tempSquare.transform.localScale = new Vector3((float)0.3, (float)0.3, 0);
        tempSquare.transform.localPosition = new Vector3(tempScale.transform.localPosition.x - 60, tempScale.transform.localPosition.y, 0);

        tempCircle = Instantiate(labels, new Vector3(0, 0, 1), Quaternion.identity) as Text;
        tempCircle.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
        tempCircle.transform.localScale = new Vector3((float)0.3, (float)0.3, 0);
        tempCircle.transform.localPosition = new Vector3(tempScale.transform.localPosition.x -120, tempScale.transform.localPosition.y, 0);*/

        /*tempWhatIsIt = Instantiate(labels, new Vector3(0, 0, 1), Quaternion.identity) as Text;
        tempWhatIsIt.name = "Label" + drawingNumber;
        tempWhatIsIt.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>());
        tempWhatIsIt.text = "What could go here?";
        tempWhatIsIt.transform.localScale = new Vector3((float)0.3, (float)0.3, 0);
        tempWhatIsIt.transform.localPosition = new Vector3(tempButton.transform.localPosition.x - 120, tempButton.transform.localPosition.y - 5, 0);*/

        //scale = GameObject.Find("Canvas").GetComponent<Transform>().Find("Drawing").GetComponent<Transform>().Find("Scale").GetComponent<Text>();
        //avgSquare = GameObject.Find("Canvas").GetComponent<Transform>().Find("Drawing").GetComponent<Transform>().Find("Square").GetComponent<Text>();
        //avgCircle = GameObject.Find("Canvas").GetComponent<Transform>().Find("Drawing").GetComponent<Transform>().Find("Circle").GetComponent<Text>();
        //whatIsIt = GameObject.Find("Canvas").GetComponent<Transform>().Find("Drawing").GetComponent<Transform>().Find("WhatIsIt").GetComponent<Text>();

        //Button btn = resetButton.GetComponent<Button>();
        //btn.onClick.AddListener(TaskOnClick);
        //Debug.Log(grid);
        //sprite = spriteObject.GetComponent<SpriteRenderer>();
    }
	
	// Update is called once per frame
	void Update () {
    
        mousePosition = inputCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        if (mousePosition.x > moveX && mousePosition.x < moveX + resY * scale && mousePosition.y > moveY && mousePosition.y < moveY + resX * scale)
        {         
            if (Input.GetMouseButtonDown(0))
            {
                mousePressed = true;
                lineNum++;
                GameObject lineObject = new GameObject("NewLine" + lineNum);
                lineRenderer = lineObject.AddComponent<LineRenderer>();
                //lineRenderer.startColor = Color.grey;
                //lineRenderer.endColor = Color.blue;
                lineRenderer.startWidth = (float)0.5*scale;
                lineRenderer.endWidth = (float)0.5*scale;
                lineRenderer.numCapVertices = 20;
                lineRenderer.numCornerVertices = 20;
                lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
                lineRenderer.transform.parent = GameObject.Find("DrawingRenderer").GetComponent<Transform>();
                lines.AddLast(lineRenderer);

                nodes.Clear();
                Debug.Log("Mouse pressed");
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (linePositions != null)
                {
                    originalPositions.AddLast(linePositions);
                    linePositions = null;
                }
                mousePressed = false;
                Debug.Log("Mouse released, num lines: " + originalPositions.Count);
            }
        }
        else if (mousePressed)/*if (Input.GetMouseButtonUp(0))*/
        {
            if (linePositions != null)
            {
                originalPositions.AddLast(linePositions);
                linePositions = null;
            }
            mousePressed = false;
            Debug.Log("Mouse released, num lines: " + originalPositions.Count);
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
                    linePositions = nodes.ToArray();
                    lineRenderer.positionCount = linePositions.Length;
                    lineRenderer.SetPositions(linePositions);
                    //Debug.Log(mousePosition);
                }
            }
            frameCount++;       
        }       
    }

    private bool insideSprite(Vector3 position, GameObject sprite)
    {
        if (position.x > sprite.transform.position.x && position.y > sprite.transform.position.y)
            if (position.x < sprite.transform.position.x + scale && position.y < sprite.transform.position.y + scale)
            {
                //Debug.Log(position.x + "  " + position.y);
                //Debug.Log(sprite.transform.position.x + "  " + sprite.transform.position.y);
                return true;
            }
        return false;
    }

    private Vector3[]  scaleImage (Vector3[] nodesArray, int nodesNum)
    {
        List<Vector3> newNodes = new List<Vector3>();
        Vector3[] newArray = new Vector3[nodesNum];
        float xMax = moveX;
        float yMax = moveY;
        float xMin = moveX + resX;
        float yMin = moveY + resY;
        float xNew = 0;
        float yNew = 0;
        float newScale = 1;

        foreach (Vector3 node in nodesArray)    // getting furthest coordinates
        {
            if (node.x > xMax)
                xMax = node.x;
            if (node.x < xMin)
                xMin = node.x;
            if (node.y > yMax)
                yMax = node.y;
            if (node.y < yMin)
                yMin = node.y;     
        }
        xNew = xMax - xMin;
        yNew = yMax - yMin;

        if (xNew > yNew)                        // scaling to a bigger axis
            newScale = resX / xNew;
        else
            newScale = resX / yNew;

        foreach (Vector3 node in nodesArray)
        {
            Vector3 newNode = new Vector3(moveX + (node.x - xMin) * newScale * scale, moveY + (node.y - yMin) * newScale * scale, 1);
            if (node.x < xMin + 0.5)
                newNode = new Vector3(moveX + 0.2f + (node.x - xMin) * newScale * scale, moveY + (node.y - yMin) * newScale * scale, 1);
            else if (node.x > xMax - 0.5)
                newNode = new Vector3(moveX - 0.2f + (node.x - xMin) * newScale * scale, moveY + (node.y - yMin) * newScale * scale, 1);
            if (node.y > yMax - 0.5)
                newNode = new Vector3(moveX + (node.x - xMin) * newScale * scale, moveY - 0.2f + (node.y - yMin) * newScale * scale, 1);
            else if (node.y < yMin + 0.5)
                newNode = new Vector3(moveX + (node.x - xMin) * newScale * scale, moveY + 0.2f + (node.y - yMin) * newScale * scale, 1);
            newNodes.Add(newNode);
        }

        newArray = newNodes.ToArray();
        return newArray;
    }  

    private void checkVisited(Vector3[] nodes, int nodesNum)
    {
        /*foreach (GameObject sprite in grid)
        {
            sprite.GetComponent<SpriteRenderer>().color = backgroundColor;
        }*/
        int lastX = 0;
        int lastY = 0;

        for (int i = 0; i < nodesNum; i++)
        {
            bool found = false;
            for (int x = 0; x < resX; x++)
            {
                for (int y = 0; y < resY; y++)
                {
                    if (insideSprite(nodes[i], grid[x, y]))
                    {
                        if (visited[x, y] == 0)
                        {
                            visited[x, y] = 2;
                            if (showBackground)
                                grid[x, y].GetComponent<SpriteRenderer>().color = Color.red;
                        }
                        if (i == 0)
                        {
                            lastX = x;
                            lastY = y;
                        }
                        if (i > 0 && (lastX - x > 1 || lastY - y > 1 || x - lastX > 1 || y - lastY > 1))
                        {
                            addVisited(lastX, lastY, x, y);
                        }
                        lastX = x;
                        lastY = y;
                        found = true;
                        break;
                    }
                }
                if (found)
                    break;
            }
        }

        for (int x = 0; x < resX; x++)
        {
            for (int y = 0; y < resY; y++)
            {
                if (visited[x, y] == 1 && showBackground)
                {
                    grid[x, y].GetComponent<SpriteRenderer>().color = Color.white;
                }
            }
        }
    }

    private void addVisited (int firstX, int firstY, int secondX, int secondY)
    {
        //Debug.Log(firstX + " " + firstY + ", " + secondX + " " + secondY);
        float difX, difY;
        float step;
        bool xAxis;
        bool right;
        bool up;
        if (firstX >= secondX)
        {
            difX = firstX - secondX;
            right = false;
        }            
        else
        {
            difX = secondX - firstX;
            right = true;
        }           
        if (firstY >= secondY)
        {
            difY = firstY - secondY;
            up = false;
        }
        else
        {
            difY = secondY - firstY;
            up = true;
        }

        if (difX >= difY)
        {
            step = difY / difX;
            xAxis = true;
        }
        else
        {
            step = difX / difY;
            xAxis = false;
        }

        //Debug.Log(difX + "  " + difY + ", " + step + ", " + xAxis);

        float overallStep = 0;
        if (xAxis)
        {
            for (int i = 1; i < difX; i++)
            {
                overallStep += step;
                int round = (int)overallStep;
                if (overallStep - round < 1f / 2)
                {
                    if (right)
                    {
                        if (up)
                            visited[firstX + i, firstY + round] = 1;
                        else
                            visited[firstX + i, firstY - round] = 1;
                    }
                    else
                    {
                        if (up)
                            visited[firstX - i, firstY + round] = 1;
                        else
                            visited[firstX - i, firstY - round] = 1;
                    }
                }
                else
                {
                    if (right)
                    {
                        if (up)
                            visited[firstX + i, firstY + round + 1] = 1;
                        else
                            visited[firstX + i, firstY - round - 1] = 1;
                    }
                    else
                    {
                        if (up)
                            visited[firstX - i, firstY + round + 1] = 1;
                        else
                            visited[firstX - i, firstY - round - 1] = 1;
                    }
                }
                    
            }
        }
        else
        {
            for (int i = 1; i < difY; i++)
            {
                overallStep += step;
                int round = (int)overallStep;
                if (overallStep - round < 1f / 2)
                {
                    if (right)
                    {
                        if (up)
                            visited[firstX + round, firstY + i] = 1;
                        else
                            visited[firstX + round, firstY - i] = 1;
                    }
                    else
                    {
                        if (up)
                            visited[firstX - round, firstY + i] = 1;
                        else
                            visited[firstX - round, firstY - i] = 1;
                    }
                }
                else
                {
                    if (right)
                    {
                        if (up)
                            visited[firstX + round + 1, firstY + i] = 1;
                        else
                            visited[firstX + round + 1, firstY - i] = 1;
                    }
                    else
                    {
                        if (up)
                            visited[firstX - round - 1, firstY + i] = 1;
                        else
                            visited[firstX - round - 1, firstY - i] = 1;
                    }
                }
            }
        }
    }

    private Vector3 rotateNode(Vector3 pivotPoint, Vector3 node, float angle)
    {
        Vector3 result;

        Vector3 dir = node - pivotPoint;                    // get point direction relative to pivot
        dir = Quaternion.Euler(0f, 0f, angle) * dir;        // rotate it
        result = dir + pivotPoint;                          // calculate rotated point

        return result;
    }

    private void clearVisited()
    {
        for (int x = 0; x < resX; x++)
        {
            for (int y = 0; y < resY; y++)
            {
                visited[x, y] = 0;
            }
        }
    }

    public void Scale()
    {
        float xMax = moveX;
        float yMax = moveY;
        float xMin = moveX + resX;
        float yMin = moveY + resY;
        float xNew = 0;
        float yNew = 0;
        float newScale = 1;

        foreach (LineRenderer line in lines)
        {
            if (line != null)
            {
                Vector3[] oldPositions = new Vector3[line.positionCount];
                line.GetPositions(oldPositions);            
                foreach (Vector3 node in oldPositions)    // getting furthest coordinates
                {
                    if (node.x > xMax)
                        xMax = node.x;
                    if (node.x < xMin)
                        xMin = node.x;
                    if (node.y > yMax)
                        yMax = node.y;
                    if (node.y < yMin)
                        yMin = node.y;
                }                             
            }
        }

        xNew = xMax - xMin;
        yNew = yMax - yMin;

        if (xNew > yNew)                        // scaling to a bigger axis
            newScale = resX / xNew;
        else
            newScale = resX / yNew;

        foreach (LineRenderer line in lines)
        {
            if (line != null)
            {
                Vector3[] positions = new Vector3[line.positionCount];
                int positionsNum = line.GetPositions(positions);

                List<Vector3> newNodes = new List<Vector3>();
                Vector3[] newArray = new Vector3[positionsNum];
                foreach (Vector3 node in positions)
                {
                    Vector3 newNode = new Vector3(moveX + (node.x - xMin) * newScale * scale, moveY + (node.y - yMin) * newScale * scale, 1);
                    if (node.x < xMin + 0.2)
                        newNode = new Vector3(moveX + 0.1f + (node.x - xMin) * newScale * scale, moveY + (node.y - yMin) * newScale * scale, 1);
                    else if (node.x > xMax - 0.2)
                        newNode = new Vector3(moveX - 0.1f + (node.x - xMin) * newScale * scale, moveY + (node.y - yMin) * newScale * scale, 1);
                    if (node.y > yMax - 0.2)
                        newNode = new Vector3(moveX + (node.x - xMin) * newScale * scale, moveY - 0.1f + (node.y - yMin) * newScale * scale, 1);
                    else if (node.y < yMin + 0.2)
                        newNode = new Vector3(moveX + (node.x - xMin) * newScale * scale, moveY + 0.1f + (node.y - yMin) * newScale * scale, 1);
                    newNodes.Add(newNode);
                }
                newArray = newNodes.ToArray();
                line.SetPositions(newArray);
                checkVisited(newArray, positionsNum);
            }
        }
    }

    public void Rotate()
    {
        clearVisited();
        Vector3 pivot = new Vector3(25, 25, 0);
        rotationAngle += rotationStep;
        rotationLineNum = 0;
        foreach (LineRenderer line in lines)
        {
            if (line != null)
            {
                Vector3 temp;
                //Debug.Log("Rotating line: " + rotationLineNum);     
                Vector3[] positions = originalPositions.ElementAt(rotationLineNum);
                List<Vector3> tempList = new List<Vector3>();
                foreach (Vector3 node in positions)
                {
                    temp = rotateNode(pivot, node, rotationAngle);
                    tempList.Add(temp);
                }
                line.SetPositions(tempList.ToArray());
                rotationLineNum++;
            }         
        }
        //Debug.Log(rotationAngle);
        Scale(); 
    }

    public void TaskOnClick()
    {
        Debug.Log("CLicked!");
        foreach(LineRenderer line in lines)
        {
            Destroy(line);
        }
        originalPositions = new LinkedList<Vector3[]>();
        rotationLineNum = 0;
        rotationAngle = 0;

        foreach (GameObject sprite in grid)
        {
            sprite.GetComponent<SpriteRenderer>().color = backgroundColor;
        }
        clearVisited();
        lineNum = 0;
        num = 0;
        //sumCircle = 0;
        //sumSquare = 0;
        frameCount = 0;
        mousePressed = false;
        xMax = 0;
        xMin = resY;
        yMax = 0;
        yMin = resX;
        //tempScale.text = "x, y";
        //tempCircle.text = "circle avg";
        //tempSquare.text = "square avg";
        //tempWhatIsIt.text = "What could go here?";
    }

    public void ToggleVisited()
    {
        showBackground = !showBackground;       
        for (int x = 0; x < resX; x++)
        {
            for (int y = 0; y < resY; y++)
            {
                if (showBackground)
                {
                    if (visited[x, y] == 1)
                        grid[x, y].GetComponent<SpriteRenderer>().color = Color.white;
                    else if (visited[x, y] == 2)
                        grid[x, y].GetComponent<SpriteRenderer>().color = Color.red;
                }
                else
                    grid[x, y].GetComponent<SpriteRenderer>().color = backgroundColor;
            }
        }      
    }

    IEnumerator TrainCNN()
    {
        for (int i = 1; i < 71; i++)
        {
            gameObject.GetComponent<CNN>().test(visited);
            Rotate();
            yield return new WaitForSeconds(1);
            for (int x = 0; x < resX; x++)
            {
                for (int y = 0; y < resY; y++)
                {
                    if (showBackground)
                    {
                        if (visited[x, y] == 1)
                            grid[x, y].GetComponent<SpriteRenderer>().color = Color.white;
                        else if (visited[x, y] == 2)
                            grid[x, y].GetComponent<SpriteRenderer>().color = Color.red;
                        else
                            grid[x, y].GetComponent<SpriteRenderer>().color = backgroundColor;
                    }
                    else
                        grid[x, y].GetComponent<SpriteRenderer>().color = backgroundColor;
                }
            }
        }
    }

    public void StartTraining()
    {
        clearVisited();
        StartCoroutine(TrainCNN());
    }

    public void InitializeCNN()
    {
        gameObject.GetComponent<CNN>().test(visited);
        Rotate();
        //double[] results = gameObject.GetComponent<CNN>().test(visited);
        //string temp = "";
        //Debug.Log("Dalekozor: " + results[0]);
        //Debug.Log("Ljestve: " + results[1]);
        //Debug.Log("Laso: " + results[2]);


        /*for (int d = 0; d < 6; d++)
        {
            for (int i = 0; i < 28; i++)
            {
                temp = "";
                for (int j = 0; j < 28; j++)
                {
                    temp += results[d, i, j] + "  ";
                }
                Debug.Log(temp);
            }
            Debug.Log("------------------------------------------------------");
        }*/
    }

    public void DisableDrawing()
    {
        foreach (GameObject tile in grid)
        {
            tile.GetComponent<SpriteRenderer>().enabled = false;
        }
        //tempButton.enabled = false;
        //tempButton.GetComponent<Image>().enabled = false;
        GameObject button = GameObject.Find("CloseButton" + drawingNumber);
        button.SetActive(false);
        tempWhatIsIt.enabled = false;
    }

    public void EnableDrawing()
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

        tempWhatIsIt.enabled = true;
    }

    /*private int[,] square = new int[,] {{ 5, 5, 5, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 5, 5, 5, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 5, 5, 5, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 5, 5, 5, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 5, 5, 3, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 5, 5, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 3, 3, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 3, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 3, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 3, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 3, 3, 2, 1, 0, 0,-1,-1,-1,-1, 0, 0, 1, 2, 3, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 3, 3, 2, 1, 0, 0,-1,-1,-1,-1, 0, 0, 1, 2, 3, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 3, 3, 2, 1, 0, 0,-1,-1,-1,-1, 0, 0, 1, 2, 3, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 3, 3, 2, 1, 0, 0,-1,-1,-1,-1, 0, 0, 1, 2, 3, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 3, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 3, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 3, 3, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 3, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 5, 5, 3, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 5, 5, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 5, 5, 5, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 5, 5, 5, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 5, 5, 5, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 5, 5, 5, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },};
    private int sumSquare = 0;

    private int[,] circle = new int[,] {{ 0, 1, 1, 2, 3, 3, 3, 5, 5, 3, 3, 3, 2, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 1, 2, 3, 3, 3, 3, 3, 5, 5, 3, 3, 3, 3, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 1, 3, 3, 3, 3, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 2, 3, 3, 3, 2, 1, 1, 1, 1, 1, 1, 2, 3, 3, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 3, 3, 3, 2, 1, 1, 0, 0, 0, 0, 1, 1, 2, 3, 3, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 3, 3, 2, 1, 1, 0, 0,-1,-1, 0, 0, 1, 1, 2, 3, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 3, 3, 2, 1, 0, 0,-1,-1,-1,-1, 0, 0, 1, 2, 3, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 5, 5, 2, 1, 0,-1,-1,-1,-1,-1,-1, 0, 1, 2, 5, 5, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 5, 5, 2, 1, 0,-1,-1,-1,-1,-1,-1, 0, 1, 2, 5, 5, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 3, 3, 2, 1, 0, 0,-1,-1,-1,-1, 0, 0, 1, 2, 3, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 3, 3, 2, 1, 1, 0, 0,-1,-1, 0, 0, 1, 1, 2, 3, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 3, 3, 3, 2, 1, 1, 0, 0, 0, 0, 1, 1, 2, 3, 3, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 2, 3, 3, 3, 2, 1, 1, 1, 1, 1, 1, 2, 3, 3, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 1, 3, 3, 3, 3, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 1, 2, 3, 3, 3, 3, 3, 5, 5, 3, 3, 3, 3, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                        { 0, 1, 1, 2, 3, 3, 3, 5, 5, 3, 3, 3, 2, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },};
    private int sumCircle = 0;*/

    /*private void old()
    {
        for (int x = 0; x < resX; x++)
            {
                for (int y = 0; y < resY; y++)
                {
                    if (insideSprite(mousePosition, grid[x, y]))
                    {
                        //Debug.Log(x + " " + y);
                        if (visited[x, y] == 0)
                        {
                            visited[x, y] = 1;
                            //num++;
                            //sumCircle += circle[x, y];
                            //sumSquare += square[x, y];
                            //double avgC = Math.Round(1.0 * sumCircle / num, 2);
                            //double avgS = Math.Round(1.0 * sumSquare / num, 2);
                            //tempCircle.text = "Circle: " + avgC;
                            //tempSquare.text = "Square: " + avgS;
                            if(num > 30)
                            {
                                if (avgC > avgS)
                                {
                                    if (avgC >= 2.8)
                                        tempWhatIsIt.text = "It's a CIRCLE!";
                                    else
                                        tempWhatIsIt.text = "It's nothing...";
                                }
                                else if (avgS >= 2.8)
                                    tempWhatIsIt.text = "It's a SQUARE!";
                                else
                                    tempWhatIsIt.text = "It's nothing...";
                            }

                            if (showBackground)
                            {
                                grid[x, y].GetComponent<SpriteRenderer>().color = Color.white;
                            }
                       
                            if (testSquare && showBackground)
                            {
                                if (square[x, y] == 5)
                                    grid[x, y].GetComponent<SpriteRenderer>().color = Color.red;
                                else if (square[x, y] == 3)
                                    grid[x, y].GetComponent<SpriteRenderer>().color = Color.blue;
                                else if (square[x, y] == 2)
                                    grid[x, y].GetComponent<SpriteRenderer>().color = Color.yellow;
                                else if (square[x, y] == 1)
                                    grid[x, y].GetComponent<SpriteRenderer>().color = Color.green;
                                else
                                    grid[x, y].GetComponent<SpriteRenderer>().color = Color.white;
                            }
                            else if (testCircle && showBackground)
                            {
                                if (circle[x, y] == 5)
                                    grid[x, y].GetComponent<SpriteRenderer>().color = Color.red;
                                else if (circle[x, y] == 3)
                                    grid[x, y].GetComponent<SpriteRenderer>().color = Color.blue;
                                else if (circle[x, y] == 2)
                                    grid[x, y].GetComponent<SpriteRenderer>().color = Color.yellow;
                                else if (circle[x, y] == 1)
                                    grid[x, y].GetComponent<SpriteRenderer>().color = Color.green;
                                else
                                    grid[x, y].GetComponent<SpriteRenderer>().color = Color.white;
                            }
                        }


                    }
                }
        }
    }*/
}
