using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasPosition : MonoBehaviour
{
    TerrainMovement tm;

    // Update is called once per frame
    void Update()
    {
        //transform.Translate(Vector3.right * 0.15f, Space.World);
        tm = this.GetComponentInParent<TerrainMovement>();
    }

    private void FixedUpdate()
    {
        transform.Translate(-tm.translate_increment, Space.World); //m per second
    }
}
