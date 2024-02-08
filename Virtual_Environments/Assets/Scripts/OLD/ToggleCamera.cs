using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleCamera : MonoBehaviour
{
    public Camera VRCamera;
    public Camera VRCamera2;
    public GameObject Player;
    public GameObject VR_Rig;

    private void Start()
    {
        VRCamera.enabled = true;
        Player.SetActive(true);
        VRCamera2.enabled = false;
        VR_Rig.SetActive(false);
    }

    public void switchCamera(int x)
    {
        if (x == 1)
        {
            VRCamera2.enabled = true;
            VR_Rig.SetActive(true);
            VRCamera.enabled = false;
            Player.SetActive(false);
        }
    }
}
