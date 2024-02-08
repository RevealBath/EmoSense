using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Test : MonoBehaviour
{
    public AudioSource coinSource;
    public void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Cube")
        {
            coinSource.Play();
        }
    }
}
