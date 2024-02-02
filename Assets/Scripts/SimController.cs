using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public class SimController : MonoBehaviour
{
    public Camera gamecam;
    public List<Body> Bodies = new List<Body>{};
    public List<Body> realbodies = new List<Body>{};
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
        Body FakePrimaryBody = Bodies[0];
        float mostmassive = 0;
        foreach(Body i in Bodies)
        {
            if(i.mass>mostmassive)
            {
                FakePrimaryBody = i;
                mostmassive = FakePrimaryBody.mass;
            }
        }
        Bodies.Remove(FakePrimaryBody);
        PrimaryBody = Instantiate(FakePrimaryBody);
        realbodies.Add(PrimaryBody);
        float maxdist = 0;
        foreach(Body i in Bodies)
        {
            Body tmp = Instantiate(i);
            realbodies.Add(tmp);
            maxdist = Mathf.Max(i.r_i,maxdist);
        }
        ScaleFactor = 6/maxdist;
    }

    void adjustcamera()
    {
        Vector2 ScreenSize;
        ScreenSize.x = gamecam.pixelWidth;
        ScreenSize.y = gamecam.pixelHeight;
        float lerpval = 0;
        float aspect = ScreenSize.x/ScreenSize.y;
        float MarginSize = 0.1f;
        float MarginPixel = MarginSize*ScreenSize.y;
        foreach(Body i in realbodies)
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

    void FixedUpdate()
    {
        if(play==true)
        {
            time += Time.deltaTime*TimeScale;
            adjustcamera();
        }
    }
}
