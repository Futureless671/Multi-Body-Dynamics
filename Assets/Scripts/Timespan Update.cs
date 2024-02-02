using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimespanUpdate : MonoBehaviour
{
    private Text txt;
    private SimController controller;
    private TimeSpan time;
    private float years;
    private float Days;
    private float Hours;
    // Start is called before the first frame update
    void Start()
    {
        txt = GetComponent<Text>();
        controller = FindObjectOfType<SimController>();
    }

    // Update is called once per frame
    void Update()
    {
        time = TimeSpan.FromSeconds(controller.time);
        years = Mathf.Floor(time.Days/365);
        Days = time.Days - years*365;
        Hours = time.Hours;
        txt.text = "Y: " + years + "  D: " + Days + "  H: " + Hours;
    }
}
