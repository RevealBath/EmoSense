using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Linq;

public class BluetoothLeCyclePower : MonoBehaviour
{
    public Slider slider;
    public int oldLoad = 0;
    
    #region Blutooth Le Info

    private string _selectedDevice;
    private string _selectedService;
    private List<string> _foundCharacteristics;

    #endregion

    #region GUI Displayables

    public Text subscribeText;
    public Text speed;
    public Text cadence;
    public Text rpm;
    public Text distance;
    public Text power;

    #endregion

    #region Characteristics IDs
    private const string CyclePowerMeasurementID = "2A63";
    private const string CyclePowerWahooExtensionID = "E005";

    // Full Wahoo Characteristics ID
    private string _wahooExID;
    
    #endregion

    #region Cycle Power Measurement Properties

    private double _speed;
    private double _cadence;
    private double _rpm;
    private double _distance;
    private double _power;

    public int _prevWheelRevs;
    public short _prevWheelTime;
    public bool _prevWheelStaleness = true;
    public short _prevCrankRevs;
    public short _prevCrankTime;
    public short _prevCrankStaleness = -1;

    #endregion
    
    private enum LoadTypes
    {
        None,
        WahooKickr
    }

    #region Extended Charateristics Properties

    private LoadTypes _loadType = LoadTypes.None;
    
    private short _load = 0;
    private short _windSpeed = 0;
    private short _weight = 80;
    private double _rollingResistance = 0.0033;
    private double _windResistance = 0.6;
    public int _wheelSize = 2096; //bike wheel circumference in mm
    private byte _opResult;
    private byte _opCodeRaw;

    public float RiderWeight = 70; // in kg
    public float BikeWeight = 10; // in kg
    public float FrontalArea = 0.6f; // in sq m
    public float AirDensity = 1.225f; // in kg/m^3
    public float DragCoefficient = 0.9f;
    public float WheelDiameter = 2.096f / (float)Math.PI;// in m

    #endregion

    #region Operation Codes

    private readonly byte UNLOCK = 0x20;
    private readonly byte SET_RESISTANCE_MODE = 0x40;
    private readonly byte SET_STANDARD_MODE = 0x41;
    private readonly byte SET_ERG_MODE = 0x42;
    private readonly byte SET_SIM_MODE = 0x43;
    private readonly byte SET_SIM_CRR = 0x44;
    private readonly byte SET_WIND_RESISTANCE = 0x45;
    private readonly byte SET_SIM_GRADE = 0x46;
    private readonly byte SET_SIM_WIND_SPEED = 0x47;
    private readonly byte SET_WHEEL_CIRCUMFERENCE = 0x48;

    #endregion
    
    private bool _isCoroutineExecuting = false;

    #region Event Methods

    // Update is called once per frame
    void Update()
    {
        var newLoad = (int)slider.value;
        if (newLoad != oldLoad)
        {
            StartCoroutine(SetErgMode(2.0f, newLoad));
            oldLoad = newLoad;
        }
        
        while (BleAPI.PollData(out var res, false))
        {
            var hol = res.characteristicUuid.Substring(5, 4).ToUpper();
            //print("Hol_Bike: " + hol);
            switch (hol)
            {
                case CyclePowerMeasurementID:
                    ParseCyclePowerMeasurementData(res.buf);
                    break;
                case CyclePowerWahooExtensionID:                    
                    ParseCyclePowerWahooExData(res.buf);
                    subscribeText.text = "" + _opCodeRaw + ": " + _opResult;
                    break;
            }
        }
    }
    #endregion

    #region Public Methods

    public void Initialize(string deviceID, string serviceID, List<string> characteristicsList)
    {
        print("Initialising...");
        _selectedDevice = deviceID;
        _selectedService = serviceID;
        _foundCharacteristics = characteristicsList;
        SubscribeToCharacteristics();
    }
    
    public void SetLoad(Slider slider)
    {
        var load = (int)slider.value;
        StartCoroutine(SetErgMode(2.0f, load));
    }
    
    public void SetResistanceMode(Slider slider)
    {
        _isCoroutineExecuting = false;
        if (_loadType == LoadTypes.WahooKickr)
        {
            var load = slider.value;
            var resist = (int)((1.0 - load) * 16383);
            
            byte[] opcode = { SET_RESISTANCE_MODE, (byte)resist, (byte)(resist >> 8) };
            WriteByteArray(opcode,_wahooExID);
        }
    }
    
    public void SetStandardMode(Slider slider)
    {
        _isCoroutineExecuting = false;
        if (_loadType == LoadTypes.WahooKickr)
        {
            var level = slider.value;
            
            byte[] opcode = { SET_STANDARD_MODE, (byte)level };
            WriteByteArray(opcode,_wahooExID);
        }
    }
    
    IEnumerator SetErgMode(float time, int res)
    {
        if (_isCoroutineExecuting)
        {
            _isCoroutineExecuting = false;
            yield return new WaitForSeconds(time);
        }
        
        _isCoroutineExecuting = true;
        if (_loadType == LoadTypes.WahooKickr)
        {
            byte[] opcode = { SET_ERG_MODE, (byte)res, (byte)(res >> 8)};
            WriteByteArray(opcode,_wahooExID);
        }
    }
    
    private void SetSimMode(double weight, double rollingResistance, double windResistance)
    {
        _isCoroutineExecuting = false;
        if (_loadType == LoadTypes.WahooKickr)
        {
            var valueWeight = (int)(Math.Max(0, Math.Min(655.35, weight)) * 100.0);
            var valueCrr = (int)(Math.Max(0, Math.Min(65.535, rollingResistance)) * 10000.0);
            var valueCwr = (int)(Math.Max(0, Math.Min(65.535, windResistance)) * 1000.0);
            byte[] opcode =
            {
                SET_SIM_MODE, (byte)valueWeight, (byte)(valueWeight >> 8), (byte)valueCrr, (byte)(valueCrr >> 8),
                (byte)valueCwr, (byte)(valueCwr >> 8)
            };
            WriteByteArray(opcode,_wahooExID);
        }
    }
    
    public void SetSimCRR(double rollingResistance)
    {
        if (_loadType == LoadTypes.WahooKickr)
        {
            var valueCrr = (int)(Math.Max(0, Math.Min(65.535, rollingResistance)) * 10000.0);
            byte[] opcode = { SET_SIM_CRR, (byte)valueCrr, (byte)(valueCrr >> 8)};
            WriteByteArray(opcode,_wahooExID);
        }
    }
    
    public void SetSimWindResistance(double windResistance)
    {
        if (_loadType == LoadTypes.WahooKickr)
        {
            var valueCwr = (int)(Math.Max(0, Math.Min(65.535, windResistance)) * 10000.0);
            byte[] opcode = { SET_WIND_RESISTANCE, (byte)valueCwr, (byte)(valueCwr >> 8)};
            WriteByteArray(opcode,_wahooExID);
        }        
    }
    
    public void SetSimGrade(Slider slider)
    {
        var res = slider.value;
        SetSimMode(_weight,_rollingResistance,_windResistance);
        if (_loadType == LoadTypes.WahooKickr)
        {
            var grade = (int)((Math.Min(1, Math.Max(-1, res)) + 1.0) * 65535 / 2.0);
            byte[] opcode = { SET_SIM_GRADE, (byte)grade, (byte)(grade >> 8)};
            WriteByteArray(opcode,_wahooExID);
        }
        
    }
    
    private void SetWindSpeed(double res)  // In meters/second
    {
        if (_loadType == LoadTypes.WahooKickr)
        {
            var value = (int)((Math.Max(-32.767, Math.Min(32.767, res)) + 32.767) * 1000);
            byte[] opcode = { SET_SIM_WIND_SPEED, (byte)value, (byte)(value >> 8)};
            WriteByteArray(opcode,_wahooExID);
        }
    }
    
    public void SetWheelCircumference(double res) // in millimeters
    {
        if (_loadType == LoadTypes.WahooKickr)
        {
            var value = (int)(Math.Max(0, res));
            byte[] opcode = { SET_WHEEL_CIRCUMFERENCE, (byte)value, (byte)(value >> 8)};
            WriteByteArray(opcode,_wahooExID);
            _wheelSize = value;
        }
    }

    public void SetWeight(double weight)
    {
        SetSimMode(weight, _rollingResistance, _windResistance);
    }

    #endregion

    #region Private Methods

    private void SubscribeToCharacteristics()
    {
        print("Found characteristics: "+_foundCharacteristics);
        foreach (var characteristics in _foundCharacteristics)
        {
            var res = characteristics.Substring(5, 4).ToUpper();
            switch (res)
            {            
                case CyclePowerMeasurementID:                    
                    BleAPI.SubscribeCharacteristic(_selectedDevice, _selectedService, characteristics, false);
                    break;
                
                case CyclePowerWahooExtensionID:
                    BleAPI.SubscribeCharacteristic(_selectedDevice, _selectedService, characteristics, false);
                    _wahooExID = characteristics;
                    Debug.Log(_wahooExID);
                    _loadType = LoadTypes.WahooKickr;
                    InitializeWahooExtension();
                    break;
            }
        }
    }

    private void InitializeWahooExtension()
    {
        byte[] opcode = { UNLOCK, 0xee, 0xfc };
        WriteByteArray(opcode, _wahooExID);
        SetWheelCircumference(_wheelSize);
        SetSimMode(_weight, _rollingResistance, _windResistance);
        SetWindSpeed(_windSpeed);
        SetErgMode(2.0f,_load);
    }

    #endregion

    #region Reading and Writing Data Methods

    private void ParseCyclePowerMeasurementData(byte[] data)
    {
        //string dataString = data.Aggregate(new StringBuilder(), (sb, b) => sb.AppendFormat("{0:x2} ", b), sb => sb.AppendFormat("({0})", data.Length).ToString());
        //print(dataString);

        var valueOffset = 0;
        var flags = BitConverter.ToInt16(data, valueOffset);
        valueOffset += 2;
        _power = BitConverter.ToInt16(data, valueOffset);
        power.text = "Power:" + _power;
        valueOffset += 2;

        //_speed = GetBikeSpeedInKmPerHour((float)_power, RiderWeight, BikeWeight, FrontalArea, AirDensity, DragCoefficient);
        //speed.text = "Speed: " + _speed;
        //_distance += _speed * Time.deltaTime;
        //distance.text = "Distance: " + _distance;        


        if ((flags & 0x01) != 0)
        {
            var unused = data[valueOffset];
            valueOffset += 1;
        }

        if ((flags & 0x04) != 0)
        {
            var unused = BitConverter.ToInt16(data, valueOffset);
            valueOffset += 2;
        }

        if ((flags & 0x10) != 0)
        {
            var cumulativeWheelRev = BitConverter.ToInt32(data, valueOffset);
            valueOffset += 4;
            var lastWheelEventTime = BitConverter.ToInt16(data, valueOffset);
            valueOffset += 2;

            if (!_prevWheelStaleness)
            {
                var time = lastWheelEventTime - _prevWheelTime;
                //print("time: " + time);
                var revs = cumulativeWheelRev - _prevWheelRevs;
                //print("revs: " + revs);
                //Debug.Log("{" + cumulativeWheelRev + " , " + lastWheelEventTime + "} {" + time + "," + revs + "}");
                if (time != 0) //received a bike update
                {
                    _rpm = (revs * _wheelSize) / (double)time * 60; //
                    _speed = (_wheelSize * _rpm * 60) / 1000000; //convert to km/h

                }
                //else //not received bike update
                //{
                //    //slow down the bike
                //    _speed = _speed - 1;
                //    if (_speed < 0)
                //        _speed = 0;
                //}

                //_distance += (_speed * (double)time) / 1000000;
                _distance += _wheelSize * (double)revs / 1000000; //distance in kms
            }
            else
            {
                _prevWheelStaleness = false;
            }

            _prevWheelRevs = cumulativeWheelRev;
            _prevWheelTime = lastWheelEventTime;

            //_speed = (_rpm * _wheelSize * 60) * 0.00000062;

            rpm.text = "RPM: " + _rpm;
            speed.text = "Speed: " + _speed;
            distance.text = "Distance: " + _distance;
        }

        if ((flags & 0x20) != 0)
        {
            var cumulativeCrankRev = BitConverter.ToInt16(data, valueOffset);
            valueOffset += 2;
            var lastCrankEventTime = BitConverter.ToInt16(data, valueOffset);
            valueOffset += 2;

            if (lastCrankEventTime != _prevCrankTime)
            {
                if (_prevCrankStaleness >= 0)
                {
                    var time = lastCrankEventTime + (lastCrankEventTime < _prevCrankTime ? 0x10000 : 0) - _prevCrankTime;
                    var revs = cumulativeCrankRev + (cumulativeCrankRev < _prevCrankRevs ? 0x10000 : 0) - _prevCrankRevs;
                    _cadence = (revs * 1024) / (double)time * 60;
                }
            }
            else if (_prevCrankStaleness is < 0 or >= 2)
            {
                _cadence = 0;
            }

            if (lastCrankEventTime != _prevCrankTime)
            {
                _prevCrankStaleness = 0;
            }
            else if (_prevCrankStaleness < 2)
            {
                _prevCrankStaleness += 1;
            }

            _prevCrankRevs = cumulativeCrankRev;
            _prevCrankTime = lastCrankEventTime;

            cadence.text = "Cadence: " + _cadence;
        }
    }

    //public float GetBikeSpeedInKmPerHour(float Power, float RPM)
    //{
    //    float wheelCircumference = (float)(Math.PI * WheelDiameter);
    //    float angularVelocity = RPM * 2 * (float)Math.PI / 60;
    //    float torque = Power / angularVelocity;
    //    float force = torque / (WheelDiameter / 2);
    //    float acceleration = force / (RiderWeight + BikeWeight);
    //    float time = wheelCircumference / (RPM * 60);
    //    float velocity = acceleration * time;
    //    float speed = velocity * 3.6f; // convert to km/h
    //    return speed;
    //}

    double GetBikeSpeedInKmPerHour(double powerInWatts, double riderWeightInKg, double bikeWeightInKg, double frontalAreaInSqM, double airDensityInKgPerCuM, double dragCoefficient)
    {
        const double metersPerSecondToKmPerHour = 3.6;

        double rollingResistanceCoefficient = 0.005; // assuming typical road bike tires
        double rollingResistanceForce = riderWeightInKg * bikeWeightInKg * 9.81 * rollingResistanceCoefficient;
        double gradeResistanceForce = riderWeightInKg * bikeWeightInKg * 9.81 * Math.Sin(Math.Atan(0.01)); // assuming a 1% gradient
        double dragForce = 0.5 * airDensityInKgPerCuM * Math.Pow(GetBikeSpeedInMetersPerSecond(powerInWatts, frontalAreaInSqM, dragCoefficient), 2) * frontalAreaInSqM * dragCoefficient;
        double totalForce = rollingResistanceForce + gradeResistanceForce + dragForce;
        double speedInMetersPerSecond = Math.Sqrt(powerInWatts / totalForce);
        double speedInKmPerHour = speedInMetersPerSecond * metersPerSecondToKmPerHour;

        return speedInKmPerHour;
    }

    double GetBikeSpeedInMetersPerSecond(double powerInWatts, double frontalAreaInSqM, double dragCoefficient)
    {
        const double airDensityInKgPerCuM = 1.225; // assuming standard air density at sea level

        return Math.Pow((2 * powerInWatts) / (airDensityInKgPerCuM * frontalAreaInSqM * dragCoefficient), 0.5);
    }


    private void ParseCyclePowerWahooExData(byte[] data)
    {
        if (data.Length > 1)
        {
            _opResult = data[0];
            _opCodeRaw = data[1];
        }
    }
    private void WriteByteArray(IReadOnlyList<byte> input, string uuid)
    {
        var data = new BleAPI.BLEData
        {
            buf = new byte[512],
            size = (short)input.Count,
            deviceId = _selectedDevice,
            serviceUuid = _selectedService,
            characteristicUuid = uuid
        };
        for (var i = 0; i < input.Count; i++)
        {
            data.buf[i] = input[i];
        }
        BleAPI.SendData(in data, false);
    }

    #endregion
}
