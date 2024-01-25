using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimController : MonoBehaviour
{
    public Body[] bodies;
    public float ScaleFactor;
    public float TimeScale;
    [HideInInspector]
    public Body PrimaryBody;
    public List<Body> realbodies = new List<Body> {};
    public float UGC = (float)6.674*Mathf.Pow(10,-11);
    // Start is called before the first frame update
    void Start()
    {
        foreach(Body i in bodies)
        {
            Body SpawnedBody = Instantiate(i);
            realbodies.Add(SpawnedBody);
            if(i.PrimaryBody==true)
            {
                if(PrimaryBody==null)
                {
                    PrimaryBody = SpawnedBody;
                }
                else
                {
                    i.PrimaryBody=false;
                }
            }
            
        }
    }
}
