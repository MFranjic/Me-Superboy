using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearActivation : MonoBehaviour {

    ClearScript myScript;
    bool start;
    // Use this for initialization
    void Start()
    {
        myScript = gameObject.GetComponent<ClearScript>();
        start = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void EnableClearing()
    {
        myScript.enabled = true;
        if(!start)
        {
            myScript.EnableClearing();
        }          
        start = false;
    }

    public void DisableClearing()
    {
        if(myScript.enabled != false)
        {
            myScript.DisableClearing();
            myScript.TaskOnClick();
        }
        myScript.enabled = false;      
    }
}
