using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Rendering;

public class Body : MonoBehaviour
{
    //=================================================
    //=============== Basic Parameters ================
    //=================================================
    [SerializeField]
    public string Name;
    [SerializeField]
    public Color color;
    private TrailRenderer trail;
    private SpriteRenderer sprite;
    private SimController controller;
    public Body PrimaryBody;
    public Body ParentBodyOverride;
    private bool primbodycheck;
    public float radialscale = 1;
    private GameObject SOI;
    public float SOI_radius;
    public float BodyScale;
    public RenderTexture rt_base;
    public RenderTexture rt;
    public Camera rtcam;
    private float accumtime = 0;
    public BodyEntry bodyentry;
    //=================================================
    //=========== Physical Input Parameters ===========
    //=================================================
    public float mass;
    private float mass_mem;
    [SerializeField]
    public float radius;
    [SerializeField]
    public float r_i;
    private float r_i_mem;
    [SerializeField]
    private float f_i;
    [SerializeField]
    public float v_i;
    private float v_i_mem;
    //=================================================
    //============= Calculated Parameters =============
    //=================================================
    private float r;
    //[HideInInspector]
    public float f;
    [HideInInspector]
    public Vector3 v;

    struct OrbitalConstants
    {
        public float a;
        public float alpha;
        public float mu;
        public float h;
        public float p;
        public float e;
        public float T;
        public float rp;
        public float ra;
    }

    private OrbitalConstants orbit;

    OrbitalConstants CalcParams(float r, float v)
    {
        OrbitalConstants orbit = new OrbitalConstants();
        orbit.mu = controller.UGC*(PrimaryBody.mass+mass);
        orbit.alpha = 2/r - v*v/orbit.mu;
        orbit.a = 1/orbit.alpha;
        orbit.h = r*v;
        orbit.p = orbit.h*orbit.h/orbit.mu;
        orbit.e = Mathf.Sqrt(1-orbit.p/orbit.a);
        orbit.T = 2*3.14159265f*Mathf.Sqrt(orbit.a*orbit.a*orbit.a/orbit.mu);
        orbit.rp = orbit.a*(1 - orbit.e);
        orbit.ra = orbit.a*(1 + orbit.e);
        return orbit;
    }

    float Cfunction(float y)
    {
        float C;
        if(y>0)
        {
            C = (1 - Mathf.Cos(Mathf.Sqrt(y)))/y;
        }
        else if(y<0)
        {
            C = (float)(Math.Cosh(-y) - 1)/(-y);
        }
        else
        {
            C = 1/2;
        }
        return C;
    }

    float Sfunction(float y)
    {
        float S;
        if(y>0)
        {
            S = (Mathf.Sqrt(y) - Mathf.Sin(Mathf.Sqrt(y)))/Mathf.Sqrt(y*y*y);
        }
        else if(y<0)
        {
            S = ((float)Math.Sinh(-y) - Mathf.Sqrt(-y))/Mathf.Sqrt(-y*y*y);
        }
        else
        {
            S = 1/6;
        }
        return S;
    }

    float dSdy(float y)
    {
        float dSdy;
        dSdy = (Cfunction(y) - 3*Sfunction(y))/(2*y);
        return dSdy;
    }

    float dCdy(float y)
    {
        float dCdy;
        dCdy = (1 - y*Sfunction(y) - 2*Cfunction(y))/(2*y);
        return dCdy;
    }

    float F(float x, float r, float dt)
    {
        float F;
        F = (1 - r*orbit.alpha)*x*x*x*Sfunction(orbit.alpha*x*x) + r*x - Mathf.Sqrt(orbit.mu) * dt;
        return F;
    }

    float Fprime(float x, float r)
    {
        float Fprime;
        Fprime = (1 - r*orbit.alpha)*x*x*Cfunction(orbit.alpha*x*x) + r;
        return Fprime;
    }

    float Newton(float dt)
    {
        float xplus;
        float xminus;
        float x_o;
        float x;
        xplus = Mathf.Sqrt(orbit.mu)*dt/orbit.rp;
        xminus = Mathf.Sqrt(orbit.mu)*dt/orbit.ra;
        x_o = (xplus + xminus)/2;
        x = x_o;
        for(int i = 0; i<5; i++)
        {
            x = x_o - F(x_o,r_i,dt)/Fprime(x_o,r_i);
            x_o = x;
        }
        return x;
    }

    Vector3 CalcPosition(float x, float r, float v, float dt)
    {
        float i;
        float j;
        i = r - x*x*Cfunction(orbit.alpha*x*x);
        j = dt*v - x*x*x*Sfunction(orbit.alpha*x*x)*v/Mathf.Sqrt(orbit.mu);
        return new Vector3(i,j,0);
    }

    void Awake()
    {
        //=================================================
        //============= Object Initialization =============
        //=================================================
        // This section initializes the necessary parameters for this object to function properly and retrieves necessary references to other objects
        r_i_mem = r_i;
        v_i_mem = v_i;
        SOI = transform.GetChild(0).gameObject; // Fetch Sphere of Influence Object
        primbodycheck = false; // initialize primary body bool as false (this is a basic check within the script to determine whether the current object is the system's primary body)
        controller = FindObjectOfType<SimController>(); // Get reference to sim controller
        PrimaryBody = controller.PrimaryBody; // Fetch primary body from sim controller
        // The following is a check to see if the sim controller returns a null value for the Primary body. This will only be the case if this object is the first object instantiated by the sim controller.
        if(PrimaryBody==null)
        {
            PrimaryBody = GetComponent<Body>(); // Set current object as primary object for future use
            primbodycheck = true; // Set primary body bool to true
        }
        sprite = GetComponent<SpriteRenderer>(); // Fetch Reference to Sprite Renderer
        trail = GetComponent<TrailRenderer>(); // Fetch reference to Trail Renderer
        trail.enabled = false; // Disable trail
        color = new Color(color.r,color.g,color.b,1.0f); // Set color variable to color set in Body parameters
        sprite.color = color; // Set sprite color
        Gradient gradient = new Gradient(); // Initialize new gradient object
        gradient.SetKeys(new GradientColorKey[] {new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f)}, new GradientAlphaKey[] {new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f)}); // Set trail color gradient
        trail.colorGradient = gradient; // assign trail color gradient
        rtcam = transform.GetChild(1).GetComponent<Camera>();
        rt_base = new RenderTexture(256,256,8);
        rt = Instantiate(rt_base);
        rtcam.targetTexture = rt;
        rt.Create();

        //=================================================
        //============== Initial Calculations =============
        //=================================================
        BodyScale = Mathf.Sqrt(radius/PrimaryBody.radius); // Calculate relative body scale
        if(primbodycheck==false) // Limit calculations to objects that are not the primary object
        {
            orbit = CalcParams(r_i,v_i); // Calculate orbit parameters
        }
        trail.enabled = true;
        trail.startWidth = 0.5f;
        trail.endWidth = 0.01f;
        trail.widthMultiplier = BodyScale;
        trail.time = orbit.T*0.75f/controller.TimeScale;
        transform.position = new Vector3(0,0,0);
    }

    void Update()
    {
        accumtime += Time.fixedDeltaTime;
        int desiredfps = 10;
        if(1/accumtime<=desiredfps)
        {
            rtcam.Render();
            accumtime = 0;
        }
        if(r_i != r_i_mem || v_i != v_i_mem || mass != mass_mem)
        {
            if(primbodycheck==false) // Limit calculations to objects that are not the primary object
            {
                orbit = CalcParams(r_i,v_i); // Calculate orbit parameters
            }
        }
        sprite.color = color; // Set sprite color
        float x;
        if(primbodycheck==false)
        {
            transform.localScale = new Vector3(1,1,1)*BodyScale;
            x = Newton(controller.time);
            transform.position = CalcPosition(x, r_i, v_i, controller.time)*controller.ScaleFactor;
        }
        else
        {
            //transform.position = new Vector3(0,0,0);
            transform.localScale = new Vector3(1,1,1);
        }
    }
}