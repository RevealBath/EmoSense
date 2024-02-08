using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BluetoothLEHeartRate : MonoBehaviour
{
    #region Blutooth Le Info

    private string _selectedDevice;
    private string _selectedService;
    private List<string> _foundCharacteristics;
    private const string HeartRateCharacteristicID = "2A37";

    public long totalHeartRate = 0;
    public int heartRateCount = 0;

    public Text bpm;
    

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //print("Running");
        while (HRBleAPI.PollData(out var res, false))
        {
            var hol = res.characteristicUuid.Substring(5, 4).ToUpper();
            print("Hol_HR: "+hol);
            switch (hol)
            {
                case HeartRateCharacteristicID:
                    print("running...");
                    totalHeartRate += Convert.ToInt64(res.buf[1]);
                    heartRateCount += 1;
                    bpm.text = $"Heart Rate: {res.buf[1].ToString()}\n Average: {(float)(totalHeartRate / heartRateCount)}";
                    break;
            }
        }
    }

    #region Public Methods
    public void Initialize(string deviceID, string serviceID, List<string> characteristicsList)
    {
        print("Initialising...");
        _selectedDevice = deviceID;
        _selectedService = serviceID;
        _foundCharacteristics = characteristicsList;
        SubscribeToCharacteristics();
    }
    #endregion

    #region Private Methods

    private void SubscribeToCharacteristics()
    {
        print("Found characteristics: " + _foundCharacteristics);

        foreach (var characteristics in _foundCharacteristics)
        {
            var res = characteristics.Substring(5, 4).ToUpper();
            switch (res)
            {
                case HeartRateCharacteristicID:
                    print("Subscribed");
                    HRBleAPI.SubscribeCharacteristic(_selectedDevice, _selectedService, characteristics, false);
                    break;
            }
        }
    }
    #endregion
}
