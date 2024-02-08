using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeutralEnvironmentSpawn : MonoBehaviour
{
    public bool userReady = false;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            userReady = true;
        }
        
        if (userReady)
        {
            transform.Translate(Vector3.left * 0.3f, Space.World);
        }
    }
}
