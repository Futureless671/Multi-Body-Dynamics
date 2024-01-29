using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

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
    private float r_i;
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

    void Awake()
    {
        SOI = transform.GetChild(0).gameObject;
        primbodycheck = false;
        controller = FindObjectOfType<SimController>();
        PrimaryBody = controller.PrimaryBody;
        if(PrimaryBody==null)
        {
            PrimaryBody = GetComponent<Body>();
            primbodycheck = true;
            print(Name + " primbodycheck: " + primbodycheck);
        }
        sprite = GetComponent<SpriteRenderer>();
        trail = GetComponent<TrailRenderer>();
        trail.enabled = false;
        color = new Color(color.r,color.g,color.b,1.0f);
        sprite.color = color;
        Gradient gradient = new Gradient();
        gradient.SetKeys(new GradientColorKey[] {new GradientColorKey(color, 0.0f), new GradientColorKey(Color.white, 1.0f)}, new GradientAlphaKey[] {new GradientAlphaKey(1.0f, 0.8f), new GradientAlphaKey(0.0f, 1.0f)});
        trail.colorGradient = gradient;
        trail.enabled = true;
        BodyScale = Mathf.Sqrt(PrimaryBody.radius*radius)/PrimaryBody.radius;

        if(primbodycheck==false)
        {
            h = Vector3.Cross(controller.polartocartesian(r_i, f_i),v_i);
            print(Name + " h: " + h);
            mu = controller.UGC*(mass+PrimaryBody.mass);
            print(Name + " mu: " + mu);
            p = h.magnitude*h.magnitude/mu;
            print(Name + " p: " + p);
            epsilon = 0.5f*v_i.magnitude*v_i.magnitude - mu/r_i;
            a = -mu/(2f*epsilon);
            e = Mathf.Sqrt(1-(p/a));
            SOI_radius = a*Mathf.Pow(mass/PrimaryBody.mass,2/5)*radius/PrimaryBody.radius;
        }
    }

    void calcposition()
    {
        float M = Mathf.Sqrt(mu/(a*a*a))*controller.time;
        float Ei = M;
        float E = M;
        for(int i = 0; i<6; i++)
        {
            Ei = E;
            float numerator = Ei - e*Mathf.Sin(Ei) - M;
            float denomninator = 1 - e*Mathf.Cos(Ei);
            E = Ei - numerator/denomninator;
        }
        float num = 1+e;
        float den = 1-e;
        float term = Mathf.Sqrt(num/den)*Mathf.Tan(E/2);
        f = Mathf.Abs(Mathf.Rad2Deg*2*Mathf.Atan(term));
        print(Name + f);
        r = a*(1-e*Mathf.Cos(E));
    }

    void Update()
    {
        SOI.transform.localScale = new Vector3(1,1,1)*SOI_radius*controller.ScaleFactor;
        transform.localScale = new Vector3(1,1,1)*BodyScale;
        calcposition();
        if(primbodycheck)
        {
            transform.position = new Vector3(0.0f,0.0f,0.0f);
        }
        else
        {
            if(ParentBodyOverride==null)
            {
                transform.position = controller.polartocartesian(r, f)*controller.ScaleFactor;
            }
            else
            {
                transform.position = controller.polartocartesian(r, f)*controller.ScaleFactor + ParentBodyOverride.transform.position;
            }
            
        }
    }
}
