using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Lip;
using UnityEngine.Video;
using System.IO;
using System.Globalization;
using System.Linq;
using System;

public class CalibrationDataLoader : MonoBehaviour
{
    public string fileNameContainsCalibration = "_Calibration_Data";
    public string csvPath = "C:/Users/hcila/Documents/EMIL/Exergaming_Calibration_Data/"; // Path to the CSV file
    private CalibrationData cd;
    private bool dataReady = false;
    public StudyManager sm;

    public delegate void CalibrationDataReady(CalibrationData calData);
    public static event CalibrationDataReady OnCalibrationDataReady;

    // Start is called before the first frame update
    void Start()
    {
        if (!sm.participantID.Equals(""))
        {
            cd = LoadCalibrationFile();
            print("CalibrationFile: ");
            print(cd.participant_age);
            print(cd.participant_HR_MAX);
            print(cd.participant_HR_RESERVE);
            dataReady = true;
        }
        else
        {
            print("Please enter a participant ID in the Studymanager");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (dataReady)
        {
            dataReady = false;
            OnCalibrationDataReady(cd);
        }
    }

    private CalibrationData LoadCalibrationFile() //ADD LIP_WEIGHTING CAL DATA
    {
        string[] csvFiles = Directory.GetFiles(csvPath, sm.participantID + fileNameContainsCalibration + ".csv");

        if (csvFiles.Length == 0)
        {
            Debug.LogError("No Calibration CSV file found with the specified name");
            return null;
        }

        // Use the first found CSV file
        string file = csvFiles[0];

        // Read the CSV file
        string[] csvLines = File.ReadAllLines(file);
        //string[] headers = csvLines[0].Split(',');

        CalibrationData data = new CalibrationData();
        bool first = true;

        List<int> temp_HR = new List<int>();
        List<float> temp_GSR_conductance = new List<float>();
        List<float> temp_GSR_resistance = new List<float>();
        List<float> temp_0_Jaw_Forward_Baseline = new List<float>();
        List<float> temp_1_Jaw_Right_Baseline = new List<float>();
        List<float> temp_2_Jaw_Left_Baseline = new List<float>();
        List<float> temp_3_Jaw_Open_Baseline = new List<float>();
        List<float> temp_4_Mouth_Ape_Shape_Baseline = new List<float>();
        List<float> temp_5_Mouth_O_Shape_Baseline = new List<float>();
        List<float> temp_6_Mouth_Pout_Baseline = new List<float>();
        List<float> temp_7_Mouth_Lower_Right_Baseline = new List<float>();
        List<float> temp_8_Mouth_Lower_Left_Baseline = new List<float>();
        List<float> temp_9_Mouth_Smile_Right_Baseline = new List<float>();
        List<float> temp_10_Mouth_Smile_Left_Baseline = new List<float>();
        List<float> temp_11_Mouth_Sad_Right_Baseline = new List<float>();
        List<float> temp_12_Mouth_Sad_Left_Baseline = new List<float>();
        List<float> temp_13_Cheek_Puff_Right_Baseline = new List<float>();
        List<float> temp_14_Cheek_Puff_Left_Baseline = new List<float>();
        List<float> temp_15_Mouth_Lower_Inside_Baseline = new List<float>();
        List<float> temp_16_Mouth_Upper_Inside_Baseline = new List<float>();
        List<float> temp_17_Mouth_Lower_Overlay_Baseline = new List<float>();
        List<float> temp_18_Mouth_Upper_Overlay_Baseline = new List<float>();
        List<float> temp_19_Cheek_Suck_Baseline = new List<float>();
        List<float> temp_20_Mouth_LowerRight_Down_Baseline = new List<float>();
        List<float> temp_21_Mouth_LowerLeft_Down_Baseline = new List<float>();
        List<float> temp_22_Mouth_UpperRight_Up_Baseline = new List<float>();
        List<float> temp_23_Mouth_UpperLeft_Up_Baseline = new List<float>();
        List<float> temp_24_Mouth_Philtrum_Right_Baseline = new List<float>();
        List<float> temp_25_Mouth_Philtrum_Left_Baseline = new List<float>();
        List<float> temp_26_Max_Baseline = new List<float>();

        for (int i = 1; i < csvLines.Length; i++) // Start from 1 to skip header
        {
            string[] values = csvLines[i].Split(',');

            if (first)
            {
                data.participant_id = int.Parse(values[0]);
                data.participant_age = int.Parse(values[1]);
                data.left_eye_dilation_calibration = new Dictionary<int, float>();
                data.right_eye_dilation_calibration = new Dictionary<int, float>();
                first = false;
            }

            temp_HR.Add(int.Parse(values[6]));
            temp_GSR_conductance.Add(float.Parse(values[7]));
            temp_GSR_resistance.Add(float.Parse(values[8]));

            temp_0_Jaw_Forward_Baseline.Add(float.Parse(values[9]));
            temp_1_Jaw_Right_Baseline.Add(float.Parse(values[10]));
            temp_2_Jaw_Left_Baseline.Add(float.Parse(values[11]));
            temp_3_Jaw_Open_Baseline.Add(float.Parse(values[12]));
            temp_4_Mouth_Ape_Shape_Baseline.Add(float.Parse(values[13]));
            temp_5_Mouth_O_Shape_Baseline.Add(float.Parse(values[14]));
            temp_6_Mouth_Pout_Baseline.Add(float.Parse(values[15]));
            temp_7_Mouth_Lower_Right_Baseline.Add(float.Parse(values[16]));
            temp_8_Mouth_Lower_Left_Baseline.Add(float.Parse(values[17]));
            temp_9_Mouth_Smile_Right_Baseline.Add(float.Parse(values[18]));
            temp_10_Mouth_Smile_Left_Baseline.Add(float.Parse(values[19]));
            temp_11_Mouth_Sad_Right_Baseline.Add(float.Parse(values[20]));
            temp_12_Mouth_Sad_Left_Baseline.Add(float.Parse(values[21]));
            temp_13_Cheek_Puff_Right_Baseline.Add(float.Parse(values[22]));
            temp_14_Cheek_Puff_Left_Baseline.Add(float.Parse(values[23]));
            temp_15_Mouth_Lower_Inside_Baseline.Add(float.Parse(values[24]));
            temp_16_Mouth_Upper_Inside_Baseline.Add(float.Parse(values[25]));
            temp_17_Mouth_Lower_Overlay_Baseline.Add(float.Parse(values[26]));
            temp_18_Mouth_Upper_Overlay_Baseline.Add(float.Parse(values[27]));
            temp_19_Cheek_Suck_Baseline.Add(float.Parse(values[28]));
            temp_20_Mouth_LowerRight_Down_Baseline.Add(float.Parse(values[29]));
            temp_21_Mouth_LowerLeft_Down_Baseline.Add(float.Parse(values[30]));
            temp_22_Mouth_UpperRight_Up_Baseline.Add(float.Parse(values[31]));
            temp_23_Mouth_UpperLeft_Up_Baseline.Add(float.Parse(values[32]));
            temp_24_Mouth_Philtrum_Right_Baseline.Add(float.Parse(values[33]));
            temp_25_Mouth_Philtrum_Left_Baseline.Add(float.Parse(values[34]));
            temp_26_Max_Baseline.Add(float.Parse(values[35]));

            data.left_eye_dilation_calibration.Add(int.Parse(values[3]), float.Parse(values[4]));
            data.right_eye_dilation_calibration.Add(int.Parse(values[3]), float.Parse(values[5]));
        }

        data.participant_calibration_resting_HR = CalculateMedian(temp_HR);
        data.participant_calibration_GSR_conductance = CalculateMedian(temp_GSR_conductance);
        data.participant_calibration_GSR_resistance = CalculateMedian(temp_GSR_resistance);
        data.participant_HR_MAX = 220 - data.participant_age;
        data.participant_HR_RESERVE = data.participant_HR_MAX - (int)Math.Round(data.participant_calibration_resting_HR, 0);

        data.calibrated_lip_weightings = new Dictionary<LipShape, float>();
        data.calibrated_lip_weightings.Add(LipShape.Jaw_Forward, CalculateMedian(temp_0_Jaw_Forward_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Jaw_Right, CalculateMedian(temp_1_Jaw_Right_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Jaw_Left, CalculateMedian(temp_2_Jaw_Left_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Jaw_Open, CalculateMedian(temp_3_Jaw_Open_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Mouth_Ape_Shape, CalculateMedian(temp_4_Mouth_Ape_Shape_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Mouth_O_Shape, CalculateMedian(temp_5_Mouth_O_Shape_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Mouth_Pout, CalculateMedian(temp_6_Mouth_Pout_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Mouth_Lower_Right, CalculateMedian(temp_7_Mouth_Lower_Right_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Mouth_Lower_Left, CalculateMedian(temp_8_Mouth_Lower_Left_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Mouth_Smile_Right, CalculateMedian(temp_9_Mouth_Smile_Right_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Mouth_Smile_Left, CalculateMedian(temp_10_Mouth_Smile_Left_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Mouth_Sad_Right, CalculateMedian(temp_11_Mouth_Sad_Right_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Mouth_Sad_Left, CalculateMedian(temp_12_Mouth_Sad_Left_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Cheek_Puff_Right, CalculateMedian(temp_13_Cheek_Puff_Right_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Cheek_Puff_Left, CalculateMedian(temp_14_Cheek_Puff_Left_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Mouth_Lower_Inside, CalculateMedian(temp_15_Mouth_Lower_Inside_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Mouth_Upper_Inside, CalculateMedian(temp_16_Mouth_Upper_Inside_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Mouth_Lower_Overlay, CalculateMedian(temp_17_Mouth_Lower_Overlay_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Mouth_Upper_Overlay, CalculateMedian(temp_18_Mouth_Upper_Overlay_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Cheek_Suck, CalculateMedian(temp_19_Cheek_Suck_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Mouth_LowerRight_Down, CalculateMedian(temp_20_Mouth_LowerRight_Down_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Mouth_LowerLeft_Down, CalculateMedian(temp_21_Mouth_LowerLeft_Down_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Mouth_UpperRight_Up, CalculateMedian(temp_22_Mouth_UpperRight_Up_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Mouth_UpperLeft_Up, CalculateMedian(temp_23_Mouth_UpperLeft_Up_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Mouth_Philtrum_Right, CalculateMedian(temp_24_Mouth_Philtrum_Right_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Mouth_Philtrum_Left, CalculateMedian(temp_25_Mouth_Philtrum_Left_Baseline));
        data.calibrated_lip_weightings.Add(LipShape.Max, CalculateMedian(temp_26_Max_Baseline));

        return data;
    }

    private T CalculateMedian<T>(List<T> values)
    {
        if (values.Count == 0)
        {
            throw new InvalidOperationException("The input list is empty.");
        }

        // Sort the list in ascending order
        List<T> sortedValues = values.OrderBy(x => x).ToList();

        int middleIndex = values.Count / 2;

        if (values.Count % 2 == 0)
        {
            // List length is even
            dynamic value1 = sortedValues[middleIndex - 1];
            dynamic value2 = sortedValues[middleIndex];
            return (value1 + value2) / 2;
        }
        else
        {
            // List length is odd
            return sortedValues[middleIndex];
        }
    }

    //public CalibrationData GetCalibrationData() { return cd; }


}

public class CalibrationData
{
    public int participant_id { get; set; }
    public int participant_age { get; set; }
    public double participant_calibration_resting_HR { get; set; }
    public float participant_calibration_GSR_conductance { get; set; }
    public float participant_calibration_GSR_resistance { get; set; }
    public Dictionary<int, float> left_eye_dilation_calibration { get; set; }
    public Dictionary<int, float> right_eye_dilation_calibration { get; set; }
    public int participant_HR_MAX { get; set; }
    public int participant_HR_RESERVE { get; set; }

    public Dictionary<LipShape, float> calibrated_lip_weightings { get; set; }

    public KeyValuePair<int, float> FindClosestKeyValueLeft(float targetValue)
    {
        KeyValuePair<int, float> closestKeyValue = new KeyValuePair<int, float>(0, 0f);
        float smallestDifference = Mathf.Infinity;

        foreach (KeyValuePair<int, float> pair in left_eye_dilation_calibration)
        {
            int key = pair.Key;
            float value = pair.Value;
            float difference = Mathf.Abs(targetValue - key);

            if (difference < smallestDifference)
            {
                smallestDifference = difference;
                closestKeyValue = new KeyValuePair<int, float>(key, value);
            }
        }

        return closestKeyValue;
    }

    public KeyValuePair<int, float> FindClosestKeyValueRight(float targetValue)
    {
        KeyValuePair<int, float> closestKeyValue = new KeyValuePair<int, float>(0, 0f);
        float smallestDifference = Mathf.Infinity;

        foreach (KeyValuePair<int, float> pair in right_eye_dilation_calibration)
        {
            int key = pair.Key;
            float value = pair.Value;
            float difference = Mathf.Abs(targetValue - key);

            if (difference < smallestDifference)
            {
                smallestDifference = difference;
                closestKeyValue = new KeyValuePair<int, float>(key, value);
            }
        }

        return closestKeyValue;
    }
}
