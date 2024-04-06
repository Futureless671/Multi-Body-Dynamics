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
    public SimulationUI simui;
    public List<Body> Bodies = new List<Body>{};
    public float ScaleFactor;
    public float BodyScale = 1;
    public float TimeScale;
    public Body PrimaryBody;
    public float UGC = 6.6743f*Mathf.Pow(10,-22);
    public float time;
    public bool play;
    public Body InertialFoR;
    public float TrailRatio;
    [HideInInspector] public string ScalingType = "Linear";
    [HideInInspector] public float LinScaleFactor;
    [HideInInspector] public float LinOffset;
    [HideInInspector] public float LogScaleFactor;
    private float logbase;
    [HideInInspector] public float LogOffset;
    [HideInInspector] public float LogAbsOffset;
    public Body furthest;
    public Vector3 polartocartesian(float r, float f)
    {
        return new Vector3(r*Mathf.Cos(Mathf.Deg2Rad*f),r*Mathf.Sin(Mathf.Deg2Rad*f),0);
    }
    void Start()
    {
        simui = FindObjectOfType<SimulationUI>();
        gamecam = FindObjectOfType<Camera>();
    }

    void InitializeSim()
    {
        furthest = Bodies[0];
        float mostmassive = -1;
        foreach(Body i in Bodies)
        {
            if(i.mass>=mostmassive)
            {
                mostmassive = i.mass;
                PrimaryBody = i;
            }
            if(furthest!=null)
            {
                if(i.r_i>=furthest.r_i)
                {
                    furthest = i;
                }
            }
            i.primbodycheck = false;
        }
        autoscale();
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
        InitializeSim();
        time = 0;
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

    public Vector3 scaleposition(Vector3 position)
    {
        float dist = position.magnitude;
        Vector3 scaledpos = new Vector3(0,0,0);
        if(ScalingType=="Linear")
        {
            scaledpos = Vector3.Normalize(position)*Mathf.Max(0,dist*LinScaleFactor+LinOffset);
        }
        else if(ScalingType=="Logarithmic")
        {
            logbase = Mathf.Pow(furthest.r_i+Mathf.Max(LogOffset,-furthest.r_i+1),1/LogScaleFactor);
            scaledpos = Vector3.Normalize(position)*Mathf.Max(0,Mathf.Log(dist+Mathf.Max(LogOffset,-furthest.r_i+1),logbase)+LogAbsOffset);
        }
        return scaledpos;
    }

    public void autoscale()
    {
        if(ScalingType=="Linear")
        {
            LinScaleFactor = 6/furthest.r_i;
            LinOffset = 0;
        }
        else if(ScalingType=="Logarithmic")
        {
            LogScaleFactor = 6;
            logbase = Mathf.Pow(furthest.r_i+Mathf.Max(LogOffset,-furthest.r_i+1),1/LogScaleFactor);
            LogOffset = 0;
        }
        simui.SyncScales();
    }
}
