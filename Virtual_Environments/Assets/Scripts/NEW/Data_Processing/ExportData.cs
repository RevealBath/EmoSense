using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// Exports Heart Rate, Skin Conductance, and Eye Data to a new .csv file
public class ExportData : MonoBehaviour
{
    // To access data
    private EyeTrackerExport eyeDataObject;
    private SkinConductanceService skinConductanceObject;
    private HeartRateService heartRateObject;
    private LipTrackingExport lipDataObject;
    private BikeControlService bikeControlObject;

    private HeartRateData hr_data;
    private SkinConductanceData sc_data;
    private UnityEyeData eye_data;
    private UnityLipData lip_data;
    private UnityBikeData bike_data;

    // To create and write to .csv
    private StreamWriter writeFile;

    private string filename;

    private string csv_header = "Unity_Frame,Unity_Timestamp,Participant_id," +
                        "tobiiGazePosLocal.x,tobiiGazePosLocal.y,tobiiGazePosLocal.z,tobiiGazeDirLocal.x,tobiiGazeDirLocal.y,tobiiGazeDirLocal.z," +
                        "tobiiGazePosWorld.x,tobiiGazePosWorld.y,tobiiGazePosWorld.z,tobiiGazeDirWorld.x,tobiiGazeDirWorld.y,tobiiGazeDirWorld.z," +
                        "sRanipalGazePosLocal.x,sRanipalGazePosLocal.y,sRanipalGazePosLocal.z,sRanipalGazeDirLocal.x,sRanipalGazeDirLocal.y,sRanipalGazeDirLocal.z," +
                        "pupil_dilation_left,pupil_dilation_right,eye_closed_left,eye_closed_right," +
                        "Shimmer_D36A_GSR_Skin_Conductance_uS,Shimmer_D36A_GSR_Skin_Resistance_kOhms," +
                        "Polar_HearRateBPM," +
                        "Vive_Lip_Tracker_Frame,Vive_Lip_Tracker_Time,0_Jaw_Forward,1_Jaw_Right,2_Jaw_Left,3_Jaw_Open,4_Mouth_Ape_Shape,5_Mouth_O_Shape,6_Mouth_Pout,7_Mouth_Lower_Right," +
                        "8_Mouth_Lower_Left,9_Mouth_Smile_Right,10_Mouth_Smile_Left,11_Mouth_Sad_Right,12_Mouth_Sad_Left,13_Cheek_Puff_Right,14_Cheek_Puff_Left,15_Mouth_Lower_Inside," +
                        "16_Mouth_Upper_Inside,17_Mouth_Lower_Overlay,18_Mouth_Upper_Overlay,19_Cheek_Suck,20_Mouth_LowerRight_Down,21_Mouth_LowerLeft_Down,22_Mouth_UpperRight_Up," +
                        "23_Mouth_UpperLeft_Up,24_Mouth_Philtrum_Right,25_Mouth_Philtrum_Left,26_Max";

    //private string csv_header = "Unity_Frame,Unity_Timestamp,P_id,P_age,P_calibration_resting_HR,P_calibration_GSR_conductance,P_calibration_GSR_resistance,condition,game,session_number," +
    //               "tobiiGazePosLocal.x,tobiiGazePosLocal.y,tobiiGazePosLocal.z,tobiiGazeDirLocal.x,tobiiGazeDirLocal.y,tobiiGazeDirLocal.z," +
    //               "tobiiGazePosWorld.x,tobiiGazePosWorld.y,tobiiGazePosWorld.z,tobiiGazeDirWorld.x,tobiiGazeDirWorld.y,tobiiGazeDirWorld.z," +
    //               "sRanipalGazePosLocal.x,sRanipalGazePosLocal.y,sRanipalGazePosLocal.z,sRanipalGazeDirLocal.x,sRanipalGazeDirLocal.y,sRanipalGazeDirLocal.z," +
    //               "pupil_dilation_left,pupil_dilation_right,eye_closed_left,eye_closed_right," +
    //               "Shimmer_D36A_GSR_Skin_Conductance_uS,Shimmer_D36A_GSR_Skin_Resistance_kOhms," +
    //               "Polar_HearRateBPM,percentage_of_HR_MAX,percentage_of_HR_RESERVE," +
    //               "Vive_Lip_Tracker_Frame,Vive_Lip_Tracker_Time,0_Jaw_Forward,1_Jaw_Right,2_Jaw_Left,3_Jaw_Open,4_Mouth_Ape_Shape,5_Mouth_O_Shape,6_Mouth_Pout,7_Mouth_Lower_Right," +
    //               "8_Mouth_Lower_Left,9_Mouth_Smile_Right,10_Mouth_Smile_Left,11_Mouth_Sad_Right,12_Mouth_Sad_Left,13_Cheek_Puff_Right,14_Cheek_Puff_Left,15_Mouth_Lower_Inside," +
    //               "16_Mouth_Upper_Inside,17_Mouth_Lower_Overlay,18_Mouth_Upper_Overlay,19_Cheek_Suck,20_Mouth_LowerRight_Down,21_Mouth_LowerLeft_Down,22_Mouth_UpperRight_Up," +
    //               "23_Mouth_UpperLeft_Up,24_Mouth_Philtrum_Right,25_Mouth_Philtrum_Left,26_Max," +
    //               "foveal_gray_scale_value,foveal_closest_calibration_gray_scale,foveal_participant_calibration_pupil_dilation_left,foveal_participant_calibration_pupil_dilation_right,foveal_corrected_dilation_left,foveal_corrected_dilation_right," +
    //               "parafoveal_gray_scale_value,parafoveal_closest_calibration_gray_scale,parafoveal_participant_calibration_pupil_dilation_left,parafoveal_participant_calibration_pupil_dilation_right,parafoveal_corrected_dilation_left,parafoveal_corrected_dilation_right," +
    //               "headpoint_gray_scale_value,headpoint_closest_calibration_gray_scale,headpoint_participant_calibration_pupil_dilation_left,headpoint_participant_calibration_pupil_dilation_right,headpoint_corrected_dilation_left,headpoint_corrected_dilation_right," +
    //               "image_gray_scale_value,image_closest_calibration_gray_scale,image_participant_calibration_pupil_dilation_left,image_participant_calibration_pupil_dilation_right,image_corrected_dilation_left,image_corrected_dilation_right";

    // need to add: "Shimmer_D36A_Timestamp_FormattedUnix,Polar_Timestamp"
    // Percentage Heart Rate Reserve and Max

    private bool eyesCalibrated;

    public enum condition
    {
        Nan,
        Retro,
        ESM_Retro
    }

    public enum game
    {
        Nan,
        HLA,
        PC3,
        Skyrim,
        GotS
    }

    public string participantID;
    public condition condt;
    public game VR_game;
    public string sessionNumber;

    public bool recording;
    bool first;
    
    void Start()
    {        
        Application.targetFrameRate = 90;
        QualitySettings.vSyncCount = 1;

        recording = false;
        first = true;
        eyesCalibrated = false;

        //*** Assign objects to variables.
        heartRateObject = GameObject.Find("Heart_Rate_Service").GetComponent<HeartRateService>();
        skinConductanceObject = GameObject.Find("Shimmer_Service").GetComponent<SkinConductanceService>();
        eyeDataObject = GameObject.Find("Eye_Tracking_Service").GetComponent<EyeTrackerExport>();
        lipDataObject = GameObject.Find("Lip_Tracking_Service").GetComponent<LipTrackingExport>();

        hr_data = new HeartRateData();
        sc_data = new SkinConductanceData();
        eye_data = new UnityEyeData();
        lip_data = new UnityLipData();
    }

    void Update()
    {
        //print("FrameRate: " + (1.0f / Time.deltaTime));
        if (Input.GetKeyDown(KeyCode.Space) && eyesCalibrated)
        {
            if (recording)
            {
                writeFile.Close();
                first = true;
            }
            recording = !recording;
        }
        else if (Input.GetKeyDown(KeyCode.C) && !recording && !eyesCalibrated)
        {
            ViveSR.anipal.Eye.SRanipal_Eye_API.LaunchEyeCalibration(System.IntPtr.Zero);
            eyesCalibrated = true;
        }

        if (recording && eyesCalibrated)
        {          
            //validate the data recording variables have been entered. 
            if (participantID == "" || sessionNumber == "" || condt == condition.Nan || VR_game == game.Nan || !heartRateObject.isSubscribed || !skinConductanceObject.isStreaming)
            {
                Debug.Log("Please check data recording variables in the inspector and the physiological sensor connections");
                recording = false;
                return;
            }

            if (first) //*** Create .csv file with today's date and the current time + CSV Header
            {
                first = false;                
                System.DateTime today = System.DateTime.Now;
                filename = Application.dataPath + "/CSV_Data/" + participantID + "_" + condt + "_" + VR_game + "_physiological_data_"
                    + today.ToString("yyyy-MM-dd_HH-mm-ss", System.Globalization.CultureInfo.InvariantCulture) + ".csv"; //PID_Condition_game_phys_data.csv;
                writeFile = new StreamWriter(filename, false);
                writeFile.WriteLine(csv_header);
            }

            hr_data = heartRateObject.getLatestHeartRateData();
            sc_data = skinConductanceObject.getLatestShimmerData();
            eye_data = eyeDataObject.getLatestEyeData();
            lip_data = lipDataObject.getLatestLipData();

            writeToFile();
        }
    }

    void writeToFile()
    {
        string lip_data_string = lip_data.getLipWeightingsAsCSVString();
        // Write data to .csv file
        writeFile.WriteLine(Time.frameCount + "," + 
            System.DateTime.Now.ToString("dd-MM-yyyy - HH:mm:ss.ffffff", System.Globalization.CultureInfo.InvariantCulture) + "," +
            participantID.ToString() + "," +
            condt.ToString() + "," +
            VR_game.ToString() + "," +
            sessionNumber.ToString() + "," +
            eye_data.tobiiGazePosLocal.x.ToString() + "," +
            eye_data.tobiiGazePosLocal.y.ToString() + "," +
            eye_data.tobiiGazePosLocal.z.ToString() + "," +
            eye_data.tobiiGazeDirLocal.x.ToString() + "," +
            eye_data.tobiiGazeDirLocal.y.ToString() + "," +
            eye_data.tobiiGazeDirLocal.z.ToString() + "," +
            eye_data.tobiiGazePosWorld.x.ToString() + "," +
            eye_data.tobiiGazePosWorld.y.ToString() + "," +
            eye_data.tobiiGazePosWorld.z.ToString() + "," +
            eye_data.tobiiGazeDirWorld.x.ToString() + "," +
            eye_data.tobiiGazeDirWorld.y.ToString() + "," +
            eye_data.tobiiGazeDirWorld.z.ToString() + "," +
            eye_data.SranipalGazePosLocal.x.ToString() + "," +
            eye_data.SranipalGazePosLocal.y.ToString() + "," +
            eye_data.SranipalGazePosLocal.z.ToString() + "," +
            eye_data.SranipalGazeDirLocal.x.ToString() + "," +
            eye_data.SranipalGazeDirLocal.y.ToString() + "," +
            eye_data.SranipalGazeDirLocal.z.ToString() + "," +
            eye_data.pupilDilationLeft.ToString() + "," +
            eye_data.pupilDilationRight.ToString() + "," + 
            eye_data.eyeClosedLeft.ToString() + "," +
            eye_data.eyeClosedRight.ToString() + "," + 
            sc_data.gsrConductance.ToString() + "," +
            sc_data.gsrResistance.ToString() + "," +
            hr_data.heartRateBPM.ToString() + "," +
            lip_data.currLipData.frame.ToString() + "," +
            lip_data.currLipData.time.ToString() + "," +
            lip_data_string);
        writeFile.Flush();
    }

    // Close write file.
    void OnApplicationQuit()
    {
        if(writeFile != null)
            writeFile.Close();
    }
}
