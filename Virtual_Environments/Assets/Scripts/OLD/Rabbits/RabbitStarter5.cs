using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitStarter5 : MonoBehaviour
{
    public Animator rabbitRun;
    public float timePassed = 0f;
    public bool play = false;
    
    // Start is called before the first frame update
    void Start()
    {
        rabbitRun = GetComponent<Animator>();
        rabbitRun.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (play)
        {
            timePassed += Time.deltaTime;
            if (timePassed >= 40.0f && timePassed <= 50.0f)
            {
                rabbitRun.enabled = true;
            }
            if (timePassed >= 55.0f)
            {
                rabbitRun.enabled = false;
            }
        }
    }
    
    public void PlayPressed()
    {
        play = true;
    }
}
