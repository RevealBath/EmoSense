using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeControl : MonoBehaviour
{
    public GameObject fadeEffect;
    float elapsed = 0f;
    public bool userReady = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            userReady = true;
        }

        if (userReady)
        {
            elapsed += Time.deltaTime;

            if (elapsed >= 59.0f)
            {
                fadeEffect.SetActive(true);
                elapsed = -1.0f;
            }
        }
    }
}
