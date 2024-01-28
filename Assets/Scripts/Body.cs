using System.Collections;
using System.Collections.Generic;
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
    public float f;
    public Vector3 v;
    private float a;
    private Vector3 h;
    private float p;
    private float mu;
    private float epsilon;
    private float e;

    void Awake()
    {
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
            transform.localScale = new Vector3(1,1,1)*radius/PrimaryBody.radius;
        }
    }

    void calcposition()
    {
        r = p/(1+e*Mathf.Cos(Mathf.Deg2Rad*f))*controller.ScaleFactor;
    }

    void FixedUpdate()
    {
        if(primbodycheck)
        {
            transform.position = new Vector3(0.0f,0.0f,0.0f);
        }
        else
        {
            f = controller.time;
            calcposition();
            if(ParentBodyOverride==null)
            {
                transform.position = controller.polartocartesian(r, f);
            }
            else
            {
                transform.position = controller.polartocartesian(r, f) + ParentBodyOverride.transform.position;
            }
            
        }
    }

}
