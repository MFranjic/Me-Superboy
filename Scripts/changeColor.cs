using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class changeColor : MonoBehaviour {

    Color[] colorlist = { Color.yellow, Color.green, Color.blue, Color.red };
    private GameObject colorObject;
    private SpriteRenderer spirit;



    void Start()
    {
        colorObject = GameObject.Find("Color");
        spirit = colorObject.GetComponent<SpriteRenderer>();   
        //colorChange();
    }

    // Update is called once per frame
    void Update()
    {
        //colorChange();
    }

    void OnMouseDown()
    {
        colorChange();
    }

    void colorChange()
    {
        spirit.color = colorlist[Random.Range(0, colorlist.Length)];

    }
}
