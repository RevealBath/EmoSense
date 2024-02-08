using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// Exports Heart Rate, Skin Conductance, and Eye Data to a new .csv file
public class RawDataExport : MonoBehaviour
{
    // To access data
    public EyeTrackerExport eyeDataObject;
    public SkinConductanceService skinConductanceObject;
    public HeartRateService heartRateObject;
    public LipTrackingExport lipDataObject;
    public BikeControlService bikeControlObject;
    public GazePixelAnalyser gazePixelAnalyser;

    public StudyManager studyManager;
    public SceneManager sceneManager;
    public CoolDownManager coolDownManager;
    public WarmUpManager warmUpManager;
    public CollectingCoins coinManager;

    private GameObject cameraObj;
    private HeartRateData hr_data;
    private SkinConductanceData sc_data;
    private UnityEyeData eye_data;
    private UnityLipData lip_data;
    private UnityBikeData bike_data;
    private GazePixelData gaze_PixelData;

    private CalibrationData cd;

    // To create and write to .csv
    private StreamWriter writeFile;

    private string filename;

    private string csv_header_meta = "Unity_Frame,Unity_Timestamp,Participant_ID,Participant_Age,Participant_Calibration_Resting_HR,Participant_HR_MAX,HR_RESERVE," +
                                     "Participant_Calibration_GSR_Conductance,Participant_Calibration_GSR_Resistance," +
                                     "Condition_Order,Exercise_Intensity,Exercise_Target_HR_Lower,Exercise_Target_HR_Upper,";                  

    private string csv_header_study_status = "Study_Phase,Shown_Scene,Exposure_Scene_Timer,Cool_Down_State,Warmup_HR_State,CoinGame_Collision,CoinGame_CoinsCollected,";

    private string csv_header_data= "Headset_Pos.x,Headset_Pos.y,Headset_Pos.z,Headset_Rot_Euler.x,Headset_Rot_Euler.y,Headset_Rot_Euler.z,Headset_Rot_Quat.w," +
                                    "Headset_Rot_Quat.x,Headset_Rot_Quat.y,Headset_Rot_Quat.z," +        
                                    "Tobii_Gaze_Pos_Local.x,Tobii_Gaze_Pos_Local.y,Tobii_Gaze_Pos_Local.z,Tobii_Gaze_Dir_Local.x,Tobii_Gaze_Dir_Local.y,Tobii_Gaze_Dir_Local.z," +
                                    "Tobii_Gaze_Pos_World.x,Tobii_Gaze_Pos_World.y,Tobii_Gaze_Pos_World.z,Tobii_Gaze_Dir_World.x,Tobii_Gaze_Dir_World.y,Tobii_Gaze_Dir_World.z," +
                                    "sRanipal_Gaze_Pos_Local.x,sRanipal_Gaze_Pos_Local.y,sRanipal_Gaze_Pos_Local.z,sRanipal_Gaze_Dir_Local.x,sRanipal_Gaze_Dir_Local.y,sRanipal_Gaze_Dir_Local.z," +
                                    "Pupil_Dilation_Left,Pupil_Dilation_Right,Eye_Closed_Left,Eye_Closed_Right,current_blinkDuration,current_InterBlinkInterval," +
                                    "Shimmer_D36A_GSR_Skin_Conductance_uS,Shimmer_D36A_GSR_Skin_Resistance_kOhms," +
                                    "Polar_HearRate_BPM,Polar_HeartRate_RR_Interval,Percentage_of_HR_MAX,Percentage_of_HR_RESERVE," +
                                    "Vive_Lip_Tracker_Frame,Vive_Lip_Tracker_Time,0_Jaw_Forward,1_Jaw_Right,2_Jaw_Left,3_Jaw_Open,4_Mouth_Ape_Shape,5_Mouth_O_Shape,6_Mouth_Pout,7_Mouth_Lower_Right," +
                                    "8_Mouth_Lower_Left,9_Mouth_Smile_Right,10_Mouth_Smile_Left,11_Mouth_Sad_Right,12_Mouth_Sad_Left,13_Cheek_Puff_Right,14_Cheek_Puff_Left,15_Mouth_Lower_Inside," +
                                    "16_Mouth_Upper_Inside,17_Mouth_Lower_Overlay,18_Mouth_Upper_Overlay,19_Cheek_Suck,20_Mouth_LowerRight_Down,21_Mouth_LowerLeft_Down,22_Mouth_UpperRight_Up," +
                                    "23_Mouth_UpperLeft_Up,24_Mouth_Philtrum_Right,25_Mouth_Philtrum_Left,26_Max," +
                                    "Bike_Speed_KmH,Bike_Cadence_RPM,Bike_Power_Watt," +
                                    "Foveal_Gray_Scale_Value,Foveal_Closest_Calibration_Gray_Scale,Foveal_Participant_Calibration_Pupil_Dilation_Left,Foveal_Participant_Calibration_Pupil_Dilation_Right,Foveal_Corrected_Dilation_Left,Foveal_Corrected_Dilation_Right," +
                                    "Parafoveal_Gray_Scale_Value,Parafoveal_Closest_Calibration_Gray_Scale,Paraoveal_Participant_Calibration_Pupil_Dilation_Left,Paraoveal_Participant_Calibration_Pupil_Dilation_Right,Parafoveal_Corrected_Dilation_Left,Parafoveal_Corrected_Dilation_Right," +
                                    "Headpoint_Gray_Scale_Value,Headpoint_Closest_Calibration_Gray_Scale,Headpoint_Participant_Calibration_Pupil_Dilation_Left,Headpoint_Participant_Calibration_Pupil_Dilation_Right,Headpoint_Corrected_Dilation_Left,Headpoint_Corrected_Dilation_Right," +
                                    "Image_Gray_Scale_Value,Image_Closest_Calibration_Gray_Scale,Image_Participant_Calibration_Pupil_Dilation_Left,Image_Participant_Calibration_Pupil_Dilation_Right,Image_Corrected_Dilation_Left,Image_Corrected_Dilation_Right";


    // need to add: "Shimmer_D36A_Timestamp_FormattedUnix,Polar_Timestamp"
    // Percentage Heart Rate Reserve and Max

    // private bool eyesCalibrated;

    public bool recording;
    bool first;
    
    void Start()
    {        
        //Application.targetFrameRate = 90;
        //QualitySettings.vSyncCount = 1;

        recording = false;
        first = true;

        //*** Assign objects to variables.
        //heartRateObject = GameObject.Find("Heart_Rate_Service").GetComponent<HeartRateService>();
        //skinConductanceObject = GameObject.Find("Shimmer_Service").GetComponent<SkinConductanceService>();
        //eyeDataObject = GameObject.Find("Eye_Tracking_Service").GetComponent<EyeTrackerExport>();
        //lipDataObject = GameObject.Find("Lip_Tracking_Service").GetComponent<LipTrackingExport>();

        hr_data = new HeartRateData();
        sc_data = new SkinConductanceData();
        eye_data = new UnityEyeData();
        lip_data = new UnityLipData();
        bike_data = new UnityBikeData();
        gaze_PixelData = new GazePixelData();
        cameraObj = Camera.main.gameObject;
    }

    void Update()
    {
        if (!recording)
            return;

        if (studyManager.participantID == "" /*|| !studyManager.DevicesConnected()*/)
        {
            Debug.Log("Please check data recording variables in the inspector and the physiological sensor connections");
            recording = false;
            return;
        }

        if (first) //*** Create .csv file with today's date and the current time + CSV Header
        {
            print("Creating File...");
            first = false;
            cd = studyManager.GetCalibrationData();
            System.DateTime today = System.DateTime.Now;
            filename = Application.dataPath + "/CSV_Data/" + studyManager.participantID + "_" + studyManager.conditionOrder + "_" + studyManager.exerciseIntensity + "_RAW_physiological_data_"
                + today.ToString("yyyy-MM-dd_HH-mm-ss", System.Globalization.CultureInfo.InvariantCulture) + ".csv";
            writeFile = new StreamWriter(filename, false);
            //writeFile.WriteLine(csv_header_meta + csv_header_study_status + csv_header_data);
        }

        hr_data = heartRateObject.getLatestHeartRateData();
        sc_data = skinConductanceObject.getLatestShimmerData();
        eye_data = eyeDataObject.getLatestEyeData();
        //lip_data = lipDataObject.getLatestLipData();
        bike_data = bikeControlObject.GetLatestBikeData();
        gaze_PixelData = gazePixelAnalyser.GetLatestGazePixelData();

        //writeToFile();        
    }

    void writeToFile()
    {
        string lip_data_string = lip_data.getLipWeightingsAsCSVString();       

        KeyValuePair<int, float> foveal_Left_KeyValue = cd.FindClosestKeyValueLeft(gazePixelAnalyser.fovealGrayScale);
        KeyValuePair<int, float> foveal_Right_KeyValue = cd.FindClosestKeyValueRight(gazePixelAnalyser.fovealGrayScale);
        KeyValuePair<int, float> parafoveal_Left_KeyValue = cd.FindClosestKeyValueLeft(gazePixelAnalyser.parafovealGrayScale);
        KeyValuePair<int, float> parafoveal_Right_KeyValue = cd.FindClosestKeyValueRight(gazePixelAnalyser.parafovealGrayScale);
        KeyValuePair<int, float> headpoint_Left_KeyValue = cd.FindClosestKeyValueLeft(gazePixelAnalyser.headGrayScale);
        KeyValuePair<int, float> headpoint_Right_KeyValue = cd.FindClosestKeyValueRight(gazePixelAnalyser.headGrayScale);
        KeyValuePair<int, float> image_Left_KeyValue = cd.FindClosestKeyValueLeft(gazePixelAnalyser.imageGrayScale);
        KeyValuePair<int, float> image_Right_KeyValue = cd.FindClosestKeyValueRight(gazePixelAnalyser.imageGrayScale);

        int foveal_closest_calibration_gray_scale = foveal_Left_KeyValue.Key;
        float foveal_participant_calibration_pupil_dilation_left = foveal_Left_KeyValue.Value;
        float foveal_participant_calibration_pupil_dilation_right = foveal_Right_KeyValue.Value;
        float foveal_corrected_dilation_left = eye_data.pupilDilationLeft - foveal_participant_calibration_pupil_dilation_left;
        float foveal_corrected_dilation_right = eye_data.pupilDilationRight - foveal_participant_calibration_pupil_dilation_right;

        int parafoveal_closest_calibration_gray_scale = parafoveal_Left_KeyValue.Key;
        float parafoveal_participant_calibration_pupil_dilation_left = parafoveal_Right_KeyValue.Value;
        float parafoveal_participant_calibration_pupil_dilation_right = parafoveal_Right_KeyValue.Value;
        float parafoveal_corrected_dilation_left = eye_data.pupilDilationLeft - parafoveal_participant_calibration_pupil_dilation_left;
        float parafoveal_corrected_dilation_right = eye_data.pupilDilationRight - parafoveal_participant_calibration_pupil_dilation_right;

        int headpoint_closest_calibration_gray_scale = headpoint_Left_KeyValue.Key;
        float headpoint_participant_calibration_pupil_dilation_left = headpoint_Right_KeyValue.Value;
        float headpoint_participant_calibration_pupil_dilation_right = headpoint_Right_KeyValue.Value;
        float headpoint_corrected_dilation_left = eye_data.pupilDilationLeft - headpoint_participant_calibration_pupil_dilation_left;
        float headpoint_corrected_dilation_right = eye_data.pupilDilationRight - headpoint_participant_calibration_pupil_dilation_right;

        int image_closest_calibration_gray_scale = image_Left_KeyValue.Key;
        float image_participant_calibration_pupil_dilation_left = image_Right_KeyValue.Value;
        float image_participant_calibration_pupil_dilation_right = image_Right_KeyValue.Value;
        float image_corrected_dilation_left = eye_data.pupilDilationLeft - image_participant_calibration_pupil_dilation_left;
        float image_corrected_dilation_right = eye_data.pupilDilationRight - image_participant_calibration_pupil_dilation_right;

        // Write data to .csv file
        writeFile.WriteLine(Time.frameCount + "," + 
            System.DateTime.Now.ToString("dd-MM-yyyy - HH:mm:ss.ffffff", System.Globalization.CultureInfo.InvariantCulture) + "," +
            studyManager.participantID.ToString() + "," +
            cd.participant_age.ToString() + "," +
            cd.participant_calibration_resting_HR.ToString() + "," +
            cd.participant_HR_MAX.ToString() + "," +
            cd.participant_HR_RESERVE.ToString() + "," +
            cd.participant_calibration_GSR_conductance.ToString() + "," +
            cd.participant_calibration_GSR_resistance.ToString() + "," +
            studyManager.conditionOrder.ToString() + "," +
            studyManager.exerciseIntensity.ToString() + "," +
            studyManager.ExerciseBout_HR_Range_Lower.ToString() + "," +
            studyManager.ExerciseBout_HR_Range_Upper.ToString() + "," +

            studyManager.phase.ToString() + "," +
            studyManager.shownScene.ToString() + "," +
            studyManager.emotionSceneTimer.ToString() + "," +
            coolDownManager.coolDownState.ToString() + "," +
            warmUpManager.HR_Status.ToString() + "," +
            coinManager.coinCollisionStatus.ToString() + "," +
            coinManager.coins.ToString() + "," +
            
            cameraObj.transform.position.x.ToString() + "," +
            cameraObj.transform.position.y.ToString() + "," +
            cameraObj.transform.position.z.ToString() + "," +
            cameraObj.transform.rotation.eulerAngles.x.ToString() + "," +
            cameraObj.transform.rotation.eulerAngles.y.ToString() + "," +
            cameraObj.transform.rotation.eulerAngles.z.ToString() + "," +
            cameraObj.transform.rotation.w.ToString() + "," +
            cameraObj.transform.rotation.x.ToString() + "," +
            cameraObj.transform.rotation.y.ToString() + "," +
            cameraObj.transform.rotation.z.ToString() + "," +           

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

            eye_data.current_blinkDuration.ToString() + "," + 
            eye_data.current_interBlinkInterval.ToString() + "," + 

            sc_data.gsrConductance.ToString() + "," +
            sc_data.gsrResistance.ToString() + "," +

            hr_data.heartRateBPM.ToString() + "," +
            hr_data.heartRate_RR_Interval.ToString() + "," +
            ((hr_data.heartRateBPM/cd.participant_HR_MAX)*100).ToString() + "," +
            ((hr_data.heartRateBPM/cd.participant_HR_RESERVE)*100).ToString() + "," +

            lip_data.currLipData.frame.ToString() + "," +
            lip_data.currLipData.time.ToString() + "," +
            lip_data_string + "," +
            bike_data.speed_kmh.ToString() + "," +
            bike_data.rpm.ToString() + "," +
            bike_data.power.ToString() + "," +            
            gazePixelAnalyser.fovealGrayScale.ToString() + "," +
            foveal_closest_calibration_gray_scale + "," +
            foveal_participant_calibration_pupil_dilation_left + "," +
            foveal_participant_calibration_pupil_dilation_right + "," +
            foveal_corrected_dilation_left + "," +
            foveal_corrected_dilation_right + "," +
            gazePixelAnalyser.parafovealGrayScale.ToString() + "," +
            parafoveal_closest_calibration_gray_scale + "," +
            parafoveal_participant_calibration_pupil_dilation_left + "," +
            parafoveal_participant_calibration_pupil_dilation_right + "," +
            parafoveal_corrected_dilation_left + "," +
            parafoveal_corrected_dilation_right + "," +
            gazePixelAnalyser.headGrayScale.ToString() + "," +
            headpoint_closest_calibration_gray_scale + "," +
            headpoint_participant_calibration_pupil_dilation_left + "," +
            headpoint_participant_calibration_pupil_dilation_right + "," +
            headpoint_corrected_dilation_left + "," +
            headpoint_corrected_dilation_right + "," +
            gazePixelAnalyser.imageGrayScale.ToString() + "," +
            image_closest_calibration_gray_scale + "," +
            image_participant_calibration_pupil_dilation_left + "," +
            image_participant_calibration_pupil_dilation_right + "," +
            image_corrected_dilation_left + "," +
            image_corrected_dilation_right);
        writeFile.Flush();
    }

    // Close write file.
    void OnApplicationQuit()
    {
        if(writeFile != null)
            writeFile.Close();
    }

    public void BeginRecording()
    {
        //start
        print("Starting Recording ...");
        recording = true;
        first = true;
    }

    public void StopRecording()
    {
        //stop
        recording = false;
        first = true;

        if (writeFile != null)
            writeFile.Close();
    }
}
