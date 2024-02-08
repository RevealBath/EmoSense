using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitStarter2 : MonoBehaviour
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
            if (timePassed > 5.0f)
            {
                rabbitRun.enabled = true;
            }   
        }
    }
    
    public void PlayPressed()
    {
        play = true;
    }
}
