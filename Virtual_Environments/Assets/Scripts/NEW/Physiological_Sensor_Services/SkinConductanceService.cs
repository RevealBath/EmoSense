using System;
using System.IO.Ports;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShimmerAPI;
using TMPro;

public class SkinConductanceService : MonoBehaviour
{
    // New shimmer device.
    private int enabledSensors;
    private ShimmerLogAndStreamSystemSerialPort ShimmerDevice;

    // To know if connected and streaming.
    public bool isConnected = false;
    public bool isStreaming = false;



    [HideInInspector]
    public double gsrConductance;
    [HideInInspector]
    public double resConductance;

    private double SamplingRate = 128;

    private bool gsrSet = false;
    private int IndexGSR;
    private int IndexRes;
    private SensorData dataGSR;
    private SensorData dataResistance;

    // Displays skin conductance in Debug.
    public string skinConductance;
    public string skinResistance;

    public bool bruteForceConnection = false;
    private SkinConductanceData scd;

    void Start()
    {
        scd = new SkinConductanceData();

        // Enable GSR sensor.
        enabledSensors = ((int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_A_ACCEL | (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_GSR | (int)ShimmerBluetooth.SensorBitmapShimmer3.SENSOR_INT_A13);

        // Add shimmer device with empty COM port.
        ShimmerDevice = new ShimmerLogAndStreamSystemSerialPort("Shimmer", "", SamplingRate, 0, ShimmerBluetooth.GSR_RANGE_AUTO, enabledSensors, false, false, false, 1, 0, Shimmer3Configuration.EXG_EMG_CONFIGURATION_CHIP1, Shimmer3Configuration.EXG_EMG_CONFIGURATION_CHIP2, true);
        ShimmerDevice.UICallback += this.HandleEvent;

        // Disconnect any shimmer device to reduce errors.
        ShimmerDevice.Disconnect();

        // cycle through ports available until connection is established.
        foreach (String port in SerialPort.GetPortNames())
        {
            ShimmerDevice.SetShimmerAddress(port);

            bool connect = true; // check to connect one at a time

            if (ShimmerDevice.GetState() != ShimmerBluetooth.SHIMMER_STATE_CONNECTED)
            {
                if (connect)
                {
                    ShimmerDevice.StartConnectThread();
                    //ShimmerDevice.Connect();
                    connect = false;
                }
            }
        }

        //skinConductance.text = "Skin Conductance: 0.0";
    }

    void Update()
    {

        // Set debug display to value.
        if (isStreaming)
        {
            scd.gsrConductance = gsrConductance;
            scd.gsrResistance = resConductance;
            skinConductance = $"Skin Conductance: {gsrConductance}";
            skinResistance = $"Skin Resistance: {resConductance}";
        }

        if (bruteForceConnection)
        {
            foreach (String port in SerialPort.GetPortNames())
            {
                ShimmerDevice.SetShimmerAddress(port);

                bool connect = true; // check to connect one at a time

                if (ShimmerDevice.GetState() != ShimmerBluetooth.SHIMMER_STATE_CONNECTED)
                {
                    if (connect)
                    {
                        ShimmerDevice.StartConnectThread();
                        connect = false;
                    }
                }
            }
        }
    }

    // If event occurs, handle
    public void HandleEvent(object sender, EventArgs args)
    {
        CustomEventArgs eventArgs = (CustomEventArgs)args;
        int indicator = eventArgs.getIndicator();
        // Deals with what has been received.
        switch (indicator)
        {
            // If state change.
            case (int)ShimmerBluetooth.ShimmerIdentifier.MSG_IDENTIFIER_STATE_CHANGE:
                int state = (int)eventArgs.getObject();

                if (state == (int)ShimmerBluetooth.SHIMMER_STATE_CONNECTED)
                {
                    isConnected = true;
                    Debug.Log("Connected");
                    ShimmerDevice.StartStreaming();
                }
                else if (state == (int)ShimmerBluetooth.SHIMMER_STATE_CONNECTING)
                {
                    Debug.Log("Establishing connection");
                }
                else if (state == (int)ShimmerBluetooth.SHIMMER_STATE_NONE)
                {
                    isConnected = false;
                    Debug.Log($"Disconnected from: {ShimmerDevice.GetShimmerAddress()}");
                }
                else if (state == (int)ShimmerBluetooth.SHIMMER_STATE_STREAMING)
                {
                    isStreaming = true;
                    Debug.Log("Streaming");
                }
                break;

            case (int)ShimmerBluetooth.ShimmerIdentifier.MSG_IDENTIFIER_NOTIFICATION_MESSAGE:
                print("Notification: "+ eventArgs.getObject());
                break;
            // If received data.
            case (int)ShimmerBluetooth.ShimmerIdentifier.MSG_IDENTIFIER_DATA_PACKET:
                // this is essential to ensure the object is not a reference
                ObjectCluster objectCluster = new ObjectCluster((ObjectCluster)eventArgs.getObject());
                List<String> names = objectCluster.GetNames();
                List<String> formats = objectCluster.GetFormats();
                List<String> units = objectCluster.GetUnits();
                List<Double> data = objectCluster.GetData();

                // Get GSR Conductance rate and store it.
                if (!gsrSet)
                {
                    IndexGSR = objectCluster.GetIndex(Shimmer3Configuration.SignalNames.GSR_CONDUCTANCE, ShimmerConfiguration.SignalFormats.CAL);
                    IndexRes = objectCluster.GetIndex(Shimmer3Configuration.SignalNames.GSR, ShimmerConfiguration.SignalFormats.CAL);
                    gsrSet = true;
                }

                dataGSR = objectCluster.GetData(IndexGSR);
                dataResistance = objectCluster.GetData(IndexRes);
                gsrConductance = dataGSR.Data;
                resConductance = dataResistance.Data;

                break;

            default:
                break;
        }
    }

    public SkinConductanceData getLatestShimmerData() { return scd; }

    // On quit, stop streaming and disconnect shimmer from COM port.
    private void OnApplicationQuit()
    {
        ShimmerDevice.StopStreaming();
        ShimmerDevice.Disconnect();
    }
}

/***
 * Data Packet for SkinConductance Data for internal Unity Scripts to use. 
 */
public struct SkinConductanceData
{
    //Shimmer_D36A_Timestamp_FormattedUnix
    public double gsrConductance { get; set; } //Shimmer_D36A_GSR_Skin_Conductance_uS
    public double gsrResistance { get; set; } //Shimmer_D36A_GSR_Skin_Resistance_kOhms
    //Shimmer_D36A_GSR_Range_noUnits
}
