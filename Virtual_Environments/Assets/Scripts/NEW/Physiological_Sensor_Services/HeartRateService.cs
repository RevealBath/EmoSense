using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeartRateService : MonoBehaviour
{
    // Heart rate sensor name.
    public string HeartRateDeviceName = "Polar H10";

    // Connection details as per Bluetooth Assigned Numbers
    // https://btprodspecificationrefs.blob.core.windows.net/assigned-numbers/Assigned%20Number%20Types/Assigned_Numbers.pdf
    private const string HeartRateServiceID = "180D";
    private const string HeartRateCharacteristicID = "2A37";

    // Restarts connection process.
    public bool startScan = true;

    // Used to indicate the stage of the connection process.
    public bool isSubscribed = false;

    private bool isScanningDevices = false;
    private bool isScanningServices = false;
    private bool isScanningCharacteristics = false;

    private string selectedDeviceId;
    private string selectedServiceId;
    private string selectedCharacteristicId;

    // Heart rate available for the adaptation of scenes.
    [HideInInspector]
    public int heartBeatsPerMinute = 0;
    [HideInInspector]
    public int heartRateAverage = 0;
    [HideInInspector]
    public int heartRateSamples = 0;
    private long totalHeartRate = 0;

    // Holds detected devices.
    Dictionary<string, string> devices = new Dictionary<string, string>();

    // Displays HR in debug screen.
    public string bpm;
    public string HR_RR_Interval;
    HeartRateData hrd;

    void Start() {
        hrd = new HeartRateData();

        bpm = "Heart Rate: 0";
        HR_RR_Interval = "HR Interval: 0";
        StartCoroutine(RefreshUntilConnection());
    }

    // Update is called once per frame
    void Update()
    {
        HeartRateAPI.ScanStatus status;

        if (startScan) {
            StartStopDeviceScan();
        }

        // Attempts to connect to any devices containing the Heart Rate Name Device Name defined.
        if (isScanningDevices)
        {
            HeartRateAPI.DeviceUpdate res = new HeartRateAPI.DeviceUpdate();
            do
            {
                status = HeartRateAPI.PollDevice(ref res, false);

                if (!string.IsNullOrEmpty(selectedDeviceId)) {
                    status = HeartRateAPI.ScanStatus.FINISHED;
                    if (string.IsNullOrEmpty(selectedServiceId)) StartServiceScan();
                }

                if (status == HeartRateAPI.ScanStatus.AVAILABLE)
                {
                    if (!devices.ContainsKey(res.id)) {
                        devices[res.id] = res.name;
                        if (res.nameUpdated && res.name.Contains(HeartRateDeviceName)) selectedDeviceId = res.id;
                    }
                }
                else if (status == HeartRateAPI.ScanStatus.FINISHED)
                {
                    isScanningDevices = false;
                }
            } while (status == HeartRateAPI.ScanStatus.AVAILABLE);
        }
        
        // Attempts to connect to any services containing the services string defined.
        if (isScanningServices)
        {
            HeartRateAPI.Service res = new HeartRateAPI.Service();
            do
            {
                status = HeartRateAPI.PollService(out res, false);

                if (!string.IsNullOrEmpty(selectedServiceId)) {
                    status = HeartRateAPI.ScanStatus.FINISHED;
                    if (string.IsNullOrEmpty(selectedCharacteristicId)) StartCharacteristicScan();
                }

                if (status == HeartRateAPI.ScanStatus.AVAILABLE)
                {
                    if (res.uuid.Substring(5, 4).ToUpper() == HeartRateServiceID) {
                        selectedServiceId = res.uuid;
                    }
                }
                else if (status == HeartRateAPI.ScanStatus.FINISHED)
                {
                    isScanningServices = false;
                }
            } while (status == HeartRateAPI.ScanStatus.AVAILABLE);
        }

        // Attempts to connect to any characteristics containing the characteristics string defined.
        if (isScanningCharacteristics)
        {
            HeartRateAPI.Characteristic res = new HeartRateAPI.Characteristic();
            do
            {
                status = HeartRateAPI.PollCharacteristic(out res, false);

                if (!string.IsNullOrEmpty(selectedCharacteristicId)) {
                    status = HeartRateAPI.ScanStatus.FINISHED;
                    if (!isSubscribed) Subscribe();
                    startScan = false;
                }

                if (status == HeartRateAPI.ScanStatus.AVAILABLE)
                {
                    if (res.uuid.Substring(5, 4).ToUpper() == HeartRateCharacteristicID) {
                        selectedCharacteristicId = res.uuid;
                    }
                }
                else if (status == HeartRateAPI.ScanStatus.FINISHED)
                {
                    isScanningCharacteristics = false;
                }
            } while (status == HeartRateAPI.ScanStatus.AVAILABLE);
        }

        // When subscribed it receives a Byte array as defined in BikeAPI script.
        if (isSubscribed)
        {
            HeartRateAPI.BLEData res = new HeartRateAPI.BLEData();
            

            while (HeartRateAPI.PollData(out res, false))
            {
                //bool hasRRIntervalData = (flags & 0x10) != 0; // Check bit 4 for RR-Interval data presence.
                // HR added to total for average HR.
                totalHeartRate += Convert.ToInt64(res.buf[1]);
                heartRateSamples += 1;

                // https://stackoverflow.com/questions/64002583/decode-bluetooth-data-from-the-indoor-bike-data-characteristic
                // HR in beats per minute.
                heartBeatsPerMinute = (int)res.buf[1];
                heartRateAverage = (int)(totalHeartRate / heartRateSamples);

                hrd.heartRateBPM = (int)res.buf[1];
                bpm = $"Heart Rate: {res.buf[1].ToString()}, Average: {(float)(totalHeartRate / heartRateSamples)}";

                hrd.heartRate_RR_Interval = res.buf[3] << 8 | res.buf[2];
                HR_RR_Interval = $"Heart Rate Interval: {hrd.heartRate_RR_Interval}";
            }
        }
    }

    // Starts and stops device scan.
    public void StartStopDeviceScan()
    {
        if (!isScanningDevices)
        {
            // start new scan
            HeartRateAPI.StartDeviceScan();
            isScanningDevices = true;
	    startScan = false;
        }
        else
        {
            // stop scan
            isScanningDevices = false;
            HeartRateAPI.StopDeviceScan();
        }
    }

    // Starts services scan.
    public void StartServiceScan()
    {
        if (!isScanningServices)
        {
            // start new scan
            HeartRateAPI.ScanServices(selectedDeviceId);
            isScanningServices = true;
        }
    }

    // Starts characteristic scan.
    public void StartCharacteristicScan()
    {
        if (!isScanningCharacteristics)
        {
            // start new scan
            HeartRateAPI.ScanCharacteristics(selectedDeviceId, selectedServiceId);
            isScanningCharacteristics = true;
        }
    }

    // Subscribes to machine.
    public void Subscribe() {
        HeartRateAPI.SubscribeCharacteristic(selectedDeviceId, selectedServiceId, selectedCharacteristicId, false);
        isSubscribed = true;
    }

    // Every 10 seconds, if it still isn't sending data it restarts the scan.
    IEnumerator RefreshUntilConnection() {
        while (!isSubscribed) {
            yield return new WaitForSeconds(10);
            if (!isSubscribed) startScan = true;
        }
    }

    public HeartRateData getLatestHeartRateData() { return hrd; }

    // Stops API on quit.
    private void OnApplicationQuit()
    {
        HeartRateAPI.Quit();
    }
}

/***
 * Data Packet for HeartRate Data for internal Unity Scripts to use. 
 */
public struct HeartRateData
{    
    public int heartRateBPM { get; set; } //Polar_HearRateBPM
    public float heartRate_RR_Interval { get; set; } //Polar_HearRate_interval
}