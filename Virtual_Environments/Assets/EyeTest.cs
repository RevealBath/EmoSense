using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeTest : MonoBehaviour
{
    public bool finishedTest;

    // Start is called before the first frame update
    void Start()
    {
        finishedTest = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !finishedTest)
        {
            finishedTest = true;
            this.gameObject.SetActive(false);
        }            
    }
}
