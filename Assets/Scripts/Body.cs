using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class Body : MonoBehaviour
{
    //=================================================
    //=============== Basic Parameters ================
    //=================================================
    [SerializeField]
    public string Name;
    [SerializeField]
    private Color color;
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
    //=================================================
    //=========== Physical Input Parameters ===========
    //=================================================
    public float mass;
    [SerializeField]
    private float radius;
    [SerializeField]
    public float r_i;
    [SerializeField]
    private float f_i;
    [SerializeField]
    private Vector3 v_i;
    //=================================================
    //============= Calculated Parameters =============
    //=================================================
    private float r;
    //[HideInInspector]
    public float f;
    [HideInInspector]
    public Vector3 v;
    private float a;
    private Vector3 h;
    private float p;
    private float mu;
    private float epsilon;
    private float e;
    private float orbitdir;
    private float T;

    void Awake()
    {
        //=================================================
        //============= Object Initialization =============
        //=================================================
        // This section initializes the necessary parameters for this object to function properly and retrieves necessary references to other objects
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


        //=================================================
        //============== Initial Calculations =============
        //=================================================
        BodyScale = Mathf.Sqrt(PrimaryBody.radius*radius)/PrimaryBody.radius; // Calculate relative body scale
        if(primbodycheck==false) // Limit calculations to objects that are not the primary object
        {
            calcparameters(r_i,f_i,v_i); // Calculate orbit parameters
        }
        trail.enabled = true;
        trail.startWidth = 0.5f;
        trail.endWidth = 0.01f;
        trail.widthMultiplier = BodyScale;
    }

    //=================================================
    //========== Orbit Parameter Calculations =========
    //=================================================
    void calcparameters(float r, float f, Vector3 v)
    {
        h = Vector3.Cross(controller.polartocartesian(r, f),v); // Calculate specific angular momentum (this is constant for all points of an orbit)
        orbitdir = Mathf.Sign(h.z); // Set Orbit Direction (this only works because the simulation is two-dimensional)
        mu = controller.UGC*(mass+PrimaryBody.mass); // Calculate gravitational constant (this is constant for all points of an orbit)
        p = h.magnitude*h.magnitude/mu; // calculate orbital parameter (position of the semi-latus rectum) (this is constant for all points of an orbit)
        epsilon = 0.5f*v.magnitude*v.magnitude - mu/r; // Calculate the orbit's specific energy (this is constant for all points of an orbit)
        a = -mu/(2f*epsilon); // calculate orbit's semi-major axis (this applies only to elliptical and hyperbolic orbits) (this is constant for all points of an orbit)
        e = Mathf.Sqrt(1-(p/a)); // calculate orbit eccentricity (this applies only to elliptical and hyperbolic orbits) (this is constant for all points of an orbit)
        SOI_radius = a*Mathf.Pow(mass/PrimaryBody.mass,2/5)*radius/PrimaryBody.radius; // calculate the radius of the orbit's sphere of influence (this applies only to elliptical orbits)
        T = 2*3.14159265f*Mathf.Sqrt((a*a*a)/mu);
    }

    //=================================================
    //========== Orbit Position Calculations ==========
    //=================================================
    void calcposition()
    {
        //=================================================
        //=============== Elliptical Orbits ===============
        //=================================================
        float M = Mathf.Sqrt(mu/(a*a*a))*controller.time; // Calculate Mean Anomoly for current simulation time
        float Ei; // Initialize Ei
        float E = M; // Set Eccentric Anomoly equal to mean anomoly as initial "guess"
        for(int i = 0; i<4; i++) //iterate 4 times per update to solve for actual Eccentric Anomoly
        {
            Ei = E; // Set Ei to E from previous iteration (for first iteration this sets Ei = M)
            float numerator = Ei - e*Mathf.Sin(Ei) - M; // calculate Newton's Method Numerator
            float denomninator = 1 - e*Mathf.Cos(Ei); // calculate denominator of NEwton;s Method
            E = Ei - numerator/denomninator; // Calculate E value based on above
        }
        // Four lines below calculate for the True Anomoly, f
        float num = 1+e;
        float den = 1-e;
        float term = Mathf.Sqrt(num/den)*Mathf.Tan(E/2);
        f = orbitdir*Mathf.Rad2Deg*2*Mathf.Atan(term);
        //print(Name + f); //debug line
        r = Mathf.Log(a*(1-e*Mathf.Cos(E)),5)-14.5f; // calculate radial position based on true anomoly
    }

    void Update()
    {
        transform.localScale = new Vector3(1,1,1)*BodyScale*controller.BodyScale; // Set relative scale based on largest object in system
        SOI.transform.localScale = new Vector3(1,1,1)*SOI_radius*controller.ScaleFactor; // Set relative scale of Sphere of Influence (for display)
        calcposition();
        if(primbodycheck) // Set Primary Body to 0,0,0 so it doesn't move
        {
            transform.position = new Vector3(0.0f,0.0f,0.0f);
        }
        else
        {
            transform.position = controller.polartocartesian(r, f);//*controller.ScaleFactor; 
        }
        trail.time = T*controller.TrailRatio/controller.TimeScale;
    }
}
