using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TimespanUpdate : MonoBehaviour
{
    private Label yrslabel;
    private Label dayslabel;
    private Label hrslabel;
    private SimController controller;
    private TimeSpan time;
    private float years;
    private float Days;
    private float Hours;
    private Button playbutton;
    private Button pausebutton;
    private Button stopbutton;
    // Start is called before the first frame update
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        yrslabel = root.Q<Label>("Years");
        dayslabel = root.Q<Label>("Days");
        hrslabel = root.Q<Label>("Hours");
        playbutton = root.Q<Button>("Play");
        pausebutton = root.Q<Button>("Pause");
        stopbutton = root.Q<Button>("Stop");
        controller = FindObjectOfType<SimController>();

        playbutton.RegisterCallback<ClickEvent>(playsim);
        pausebutton.RegisterCallback<ClickEvent>(pausesim);
        stopbutton.RegisterCallback<ClickEvent>(stopsim);
    }

    void playsim(ClickEvent evt)
    {
        controller.play = true;
    }

    void pausesim(ClickEvent evt)
    {
        controller.play = false;
    }

    void stopsim(ClickEvent evt)
    {
        controller.resetsim();
    }
    
    void Update()
    {
        time = TimeSpan.FromSeconds(controller.time);
        years = Mathf.Floor(time.Days/365);
        Days = time.Days - years*365;
        Hours = time.Hours;
        yrslabel.text = "Y: " + years;
        dayslabel.text = "D: " + Days.ToString("000");
        hrslabel.text = " H: " + Hours.ToString("00");
        //txt.text = "Y: " + years + " D: " + Days.ToString("000") + " H: " + Hours.ToString("00");
    }
    
}
