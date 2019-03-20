using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Activation : MonoBehaviour {

    MouseClick myScript;
    bool start;
	// Use this for initialization
	void Start () {
        myScript = gameObject.GetComponent<MouseClick>();
        start = true;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void EnableDrawing()
    {
        myScript.enabled = true;
        if(!start)
        {
            myScript.EnableDrawing();
        }
        start = false;
    }

    public void DisableDrawing()
    {
        if(myScript.enabled != false)
        {
            myScript.DisableDrawing();
            myScript.TaskOnClick();
        }
        myScript.enabled = false;      
    }
}
