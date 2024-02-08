using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceSiren : MonoBehaviour
{
    
    public AudioSource policeInterceptor;
    public AudioSource dogsBarking;
    float elapsed = 0f;
    public bool play = false;
    private bool first = false;
    private bool second = false;

    // Update is called once per frame
    void Update()
    {
        if (play)
        {
            elapsed += Time.deltaTime;

            if (elapsed >= 10.0f && elapsed <= 24.0f)
            {
                if (first == false)
                {
                    policeInterceptor.Play();
                    first = true;
                }

                if (policeInterceptor.volume <= 1 && elapsed <= 16.0f)
                {
                    policeInterceptor.volume += 0.005f;
                }

                if (elapsed >= 16.0f)
                {
                    policeInterceptor.volume -= 0.01f;
                }
                
                transform.Translate(Vector3.right * 2.0f, Space.World);
            }

            if (elapsed >= 24.0f && elapsed <= 55.0f)
            {
                transform.Translate(Vector3.right * 2.0f, Space.World);
                if (second == false)
                {
                    policeInterceptor.Stop();
                    dogsBarking.Play();
                    second = true;
                }
            }

            if (elapsed >= 55.0f)
            {
                if (second)
                {
                    dogsBarking.Stop();
                    second = false;
                }
            }
        }
    }
}
