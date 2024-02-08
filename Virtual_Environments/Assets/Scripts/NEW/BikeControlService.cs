using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BikeControlService : MonoBehaviour
{
    // Bike name.
    public string BikeDeviceName = "KICKR BIKE";

    // Restarts connection process.
    public bool startScan = true;

    // Used to indicate the stage of the connection process.
    public bool isSubscribed = false;

    private bool isScanningDevices = false;
    private bool isScanningServices = false;
    private bool isScanningCharacteristics = false;

    // Stores connected strings.
    private string selectedDeviceId;
    private string selectedServiceId;
    private string selectedCharacteristicId;

    // Displays speed in Debug screen.
    //public TMP_Text speed;

    // Speed and cadance available for player model to move and interact.
    public float speed_kmh = 0;
    public float rpm = 0;
    public float power = 0;

    private UnityBikeData ubd;

    // Connection details as per Bluetooth Assigned Numbers
    // https://btprodspecificationrefs.blob.core.windows.net/assigned-numbers/Assigned%20Number%20Types/Assigned_Numbers.pdf
    private const string BikeServiceID = "1826";
    private const string BikeCharacteristicID = "2AD2";

    // Holds detected devices.
    Dictionary<string, string> devices = new Dictionary<string, string>();

    void Start() {
        //speed.text = "Speed: 0 km/h";
        StartCoroutine(RefreshUntilConnection());
    }

    // Update is called once per frame
    void Update()
    {
        BikeAPI.ScanStatus status;

        if (startScan) {
            StartStopDeviceScan();
        }

        // Attempts to connect to any devices containing the Bike Device Name defined.
        if (isScanningDevices)
        {
            BikeAPI.DeviceUpdate res = new BikeAPI.DeviceUpdate();
            do
            {
                status = BikeAPI.PollDevice(ref res, false);

                if (!string.IsNullOrEmpty(selectedDeviceId)) {
                    status = BikeAPI.ScanStatus.FINISHED;
                    StartStopDeviceScan();
                    if (string.IsNullOrEmpty(selectedServiceId)) StartServiceScan();
                }

                if (status == BikeAPI.ScanStatus.AVAILABLE)
                {
                    if (!devices.ContainsKey(res.id)) {
                        devices[res.id] = res.name;
                        if (res.nameUpdated && res.name.Contains(BikeDeviceName)) selectedDeviceId = res.id;
                    }
                }
                else if (status == BikeAPI.ScanStatus.FINISHED)
                {
                    isScanningDevices = false;
                }
            } while (status == BikeAPI.ScanStatus.AVAILABLE);
        }

        // Attempts to connect to any services containing the services string defined.
        if (isScanningServices)
        {
            BikeAPI.Service res = new BikeAPI.Service();
            do
            {
                status = BikeAPI.PollService(out res, false);

                if (!string.IsNullOrEmpty(selectedServiceId)) {
                    status = BikeAPI.ScanStatus.FINISHED;
                    if (string.IsNullOrEmpty(selectedCharacteristicId)) StartCharacteristicScan();
                }

                if (status == BikeAPI.ScanStatus.AVAILABLE)
                {
                    if (res.uuid.Substring(5, 4).ToUpper() == BikeServiceID) {
                        selectedServiceId = res.uuid;
                    }
                }
                else if (status == BikeAPI.ScanStatus.FINISHED)
                {
                    isScanningServices = false;
                }
            } while (status == BikeAPI.ScanStatus.AVAILABLE);
        }

        // Attempts to connect to any characteristics containing the characteristics string defined.
        if (isScanningCharacteristics)
        {
            BikeAPI.Characteristic res = new BikeAPI.Characteristic();
            do
            {
                status = BikeAPI.PollCharacteristic(out res, false);

                if (!string.IsNullOrEmpty(selectedCharacteristicId)) {
                    status = BikeAPI.ScanStatus.FINISHED;
                    if (!isSubscribed) Subscribe();
                }

                if (status == BikeAPI.ScanStatus.AVAILABLE)
                {
                    if (res.uuid.Substring(5, 4).ToUpper() == BikeCharacteristicID) {
                        selectedCharacteristicId = res.uuid;
                    }
                }
                else if (status == BikeAPI.ScanStatus.FINISHED)
                {
                    isScanningCharacteristics = false;
                }
            } while (status == BikeAPI.ScanStatus.AVAILABLE);
        }

        // When subscribed it receives a Byte array as defined in BikeAPI script.
        if (isSubscribed) {

            BikeAPI.BLEData res = new BikeAPI.BLEData();
            while (BikeAPI.PollData(out res, false))
            {
                // https://stackoverflow.com/questions/64002583/decode-bluetooth-data-from-the-indoor-bike-data-characteristic
                // Speed in KM/H
                speed_kmh = (float) BitConverter.ToUInt16(res.buf, 2) / 100f;

                // Cadence in Rotations per Minute
                rpm = (float) BitConverter.ToUInt16(res.buf, 4) * 0.5f;

                // Power in Watts
                power = (float)BitConverter.ToInt16(res.buf, 6);
                
                ubd.speed_kmh = speed_kmh;
                ubd.rpm = rpm;
                ubd.power = power;
            }

        }
    }

    public UnityBikeData GetLatestBikeData() {  return ubd; }

    // Starts and stops device scan.
    private void StartStopDeviceScan()
    {
        if (!isScanningDevices)
        {
            // start new scan
            BikeAPI.StartDeviceScan();
            isScanningDevices = true;
            startScan = false;
        }
        else
        {
            // stop scan
            isScanningDevices = false;
            BikeAPI.StopDeviceScan();
        }
    }

    // Starts services scan.
    private void StartServiceScan()
    {
        if (!isScanningServices)
        {
            // start new scan
            BikeAPI.ScanServices(selectedDeviceId);
            isScanningServices = true;
        }
    }

    // Starts characteristic scan.
    private void StartCharacteristicScan()
    {
        if (!isScanningCharacteristics)
        {
            // start new scan
            BikeAPI.ScanCharacteristics(selectedDeviceId, selectedServiceId);
            isScanningCharacteristics = true;
        }
    }

    // Subscribes to machine.
    private void Subscribe() {
        BikeAPI.SubscribeCharacteristic(selectedDeviceId, selectedServiceId, selectedCharacteristicId, false);
        isSubscribed = true;
    }

    // Every 10 seconds, if it still isn't sending data it restarts the scan.
    IEnumerator RefreshUntilConnection() {
        while (!isSubscribed) {
            yield return new WaitForSeconds(10);
            if (!isSubscribed) startScan = true;
        }
    }

    // Stops API on quit.
    private void OnApplicationQuit()
    {
        BikeAPI.Quit();
    }
}

public struct UnityBikeData 
{ 
    public float speed_kmh { get; set; }
    public float rpm { get; set; }
    public float power { get; set; }
}
