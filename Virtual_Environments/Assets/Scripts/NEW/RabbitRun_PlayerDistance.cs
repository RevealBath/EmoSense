using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitRun_PlayerDistance : MonoBehaviour
{
    public Animator rabbitRun;
    private float dist;
    public float distThreshold = 200.0f;
    private GameObject playerCamera;

    // Start is called before the first frame update
    void Start()
    {
        playerCamera = Camera.main.gameObject;
        rabbitRun = GetComponent<Animator>();
        rabbitRun.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        dist = Vector3.Distance(this.transform.position, playerCamera.transform.position);       

        if (!rabbitRun.enabled && dist <= 200.0f)
        {
            rabbitRun.enabled = true;
        }
        else if (rabbitRun.enabled && dist >= 200.0f)
        {
            rabbitRun.enabled = false;
        }
    }
}
