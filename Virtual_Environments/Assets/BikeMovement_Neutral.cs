using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeMovement_Neutral : MonoBehaviour
{
    public BikeControlService bcs;
    public StudyManager sm;
    public WarmUpManager wm;

    GameObject NextTerrain;
    
    UnityBikeData ubd;
    Terrain terrain;
    public float Target_Speed;
    public float Percentage_Target_Speed;
    public float Percentage_Target_HR;
    public float Current_Speed;
    public static float coefficient = 0.1f;
    public static float bike_rpm;
    public float pub_bike_rpm;
    public float pub_coefficient;

    public bool first;

    public delegate void SpeedCoefficientReady(float speedCoefficient);
    public static event SpeedCoefficientReady OnSpeedCoefficientReady;

    Vector3 endPos = new Vector3(-6500, -4, -500);

    // Start is called before the first frame update
    void Start()
    {
        terrain = this.GetComponent<Terrain>();
        Target_Speed = ((terrain.terrainData.size.x/2) / sm.exposureSceneTime) / 1000; // KM per Seconds
        first = true;
    }

    // Update is called once per frame
    void Update()
    {


        if (!wm.runHR_Warmup && !first)
        {
            first = true;
            return;
        }

        if (!wm.runHR_Warmup)
            return;

        if (first)
        {
            NextTerrain = sm.GetNextScene();
            Target_Speed = 0.85f * NextTerrain.GetComponent<Terrain>().terrainData.size.x / sm.exposureSceneTime; // meters per Second
            first = false;
        }

        Current_Speed = coefficient * ubd.rpm; //meters per second

        Percentage_Target_Speed = (Current_Speed / Target_Speed) * 100;
        Percentage_Target_HR = wm.percentage_target_HR_Mid_Point;

        //if (Percentage_Target_Speed < Percentage_Target_HR)
        //    coefficient += 0.005f;
        //else
        //    coefficient -= 0.005f;

        if(Percentage_Target_Speed > 100.0)
        {
            coefficient -= 0.005f;
        }
        else
        {
            coefficient += 0.005f;
        }

        pub_coefficient = coefficient;
        bike_rpm = ubd.rpm;
        pub_bike_rpm = ubd.rpm;
    }

    private void FixedUpdate()
    {
        ubd = bcs.GetLatestBikeData();
        Current_Speed = coefficient * ubd.rpm; //meters per second
        transform.Translate((Vector3.left * Current_Speed)*Time.deltaTime, Space.World); //m per second

        if (transform.position.x < endPos.x)
            transform.localPosition = new Vector3(-500f,-4,-500);
    }
}
