using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class Body : MonoBehaviour
{
    [SerializeField]
    public string Name;
    [SerializeField]
    private Color color;
    private TrailRenderer trail;
    private SpriteRenderer sprite;
    public float mass;
    [SerializeField]
    private float radius;
    public bool PrimaryBody;
    [SerializeField]
    private float perigee;
    [SerializeField]
    private float vatperigee;
    [HideInInspector]
    public Vector2 position;
    private SimController controller;
    private Vector2 x;
    private Vector2 v;
    private float time;

    void Awake()
    {
        controller = FindObjectOfType<SimController>();
        sprite = GetComponent<SpriteRenderer>();
        trail = GetComponent<TrailRenderer>();
        //trail.enabled = false;
        color = new Color(color.r,color.g,color.b,1.0f);
        sprite.color = color;
        Gradient gradient = new Gradient();
        gradient.SetKeys(new GradientColorKey[] {new GradientColorKey(color, 0.0f), new GradientColorKey(Color.white, 1.0f)}, new GradientAlphaKey[] {new GradientAlphaKey(1.0f, 0.8f), new GradientAlphaKey(0.0f, 1.0f)});
        trail.colorGradient = gradient;
        x = new Vector2(1,0)*perigee*controller.ScaleFactor;
        transform.position = x;
        v = new Vector2(0,1)*vatperigee*controller.ScaleFactor;
        transform.localScale = radius/controller.PrimaryBody.radius*new Vector3(1,1,1);

        
        trail.enabled = true;
    }

    void FixedUpdate()
    {
        time = Time.deltaTime*controller.TimeScale;
        Vector2 Force = new Vector2(0.0f,0.0f);
        foreach(Body i in controller.realbodies)
        {
            if(GetComponent<Body>()==i)
            {
                continue;
            }
            else
            {
                float rmag = Vector2.Distance(transform.position,i.transform.position)/controller.ScaleFactor;
                //print(Name+"-"+i.Name+" rmag:"+rmag);
                Vector2 r = (i.transform.position-transform.position)/controller.ScaleFactor;
                //print(Name+"-"+i.Name+" r:"+r);
                Vector2 rhat = r/rmag;
                float Fmag = controller.UGC*mass*i.mass/Mathf.Pow(rmag,2);
                Vector2 F = rhat*Fmag;
                Force = Force+F;
            }
        }
        Vector2 a = (Force/mass)*controller.ScaleFactor;
        transform.position = x;
        transform.position -= controller.PrimaryBody.transform.position;
        v += a*time;
        x += v*time+0.5f * Mathf.Pow(time,2) * a;
    }
}
