using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public class SimController : MonoBehaviour
{
    public Camera gamecam;
    public List<Body> Bodies = new List<Body>{};
    public float ScaleFactor;
    public float BodyScale = 1;
    public float TimeScale;
    public Body PrimaryBody;
    public float UGC = (float)6.674*Mathf.Pow(10,-11);
    public float time;
    public bool play;
    public Body InertialFoR;
    public float TrailRatio;
    public Vector3 polartocartesian(float r, float f)
    {
        return new Vector3(r*Mathf.Cos(Mathf.Deg2Rad*f),r*Mathf.Sin(Mathf.Deg2Rad*f),0);
    }
    void Start()
    {
        gamecam = FindObjectOfType<Camera>();
    }

    void InitializeSim()
    {
        float mostmassive = -1;
        foreach(Body i in Bodies)
        {
            if(i.mass>=mostmassive)
            {
                mostmassive = i.mass;
                PrimaryBody = i;
                print("Body Set");
            }
            i.primbodycheck = false;
        }
        foreach(Body i in Bodies)
        {
            i.InitBody();
        }
    }

    void adjustcamera()
    {
        Vector2 ScreenSize;
        ScreenSize.x = gamecam.pixelWidth;
        ScreenSize.y = gamecam.pixelHeight;
        float lerpval = 0;
        float aspect = ScreenSize.x/ScreenSize.y;
        float MarginSize = 0.05f;
        float MarginPixel = MarginSize*ScreenSize.y;
        foreach(Body i in Bodies)
        {
            Vector2 Position = gamecam.WorldToScreenPoint(i.transform.position);
            if(Position.x<MarginPixel)
            {
                lerpval = Mathf.Max((MarginPixel - Position.x)/MarginPixel,lerpval);
            }
            else if(Position.x>ScreenSize.x-MarginPixel)
            {
                lerpval = Mathf.Max((Position.x - (ScreenSize.x - MarginPixel))/MarginPixel,lerpval);
            }

            if(Position.y<MarginPixel)
            {
                lerpval = Mathf.Max((MarginPixel - Position.y)/MarginPixel,lerpval);
            }
            else if(Position.y>ScreenSize.y-MarginPixel)
            {
                lerpval = Mathf.Max((Position.y - (ScreenSize.y - MarginPixel))/MarginPixel,lerpval);
            }
        }
        gamecam.orthographicSize *= Mathf.SmoothStep(1,1.01f,lerpval);
    }

    public void resetsim()
    {
        play=false;
        time = 0;
        InitializeSim();
        gamecam.orthographicSize = 5;
    }

    void FixedUpdate()
    {
        if(play==true)
        {
            time += Time.deltaTime*TimeScale;
            adjustcamera();
        }
    }
}
