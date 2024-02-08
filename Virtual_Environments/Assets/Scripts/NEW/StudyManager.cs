using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudyManager : MonoBehaviour
{
    /// <summary>
    /// TODO:
    ///  - Bike Speed Control
    /// </summary>

    public enum StudyPhase
    {
        Setup,   // getting the participant ready, connecting sensors, starting recordings.
        FirstWarmUp, // First neutral scene.
        Exposure,   // Emotion Scene.
        CoolDown,   // Neutral Scene while answering Emotion Questions.
        WarmUp,      // Neutral Scene while reaching desired heart rate.
        ExperimentEnd // After Final Cooldown
    }
    public enum ConditionOrder
    {
        NotSet,
        One, //Joy, Serenity, Stress, Sadness
        Two, //Serenity, Stress, Sadness, Joy
        Three, //Sadness, Joy, Serenity, Stress
        Four //Stress, Sadness, Joy, Serenity
    }
    public enum ShownScene
    {
        Neutral,
        Joy,
        Serenity,
        Stress,
        Sadness
    }
    public enum ExerciseIntensity
    {
        NotSet,
        Low,
        Medium,
        High
    }
       
    public string participantID;
    public ConditionOrder conditionOrder;
    public ExerciseIntensity exerciseIntensity;
    public bool recordData;

    public SceneManager sceneManager;
    public CoolDownManager coolDownManager;
    public WarmUpManager warmUpManager;
    public RawDataExport dataRecorder;

    public StudyPhase phase;
    public ShownScene shownScene;

    public float exposureSceneTime = 60.0f;
    public float emotionSceneTimer;

    public int ExerciseBout_HR_Range_Upper;
    public int ExerciseBout_HR_Range_Lower;
    public double Participant_HR_Rest;
    public int Participant_HR_Reserve;

    private bool calDataReady;
    private bool devicesConnected;
    private bool heartRateThresholdsSet;
    private bool warmUpTransitionTriggered;
    
    public int emotionSceneCount;
    private float warmUpTransitionTimer;

    private CalibrationData calibrationData;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        emotionSceneCount = 0;
        phase = StudyPhase.Setup;
        calDataReady = false;
        devicesConnected = false;
        heartRateThresholdsSet = false;
        warmUpTransitionTriggered = false;
        CalibrationDataLoader.OnCalibrationDataReady += CalibrationDataReceived;
        Device_Manager.OnDevicesConnected += DevicesReady;
    }

    // Update is called once per frame
    void Update()
    {
        //check calibration data loaded, check phys + bike connection. 
        if (!calDataReady /*|| !devicesConnected*/ || conditionOrder.Equals(ConditionOrder.NotSet)
          || exerciseIntensity.Equals(ExerciseIntensity.NotSet) || phase.Equals(StudyPhase.ExperimentEnd))
            return;

        if (!heartRateThresholdsSet)
        {
            SetHeartRateThresholds();
            heartRateThresholdsSet = true;
        }

        if (!sceneManager.configured) { sceneManager.ConfigureSceneOrder(conditionOrder); }

        // --- recording check
        if (!recordData && dataRecorder.recording)
            dataRecorder.StopRecording();

        if (!recordData)
            return;

        if (!dataRecorder.recording)
        {
            dataRecorder.BeginRecording();
            dataRecorder.GetComponent<AffectResponseExport>().WriteAffectResponseHeader();
        }
        // --- --- ---

        if (!phase.Equals(StudyPhase.Exposure))            
            CheckExperimenterInput();            
        else
            RunEmotionExposure();

        if(warmUpTransitionTriggered && warmUpTransitionTimer <= 0.0f)
            TransitionToEmotionScene();        
        else if (warmUpTransitionTriggered)        
            warmUpTransitionTimer -= Time.deltaTime;
    }

    private void RunEmotionExposure()
    {
        //check if we're 1 second from the end - then trigger fade in scenemanager

        if (emotionSceneTimer <= sceneManager.fadeEffectDuration && !sceneManager.screenFadeRunning)
        {
            emotionSceneTimer = sceneManager.fadeEffectDuration;
            sceneManager.triggerFadeEffect();
        }

        if(sceneManager.screenFadeRunning && emotionSceneTimer <= 0)
        {
            sceneManager.screenFadeRunning = false;
            emotionSceneTimer = 0;
            phase = StudyPhase.CoolDown;
            StartCoolDown(shownScene);
            sceneManager.TriggerNeutralScene();           
        }

        emotionSceneTimer -= Time.deltaTime;
    }

    private void CheckExperimenterInput()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;
        
        switch (phase)
        {
            case StudyPhase.Setup:
                {
                    // Change state to first warm up
                    // Trigger the heart rate goal visualiser - if heart rate achieved, visualiser disappears after X seconds
                    phase = StudyPhase.FirstWarmUp;
                    StartWarmUp();
                    break;
                }
            case StudyPhase.FirstWarmUp:
                {
                    EndWarmUp();
                    break;
                }
            case StudyPhase.CoolDown:
                {
                    // Finished Sampling participant
                    // Trigger the heart rate visualiser
                    if (!coolDownManager.coolDownState.Equals(CoolDownManager.CoolDownState.FinishedMeasurements))
                        return;

                    if(emotionSceneCount < 4)
                    {
                        StartWarmUp();
                        phase = StudyPhase.WarmUp;
                    }
                    else
                    {
                        phase = StudyPhase.ExperimentEnd;
                        print("Trial End!");
                        // Trial END ***********
                    }
                    coolDownManager.coolDownState = CoolDownManager.CoolDownState.NotCoolingDown;
                    break;
                }
            case StudyPhase.WarmUp:
                {
                    EndWarmUp();
                    break;
                }
            default: break;
        }
        
    }

    private void StartWarmUp() //Need a WarmUpManager
    {
        print("Warming up...");
        if (!phase.Equals(StudyPhase.FirstWarmUp))              
            coolDownManager.thankyouMessageObj.GetComponent<FadeCanvas>().FadeOutSetUnactive(); //remove thank you        

        warmUpManager.TriggerWarmUp(ExerciseBout_HR_Range_Upper, ExerciseBout_HR_Range_Lower, Participant_HR_Reserve, Participant_HR_Rest);
        sceneManager.TriggerEmotionSceneSkybox(emotionSceneCount);
    }

    private void StartCoolDown(ShownScene scene) //Need some CoolDownManager - pass emotionSceneCount
    {
        print("cooling down...");
        warmUpManager.HR_Status = WarmUpManager.WarmupHR_Status.WarmupHR_NotSet;
        coolDownManager.TriggerCoolDown(scene, exerciseIntensity, emotionSceneCount);
    }

    private void EndWarmUp()
    {
        if(!warmUpManager.HR_Achieved && !warmUpManager.OverideTargetHR)
        {
            print("Please ensure participant reaches Target HR or Overide in the Warmup Manager");
            return;
        }            

        sceneManager.triggerFadeEffect();
        warmUpTransitionTriggered = true;
        warmUpTransitionTimer = sceneManager.fadeEffectDuration;
        warmUpManager.EndWarmUp();
    }

    private void TransitionToEmotionScene()
    {
        phase = StudyPhase.Exposure;
        sceneManager.TriggerEmotionScene(emotionSceneCount);
        emotionSceneTimer = exposureSceneTime;
        emotionSceneCount++;
        warmUpTransitionTimer = sceneManager.fadeEffectDuration;
        warmUpTransitionTriggered = false;
        sceneManager.screenFadeRunning = false;
    }

    private void CalibrationDataReceived(CalibrationData cd)
    {        
        calibrationData = cd;
        Participant_HR_Rest = calibrationData.participant_calibration_resting_HR;
        Participant_HR_Reserve = calibrationData.participant_HR_RESERVE;
        calDataReady = true;
    }

    private void SetHeartRateThresholds()
    {
        // calculate heartrate thresholds
        if (exerciseIntensity.Equals(ExerciseIntensity.Low)) //30% - 45%
        {
            ExerciseBout_HR_Range_Lower = Mathf.RoundToInt((calibrationData.participant_HR_RESERVE * 0.3f) + (int)calibrationData.participant_calibration_resting_HR);
            ExerciseBout_HR_Range_Upper = Mathf.RoundToInt((calibrationData.participant_HR_RESERVE * 0.45f) + (int)calibrationData.participant_calibration_resting_HR);            
        }
        else if (exerciseIntensity.Equals(ExerciseIntensity.Medium)) //50% - 65%
        {
            ExerciseBout_HR_Range_Lower = Mathf.RoundToInt((calibrationData.participant_HR_RESERVE * 0.5f) + (int)calibrationData.participant_calibration_resting_HR);
            ExerciseBout_HR_Range_Upper = Mathf.RoundToInt((calibrationData.participant_HR_RESERVE * 0.65f) + (int)calibrationData.participant_calibration_resting_HR);
        }
        else // High 70% - 85%
        {
            ExerciseBout_HR_Range_Lower = Mathf.RoundToInt((calibrationData.participant_HR_RESERVE * 0.7f) + (int)calibrationData.participant_calibration_resting_HR);
            ExerciseBout_HR_Range_Upper = Mathf.RoundToInt((calibrationData.participant_HR_RESERVE * 0.85f) + (int)calibrationData.participant_calibration_resting_HR);
        }
    }

    private void DevicesReady() { devicesConnected = true; }

    public bool DevicesConnected() { return devicesConnected; }

    public CalibrationData GetCalibrationData() { return calibrationData; }
    
    public GameObject GetNextScene() { return sceneManager.GetNextScene(emotionSceneCount); }

}
