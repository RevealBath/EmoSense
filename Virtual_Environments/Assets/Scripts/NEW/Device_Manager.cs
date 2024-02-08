using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Device_Manager : MonoBehaviour
{
    public BikeControlService bcs;
    public SkinConductanceService scs;
    public HeartRateService hrs;

    private bool devicesReady;
    public delegate void DevicesConnected();
    public static event DevicesConnected OnDevicesConnected;

    // Start is called before the first frame update
    void Start()
    {
        devicesReady = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!devicesReady)
        {
            if(bcs.isSubscribed && hrs.isSubscribed && scs.isStreaming)
            {
                OnDevicesConnected();
                devicesReady = true;
            }
        }
    }
}
