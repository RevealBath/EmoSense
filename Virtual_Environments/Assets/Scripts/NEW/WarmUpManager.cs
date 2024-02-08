using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WarmUpManager : MonoBehaviour
{
    public enum WarmupHR_Status
    {
        WarmupHR_NotSet,
        WarmupHR_Below_Threshold,
        WarmupHR_Above_Threshold,
        WarmupHR_OK
    }
    public bool runHR_Warmup;

    public StudyManager studyManager;
    public GameObject HR_Canvas;
    public GameObject HR_Powerbar;
    public GameObject HR_Indicator;
    public GameObject HR_TextObj;
    public TextMeshProUGUI HR_Text;
    public HeartRateService HR_service;
    
    public WarmupHR_Status HR_Status;
    public int bpm;
    public float target_HR_Mid_Point;
    public float percentage_target_HR_Mid_Point;

    public bool HR_Achieved;
    public bool OverideTargetHR;   

    private HeartRateData hrData;
    private int target_HR_Upper;
    private int target_HR_Lower;
    private int participant_HR_Reserve;
    private int participant_HR_Rest;

    private Vector3 maxIndicatorPos;
    private Vector3 minIndicatorPos;
    private Vector3 targetUpperIndicatorPos;
    private Vector3 targetLowerIndicatorPos;

    private bool runningOK_HR_Timer;
    private float OK_HR_Timer_Threshold = 10.0f;
    private float OK_HR_Timer;
    private float indicatorPos;

    // Start is called before the first frame update
    void Start()
    {
        maxIndicatorPos = new Vector3(132,8,0);         // HR low pos - Y -133                                                   
        minIndicatorPos = new Vector3(132, -133, 0);    // HR high pos - Y 8
        targetUpperIndicatorPos = new Vector3(132, -51, 0); // HR Okay High pos - y -51                                                                
        targetLowerIndicatorPos = new Vector3(132, -73, 0); // HR Okay Low pos - y -73
        runHR_Warmup = false;
        runningOK_HR_Timer = false;
        OverideTargetHR = false;
        HR_Achieved = false;
        OK_HR_Timer = 0.0f;
        HR_Status = WarmupHR_Status.WarmupHR_NotSet;
        target_HR_Mid_Point = 0.0f;
        percentage_target_HR_Mid_Point = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!runHR_Warmup)
            return;

        hrData = HR_service.getLatestHeartRateData();
        bpm = hrData.heartRateBPM;
        indicatorPos = HR_Indicator.transform.localPosition.y;

        percentage_target_HR_Mid_Point = (bpm / target_HR_Mid_Point) * 100;

        if (hrData.heartRateBPM >= target_HR_Lower && hrData.heartRateBPM <= target_HR_Upper)
        {
            indicatorPos = MapValue(target_HR_Lower, target_HR_Upper, hrData.heartRateBPM, targetLowerIndicatorPos.y, targetUpperIndicatorPos.y); //HR range -> pos range
            HR_Indicator.transform.localPosition = new Vector3(132, indicatorPos, 0);

            if (runningOK_HR_Timer)
            {
                OK_HR_Timer += Time.deltaTime;

                if(OK_HR_Timer > 2.0f)
                    HR_Text.text = "Heart Rate Please Maintain Speed";

                if (OK_HR_Timer < OK_HR_Timer_Threshold)
                    return;

                //done!
                HR_Text.text = "Heart Rate OK";

                runHR_Warmup = false;
                HR_Achieved = true;
            }
            else
            {
                runningOK_HR_Timer = true;
                HR_Status = WarmupHR_Status.WarmupHR_OK;
            }

            return;
        }

        if(hrData.heartRateBPM > target_HR_Upper)
        {
            //HR too high!
            if (!HR_Status.Equals(WarmupHR_Status.WarmupHR_Above_Threshold))
            {
                HR_Status = WarmupHR_Status.WarmupHR_Above_Threshold;
                runningOK_HR_Timer = false;
                HR_Text.text = "Heart Rate Please Decrease";
                OK_HR_Timer = 0.0f;
            }
            indicatorPos = MapValue(target_HR_Upper, participant_HR_Reserve, hrData.heartRateBPM, targetUpperIndicatorPos.y, maxIndicatorPos.y); //HR range -> pos range
        }

        if (hrData.heartRateBPM < target_HR_Lower)
        {
            //HR too low!
            if (!HR_Status.Equals(WarmupHR_Status.WarmupHR_Below_Threshold))
            {
                HR_Status = WarmupHR_Status.WarmupHR_Below_Threshold;
                runningOK_HR_Timer = false;
                HR_Text.text = "Heart Rate Please Increase";
                OK_HR_Timer = 0.0f;
            }
            indicatorPos = MapValue(participant_HR_Rest, target_HR_Lower, hrData.heartRateBPM, minIndicatorPos.y, targetLowerIndicatorPos.y); //HR range -> pos range
        }

        //set pos indicator
        HR_Indicator.transform.localPosition = new Vector3(132, indicatorPos, 0);
    }

    public void TriggerWarmUp(int t_HR_Upper, int t_HR_Lower, int hr_reserve, double hr_rest)
    {
        target_HR_Lower = t_HR_Lower;
        target_HR_Upper = t_HR_Upper;
        target_HR_Mid_Point = (target_HR_Lower + target_HR_Upper) / 2;
        participant_HR_Reserve = hr_reserve;
        participant_HR_Rest = Mathf.RoundToInt((float)hr_rest);

        runHR_Warmup = true;
        runningOK_HR_Timer = false;
        OverideTargetHR = false;
        HR_Achieved = false;
        OK_HR_Timer = 0.0f;
        HR_Status = WarmupHR_Status.WarmupHR_NotSet;
        HR_Text.text = "Heart Rate";
        //fade canvas in
        HR_Canvas.GetComponent<FadeCanvas>().FadeInSetActive();        
    }

    public void EndWarmUp()
    {
        runHR_Warmup = false;
        HR_Canvas.GetComponent<FadeCanvas>().FadeOutSetUnactive();
    }

    private float MapValue(float lowerRange, float upperRange, float value, float globalLowerRange, float globalUpperRange) // Function to map a value between two ranges to a value between global variables
    {
        float clampedValue = Mathf.Clamp(value, Mathf.Min(lowerRange, upperRange), Mathf.Max(lowerRange, upperRange));
        float normalisedPos = (clampedValue - lowerRange) / (upperRange - lowerRange);
        float mappedValue = Mathf.Lerp(globalLowerRange, globalUpperRange, normalisedPos);
        return mappedValue;
    }
}
