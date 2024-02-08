using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMovement : MonoBehaviour
{
    public float coefficient;
    public BikeControlService bcs;
    UnityBikeData ubd;
    float Current_Speed;
    public Vector3 translate_increment;
    public float pub_diff_rpm;


    // Start is called before the first frame update
    void Start()
    {
        coefficient = BikeMovement_Neutral.coefficient;
    }

    // Update is called once per frame
    void Update()
    {
        coefficient = BikeMovement_Neutral.coefficient;

        ubd = bcs.GetLatestBikeData();
        Current_Speed = coefficient * ubd.rpm; //meters per second
        pub_diff_rpm = ubd.rpm - BikeMovement_Neutral.bike_rpm;
    }

    private void FixedUpdate()
    {
        translate_increment = (Vector3.left * Current_Speed) * Time.deltaTime;
        transform.Translate(translate_increment, Space.World); //m per second
    }
}
