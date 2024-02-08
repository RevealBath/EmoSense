using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BluetoothDiscovery : MonoBehaviour
{
    #region Public Members

    public Text deviceScanStatusText;
    public Text devicePairStatusText;
    public Text deviceServiceStatusText;
    public GameObject deviceScanResultProto;
    public GameObject ble_Bike;
    public Text errorText;
    public Text connectionMessage;

    public string bikeBLE_Name; //set to KICKR BIKE
    public string bikeBLE_ServiceID; //set to 1818
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
        if (_isScanningDevices) { pollDevices(); }

        if (_isScanningServices) { pollServices(); }

        if (_isScanningCharacteristics) { pollCharacteristics(); }

        {
            // log potential errors
            BleAPI.GetError(out var res);
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
            for (var i = _scanResultRoot.childCount - 1; i >= 0; i--)
                Destroy(_scanResultRoot.GetChild(i).gameObject);
            BleAPI.StartDeviceScan();
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
            BleAPI.StopDeviceScan();
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

    public void ConnectBikeDevice()
    {
        if (_selectedDeviceName.Contains(bikeBLE_Name))
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

    public string getSelectedDeviceName() { return _selectedDeviceName; }
    #endregion

    #region Private Methods

    private void pollDevices()
    {
        BleAPI.ScanStatus status;
        var res = new BleAPI.DeviceUpdate();
        do
        {
            status = BleAPI.PollDevice(ref res, false);
            if (status == BleAPI.ScanStatus.AVAILABLE)
            {
                if (!_devices.ContainsKey(res.id))
                {
                    _devices[res.id] = new Dictionary<string, string>()
                        {
                            { "name", "" },
                            { "isConnectable", "False" }
                        };
                }

                if (res.nameUpdated)
                {
                    _devices[res.id]["name"] = res.name;
                }

                if (res.isConnectableUpdated)
                    _devices[res.id]["isConnectable"] = res.isConnectable.ToString();
                // consider only devices which have a name and which are connectable
                if (_devices[res.id]["name"] != "" && _devices[res.id]["isConnectable"] == "True")
                {
                    // add new device to list
                    GameObject g = Instantiate(deviceScanResultProto, _scanResultRoot);
                    g.name = res.id;
                    g.transform.localScale = new Vector3(1, 1, 1);
                    g.transform.GetChild(0).GetComponent<Text>().text = _devices[res.id]["name"];
                    g.transform.GetChild(1).GetComponent<Text>().text = res.id;
                }
            }
            else if (status == BleAPI.ScanStatus.FINISHED)
            {
                _isScanningDevices = false;
                deviceScanStatusText.text = "FINISHED";
            }
        } while (status == BleAPI.ScanStatus.AVAILABLE);
    }

    private void pollServices()
    {
        BleAPI.ScanStatus status;
        do
        {
            status = BleAPI.PollService(out var res, false);
            switch (status)
            {
                case BleAPI.ScanStatus.AVAILABLE:
                    //Debug.Log("Services: " + res.uuid);
                    _serviceList.Add(res.uuid);
                    break;
                case BleAPI.ScanStatus.FINISHED:
                    _isScanningServices = false;
                    SelectService();
                    break;
            }
        } while (status == BleAPI.ScanStatus.AVAILABLE);
    }

    private void pollCharacteristics()
    {
        BleAPI.ScanStatus status;
        do
        {
            status = BleAPI.PollCharacteristic(out var res, false);
            switch (status)
            {
                case BleAPI.ScanStatus.AVAILABLE:
                    Debug.Log(res.uuid);
                    _characteristicsList.Add(res.uuid);
                    break;
                case BleAPI.ScanStatus.FINISHED:
                    _isScanningCharacteristics = false;
                    Debug.Log("Characteristics Scan Finished");
                    FinishCharacteristicsSearch();
                    break;
            }
        } while (status == BleAPI.ScanStatus.AVAILABLE);
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
            BleAPI.ScanServices(_selectedDeviceId);
            _isScanningServices = true;
            deviceServiceStatusText.text = "SCANNING";
        }
    }

    private void SelectService()
    {
        if (_selectedDeviceName.Contains(bikeBLE_Name))
        {
            GetServices(bikeBLE_ServiceID);
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

            //_selectedServiceId = res switch
            //{
            //    // Fitness Machine: 0x1826
            //    //"1826" => service,
            //    // Cycling Speed and Cadence: 0x1816
            //    //"1816" => service,
            //    // Cycling Power: 0x1818
            //    "1818" => service,
            //    // Running Speed and Cadence: 0x1814
            //    //"1814" => service,
            //    _ => _selectedServiceId
            //};
        }

        //print(_selectedServiceId);
        deviceServiceStatusText.text = "SERVICE FOUND: " + _selectedServiceId;
        StartCharacteristicScan(_selectedServiceId);
    }

    private void StartCharacteristicScan(string serviceId)
    {
        if (!_isScanningCharacteristics)
        {
            // start new scan
            _characteristicsList.Clear();
            BleAPI.ScanCharacteristics(_selectedDeviceId, serviceId);
            _isScanningCharacteristics = true;
        }
    }

    private void FinishCharacteristicsSearch()
    {
        //var res = _selectedServiceId.Substring(5, 4).ToUpper();
        //bluetoothLeAccess.GetComponent<BluetoothLeCyclePower>().Initialize(_selectedDeviceId, _selectedServiceId, _characteristicsList);
        //print("Device ID: "+ _selectedDeviceId);
        //print("SelectedService ID: "+ _selectedServiceId);
        //print("Characteristics List: "+ _characteristicsList[0]);
        //Debug.Log("RES: "+res);

        if (_selectedDeviceName.Contains(bikeBLE_Name))
        {
            connectionMessage.text = "Connection to Bike Established!";
            //turn button green
            ble_Bike.GetComponent<BluetoothLeCyclePower>().Initialize(_selectedDeviceId, _selectedServiceId, _characteristicsList);
        }

        //bluetoothLeAccess.GetComponent<BluetoothLeCyclePower>().Initialize(_selectedDeviceId, _selectedServiceId, _characteristicsList);
        //switch (res)
        //{
        //    // Fitness Machine: 0x1826
        //    case "1826":
        //        //bluetoothLeAccess.GetComponent<BluetoothLeFitnessMachine>().Initialize(_selectedDeviceId,_selectedServiceId,_characteristicsList);
        //        break;
        //    // Cycling Speed and Cadence: 0x1816
        //    case "1816":
        //        //bluetoothLeAccess.GetComponent<BluetoothLeCycleSpeedAndCadence>().Initialize(_selectedDeviceId,_selectedServiceId,_characteristicsList);
        //        break;
        //    // Cycling Power: 0x1818
        //    case "1818":
        //        bluetoothLeAccess.GetComponent<BluetoothLeCyclePower>().Initialize(_selectedDeviceId,_selectedServiceId,_characteristicsList);
        //        break;
        //    // Running Speed and Cadence: 0x1814
        //    case "1814":
        //        //bluetoothLeAccess.GetComponent<BluetoothLeRunningSpeedAndCadence>().Initialize(_selectedDeviceId,_selectedServiceId,_characteristicsList);
        //        break;
        //}
    }
    
    private void OnApplicationQuit()
    {
        BleAPI.Quit();
    }
    #endregion
}
