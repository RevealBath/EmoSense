using Tobii.XR;
using UnityEngine;

public class MyBootstrapper : MonoBehaviour
{
    void Awake()
    {
        var settings = new TobiiXR_Settings();
        TobiiXR.Start(settings);
    }
}
