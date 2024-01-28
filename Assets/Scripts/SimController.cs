using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class SimController : MonoBehaviour
{
    public List<Body> Bodies = new List<Body>{};
    public List<Body> realbodies = new List<Body>{};
    public float ScaleFactor;
    public float TimeScale;
    public Body PrimaryBody;
    public float UGC = (float)6.674*Mathf.Pow(10,-11);
    public float time;
    public bool play;
    public TimeSpan test;
    public Vector3 polartocartesian(float r, float f)
    {
        return new Vector3(r*Mathf.Cos(Mathf.Deg2Rad*f),r*Mathf.Sin(Mathf.Deg2Rad*f),0);
    }
    void Start()
    {
        Body FakePrimaryBody = Bodies[0];
        float mostmassive = 0;
        play = false;
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
        foreach(Body i in Bodies)
        {
            Body tmp = Instantiate(i);
            realbodies.Add(tmp);
        }
    }

    void FixedUpdate()
    {
        if(play==true)
        {
            time += Time.deltaTime*TimeScale;
        }
    }
}
