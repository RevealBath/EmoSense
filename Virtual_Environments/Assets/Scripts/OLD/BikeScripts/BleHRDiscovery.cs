using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BleHRDiscovery : MonoBehaviour
{
    #region Public Members

    public Text deviceScanStatusText;
    public Text devicePairStatusText;
    public Text deviceServiceStatusText;
    public GameObject deviceScanResultProto;
    public GameObject ble_HR_Monitor;
    public Text errorText;
    public Text connectionMessage;

    public BluetoothDiscovery bd;

    public string hearRateBLE_Name; //set to Polar H10
    public string heartRateBLE_ServiceID; //set to 180D
    #endregion


    #region Private Members

    private bool _isScanningDevices;
    private bool _isScanningServices;
    private bool _isScanningCharacteristics;
    private Transform _scanResultRoot;
    private string _selectedDeviceName;
    private string _selectedDeviceId;
    private string _selectedServiceId;
    private List<string> _serviceList;
    private string _selectedCharacteristicId;
    private List<string> _characteristicsList;
    private Dictionary<string, Dictionary<string, string>> _devices;
    private string _lastError;

    #endregion

    #region Event Methods

    // Start is called before the first frame update
    void Start()
    {
        _isScanningDevices = false;
        _isScanningServices = false;
        _isScanningCharacteristics = false;
        _devices = new Dictionary<string, Dictionary<string, string>>();
        _serviceList = new List<string>();
        _characteristicsList = new List<string>();
        _scanResultRoot = deviceScanResultProto.transform.parent;
        deviceScanResultProto.transform.SetParent(null);
    }

    // Update is called once per frame
    void Update()
    {
        //if (_isScanningDevices) { pollDevices(); }

        if (_isScanningServices) { pollServices(); }

        if (_isScanningCharacteristics) { pollCharacteristics(); }

        {
            // log potential errors
            HRBleAPI.GetError(out var res);
            if (_lastError != res.msg)
            {
                Debug.LogError(res.msg);
                errorText.text = res.msg;
                _lastError = res.msg;
            }
        }
    }
    #endregion

    #region Public Methods
    
    public void StartDeviceScan()
    {
        if (!_isScanningDevices)
        {
            // start new scan
            //for (var i = _scanResultRoot.childCount - 1; i >= 0; i--)
            //    Destroy(_scanResultRoot.GetChild(i).gameObject);
            HRBleAPI.StartDeviceScan();
            _isScanningDevices = true;
            deviceScanStatusText.text = "SCANNING";
        }
    }

    public void StopDeviceScan()
    {
        if (_isScanningDevices)
        {
            // stop scan
            _isScanningDevices = false;
            HRBleAPI.StopDeviceScan();
            deviceScanStatusText.text = "STOPPED";
        }
    }
    
    public void PairWithDevice(GameObject data)
    {
        devicePairStatusText.text = "READY FOR PAIRING";
        SelectDevice(data);
        //StartServiceScan();
        //devicePairStatusText.text = "PAIRED";
    }

    public void ConnectHRDevice()
    {
        _selectedDeviceName = bd.getSelectedDeviceName(); 

        if (_selectedDeviceName.Contains(hearRateBLE_Name))
        {
            connectionMessage.text = "Correct BLE Found";
            StartServiceScan();
            devicePairStatusText.text = "PAIRED";
        }
        else
        {
            connectionMessage.text = "Incorrect BLE, please select another device";
        }
    }
    #endregion

    #region Private Methods

    //private void pollDevices()
    //{
    //    HRBleAPI.ScanStatus status;
    //    var res = new HRBleAPI.DeviceUpdate();
    //    do
    //    {
    //        status = HRBleAPI.PollDevice(ref res, false);
    //        if (status == HRBleAPI.ScanStatus.AVAILABLE)
    //        {
    //            if (!_devices.ContainsKey(res.id))
    //            {
    //                _devices[res.id] = new Dictionary<string, string>()
    //                    {
    //                        { "name", "" },
    //                        { "isConnectable", "False" }
    //                    };
    //            }

    //            if (res.nameUpdated)
    //            {
    //                _devices[res.id]["name"] = res.name;
    //            }

    //            if (res.isConnectableUpdated)
    //                _devices[res.id]["isConnectable"] = res.isConnectable.ToString();
    //            // consider only devices which have a name and which are connectable
    //            if (_devices[res.id]["name"] != "" && _devices[res.id]["isConnectable"] == "True")
    //            {
    //                // add new device to list
    //                GameObject g = Instantiate(deviceScanResultProto, _scanResultRoot);
    //                g.name = res.id;
    //                g.transform.localScale = new Vector3(1, 1, 1);
    //                g.transform.GetChild(0).GetComponent<Text>().text = _devices[res.id]["name"];
    //                g.transform.GetChild(1).GetComponent<Text>().text = res.id;
    //            }
    //        }
    //        else if (status == HRBleAPI.ScanStatus.FINISHED)
    //        {
    //            _isScanningDevices = false;
    //            deviceScanStatusText.text = "FINISHED";
    //        }
    //    } while (status == HRBleAPI.ScanStatus.AVAILABLE);
    //}

    private void pollServices()
    {
        HRBleAPI.ScanStatus status;
        do
        {
            status = HRBleAPI.PollService(out var res, false);
            switch (status)
            {
                case HRBleAPI.ScanStatus.AVAILABLE:
                    //Debug.Log("Services: " + res.uuid);
                    _serviceList.Add(res.uuid);
                    break;
                case HRBleAPI.ScanStatus.FINISHED:
                    _isScanningServices = false;
                    SelectService();
                    break;
            }
        } while (status == HRBleAPI.ScanStatus.AVAILABLE);
    }

    private void pollCharacteristics()
    {
        HRBleAPI.ScanStatus status;
        do
        {
            status = HRBleAPI.PollCharacteristic(out var res, false);
            switch (status)
            {
                case HRBleAPI.ScanStatus.AVAILABLE:
                    Debug.Log(res.uuid);
                    _characteristicsList.Add(res.uuid);
                    break;
                case HRBleAPI.ScanStatus.FINISHED:
                    _isScanningCharacteristics = false;
                    Debug.Log("Characteristics Scan Finished");
                    FinishCharacteristicsSearch();
                    break;
            }
        } while (status == HRBleAPI.ScanStatus.AVAILABLE);
    }        

    private void SelectDevice(GameObject data)
    {
        for (var i = 0; i < _scanResultRoot.transform.childCount; i++)
        {
            var child = _scanResultRoot.transform.GetChild(i).gameObject;
            //Debug.Log("Device: " + child + " Device Data Type: " + child.GetType());
            child.transform.GetChild(0).GetComponent<Text>().color = child == data ? Color.red :
                deviceScanResultProto.transform.GetChild(0).GetComponent<Text>().color;
        }
        _selectedDeviceName = data.transform.GetChild(0).GetComponent<Text>().text;
        //print("Selected: "+_selectedDeviceName);
;        _selectedDeviceId = data.name; 
    }
    
    private void StartServiceScan()
    {
        if (!_isScanningServices)
        {
            // start new scan
            _serviceList.Clear();
            HRBleAPI.ScanServices(_selectedDeviceId);
            _isScanningServices = true;
            deviceServiceStatusText.text = "SCANNING";
        }
    }

    private void SelectService()
    {
        if (_selectedDeviceName.Contains(hearRateBLE_Name)) 
        {
            print("Selecting service....");
            GetServices(heartRateBLE_ServiceID);
        }
    }

    private void GetServices(string selectedService)
    {
        foreach (var service in _serviceList)
        {
            var res = service.Substring(5, 4).ToUpper();
            print("res: " + res);

            if (res == selectedService)            
                _selectedServiceId = service;
        }
        deviceServiceStatusText.text = "SERVICE FOUND: " + _selectedServiceId;
        StartCharacteristicScan(_selectedServiceId);
    }

    private void StartCharacteristicScan(string serviceId)
    {
        if (!_isScanningCharacteristics)
        {
            // start new scan
            _characteristicsList.Clear();
            HRBleAPI.ScanCharacteristics(_selectedDeviceId, serviceId);
            _isScanningCharacteristics = true;
        }
    }

    private void FinishCharacteristicsSearch()
    {
        if (_selectedDeviceName.Contains(hearRateBLE_Name))
        {
            connectionMessage.text = "Connection to Heart Rate Monitor Established!";
            ble_HR_Monitor.GetComponent<BluetoothLEHeartRate>().Initialize(_selectedDeviceId, _selectedServiceId, _characteristicsList);
            
        }
    }
    
    private void OnApplicationQuit()
    {
        HRBleAPI.Quit();
    }
    #endregion
}
