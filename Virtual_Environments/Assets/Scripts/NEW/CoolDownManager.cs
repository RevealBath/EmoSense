using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class CoolDownManager : MonoBehaviour
{
    public enum CoolDownState
    {
        NotCoolingDown,
        MeasuringAffectSlider,
        MeasuringEmotions,
        FinishedMeasurements
    }

    public StudyManager studyManager;
    public float cooldownTimeThreshold;
    public GameObject affectSlider;
    public GameObject arousalSlider;
    public GameObject valenceSlider;
    public GameObject sliderIndicator;
    public TextMeshProUGUI affectSliderText;

    public GameObject emotionQuestions;
    public TextMeshProUGUI emotionQText;

    public GameObject thankyouMessageObj;
    public AudioSource audio_Q;
    public AudioSource audio_emotion_primer;

    public AudioClip AS_Valence_Audio;
    public AudioClip AS_Arousal_Audio;
    public AudioClip emotionQ_Primer;
    public AudioClip emotionQ_Fear;
    public AudioClip emotionQ_Stress;
    public AudioClip emotionQ_Sad;
    public AudioClip emotionQ_Calm;
    public AudioClip emotionQ_Bored;
    public AudioClip emotionQ_Content;
    public AudioClip emotionQ_Happy;
    public AudioClip emotionQ_Excited;
    public AudioClip thankyou_next;
    public AudioClip thankyou_end;

    public CoolDownState coolDownState;

    public int participantAnswer = -1;
    public bool moveSlider = false;
    public bool participantInputReady = false;

    private List<string> emotionQ_Order;
    private int affectResponseID; //arousal = 0, valence = 1
    private bool timerTriggered;
    private float cooldownWaitTimer;
    private bool waitingForParticipantResponse;
    private AffectResponses ar;
    private int emotionResponseCounter;
    private bool emotionFirstQ;
    private bool affectFirstQ;

    public delegate void AffectResponsesReady(AffectResponses ar);
    public static event AffectResponsesReady OnAffectResponsesReady;

    private List<Vector3> sliderIndicatorPos = new List<Vector3>
    {
        new Vector3(-210.5f, 17.5f, 0f),    //0
        new Vector3(-173.5f, 17.5f, 0f),    //1
        new Vector3(-129.5f, 17.5f, 0f),    //2
        new Vector3(-82.5f, 17.5f, 0f),     //3
        new Vector3(-34.5f, 17.5f, 0f),     //4
        new Vector3(10f, 17.5f, 0f),        //5
        new Vector3(54.5f, 17.5f, 0f),      //6
        new Vector3(100.5f, 17.5f, 0f),     //7
        new Vector3(144.5f, 17.5f, 0f),     //8
        new Vector3(190.5f, 17.5f, 0f),     //9
        new Vector3(227.5f, 17.5f, 0f)      //10
    };

    // Start is called before the first frame update
    void Start()
    {
        emotionQ_Order = new List<string> {"fear", "stress", "sad", "bored", "calm", "content", "happy", "excited"};
        timerTriggered = false;
        waitingForParticipantResponse = false;
        cooldownWaitTimer = cooldownTimeThreshold;
        ar = new AffectResponses();
        coolDownState = CoolDownState.NotCoolingDown;
        affectResponseID = -1;
        emotionFirstQ = true;
        affectFirstQ = true;
        audio_emotion_primer.clip = emotionQ_Primer;
    }

    // Update is called once per frame
    void Update()
    {
        if (coolDownState.Equals(CoolDownState.FinishedMeasurements))
            return;

        if (timerTriggered && !waitingForParticipantResponse)
        {
            cooldownWaitTimer -= Time.deltaTime;

            if (cooldownWaitTimer > 0)
                return;

            //trigger Affect Slider Question
            if (coolDownState.Equals(CoolDownState.MeasuringAffectSlider))
            {
                if(affectResponseID == -1)
                {
                    System.Random r = new System.Random();
                    affectResponseID = r.Next(0, 2);
                }                    
                LoadAffectSlider(affectResponseID);
            }
            else if (coolDownState.Equals(CoolDownState.MeasuringEmotions))
            {
                LoadEmotionQuestion();
            }

            waitingForParticipantResponse = true;
            timerTriggered = false;
            cooldownWaitTimer = cooldownTimeThreshold;            
        }
        else if (waitingForParticipantResponse)
        {
            //continuously check to see if bool changed
            if (!participantInputReady && !moveSlider)
                return;

            if (moveSlider)
                MoveAffectSlider();

            if(participantInputReady)
                RecordAffectResponse();
        }
    }

    public void TriggerCoolDown(StudyManager.ShownScene scene, StudyManager.ExerciseIntensity exercise, int sceneCount)
    {
        // randomise slider order
        RandomizeList(emotionQ_Order);
        timerTriggered = true;
        coolDownState = CoolDownState.MeasuringAffectSlider;
        emotionFirstQ = true;
        affectFirstQ = true;
        waitingForParticipantResponse = false;
        affectResponseID = -1;
        emotionResponseCounter = 0;
        sliderIndicator.transform.localPosition = sliderIndicatorPos[5];

        ar = new AffectResponses();
        ar.ExposureScene = scene;
        ar.exerciseIntensity = exercise;
        ar.emotionSceneCount = sceneCount;
    }

    private void EndCoolDown()
    {
        thankyouMessageObj.GetComponent<FadeCanvas>().FadeInSetActive();

        if (studyManager.emotionSceneCount < 4)
            audio_emotion_primer.clip = thankyou_next;
        else
            audio_emotion_primer.clip = thankyou_end;

        audio_emotion_primer.Play();

        if(OnAffectResponsesReady != null)
            OnAffectResponsesReady(ar);
    }

    public void LoadEmotionQuestion()
    {
        string emotion = emotionQ_Order[emotionResponseCounter];
        //audio_Q.Stop();
        switch (emotion)
        {
            case "fear":
                {
                    emotionQText.text = "How Fearful did you feel?";
                    audio_Q.clip = emotionQ_Fear;
                    break;
                }
            case "stress":
                {
                    emotionQText.text = "How Stressed did you feel?";
                    audio_Q.clip = emotionQ_Stress;
                    break;
                }
            case "sad":
                {
                    emotionQText.text = "How Sad did you feel?";
                    audio_Q.clip = emotionQ_Sad;
                    break;
                }
            case "bored":
                {
                    emotionQText.text = "How Bored did you feel?";
                    audio_Q.clip = emotionQ_Bored;
                    break;
                }
            case "calm":
                {
                    emotionQText.text = "How Calm did you feel?";
                    audio_Q.clip = emotionQ_Calm;
                    break;
                }
            case "content":
                {
                    emotionQText.text = "How Content did you feel?";                    
                    audio_Q.clip = emotionQ_Content;
                    break;
                }
            case "happy":
                {
                    emotionQText.text = "How Happy did you feel?";
                    audio_Q.clip = emotionQ_Happy;
                    break;
                }
            case "excited":
                {
                    emotionQText.text = "How Excited did you feel?";
                    audio_Q.clip = emotionQ_Excited;
                    break;
                }
        }

        if (emotionFirstQ)
        {
            emotionFirstQ = false;
            emotionQuestions.GetComponent<FadeCanvas>().FadeInSetActive();
            audio_Q.PlayDelayed(2.5f);
        }
        else
            audio_Q.Play();        
    }

    public void LoadAffectSlider(int val)
    {
        sliderIndicator.transform.localPosition = sliderIndicatorPos[5];

        if (affectFirstQ)
        {
            affectFirstQ = false;
            affectSlider.GetComponent<FadeCanvas>().FadeInSetActive();
        }
        audio_Q.Stop();
        if(val == 0)
        {
            affectSliderText.text = "Please rate your level of Arousal";
            audio_Q.clip = AS_Arousal_Audio;
            arousalSlider.SetActive(true);
            valenceSlider.SetActive(false);
        }
        else
        {
            affectSliderText.text = "Please rate your level of Pleasure";
            audio_Q.clip = AS_Valence_Audio;
            arousalSlider.SetActive(false);
            valenceSlider.SetActive(true);
        }
        audio_Q.Play();
    }

    public void RecordAffectResponse()
    {
        participantInputReady = false;

        //input validation
        if (participantAnswer < 0 || participantAnswer > 10)
        {
            print("Please enter participant response to Question before proceeding");
            return;
        }

        if (coolDownState.Equals(CoolDownState.MeasuringAffectSlider))
        {
            //write to affectresponse
            WriteAffectResponse();

            if (ar.arousalValue != -1 && ar.valenceValue != -1) //check if we're done with all affect responses
            {
                affectSlider.GetComponent<FadeCanvas>().FadeOutSetUnactive();
                coolDownState = CoolDownState.MeasuringEmotions;
                audio_emotion_primer.clip = emotionQ_Primer;
                audio_emotion_primer.Play();
            }
            else
            {
                affectSlider.GetComponent<FadeCanvas>().FadeInOut();                
            }                
        }
        else if (coolDownState.Equals(CoolDownState.MeasuringEmotions))
        {
            //write to affectresponse
            WriteEmotionResponse();            

            if (ar.DataValid()) //check if we're done with all emotion responses
            {
                //change cooldown state write affect response to file and change state to finished            
                emotionQuestions.GetComponent<FadeCanvas>().FadeOutSetUnactive();
                coolDownState = CoolDownState.FinishedMeasurements;
                EndCoolDown();
            }
            else
            {
                //next one!
                emotionQuestions.GetComponent<FadeCanvas>().FadeInOut();
                emotionResponseCounter++;
            }
        }
        waitingForParticipantResponse = false;
        timerTriggered = true;
    }

    private void WriteAffectResponse()
    {
        if(affectResponseID == 0) //arousal
        {
            ar.arousalValue = participantAnswer;
            affectResponseID = 1;
        }
        else //valence
        {
            ar.valenceValue = participantAnswer;
            affectResponseID = 0;
        }
        participantAnswer = -1;
    }

    private void WriteEmotionResponse()
    {
        string emotion = emotionQ_Order[emotionResponseCounter];
        switch (emotion) 
        {
            case "fear":
                {
                    ar.fearValue = participantAnswer;
                    break;
                }
            case "stress":
                {
                    ar.stressValue = participantAnswer;
                    break;
                }
            case "sad":
                {
                    ar.sadValue = participantAnswer;
                    break;
                }
            case "bored":
                {
                    ar.boredValue = participantAnswer;
                    break;
                }
            case "calm":
                {
                    ar.calmValue = participantAnswer;
                    break;
                }
            case "content":
                {
                    ar.contentValue = participantAnswer;
                    break;
                }
            case "happy":
                {
                    ar.happyValue = participantAnswer;
                    break;
                }
            case "excited":
                {
                    ar.excitedValue = participantAnswer;
                    break;
                }            
        }
        
        participantAnswer = -1;
    }

    private void MoveAffectSlider()
    {
        moveSlider = false;
        if (participantAnswer < 0 || participantAnswer > 10)
        {
            print("Please enter participant response to Question before proceeding");
            return;
        }

        sliderIndicator.transform.localPosition = sliderIndicatorPos[participantAnswer];
    }

    private static void RandomizeList<T>(List<T> list)
    {
        System.Random random = new System.Random();

        int n = list.Count;
        while (n > 1)
        {
            int k = random.Next(n);
            n--;
            T temp = list[k];
            list[k] = list[n];
            list[n] = temp;
        }
    }
}

public class AffectResponses
{
    public StudyManager.ShownScene ExposureScene { get; set; }
    public StudyManager.ExerciseIntensity exerciseIntensity { get; set; }
    public int emotionSceneCount { get; set; }
    public int valenceValue { get; set; }
    public int arousalValue { get; set; }
    public int fearValue { get; set; }
    public int stressValue { get; set; }
    public int sadValue { get; set; }
    public int boredValue { get; set; }
    public int calmValue { get; set; }
    public int contentValue { get; set; }
    public int happyValue { get; set; }
    public int excitedValue { get; set; }

    public AffectResponses()
    {
        valenceValue = -1;
        arousalValue = -1;
        fearValue = -1;
        stressValue = -1;
        sadValue = -1;
        boredValue = -1;
        calmValue = -1;
        contentValue = -1;
        happyValue = -1;
        excitedValue = -1;
    }

    public bool DataValid()
    {
        if (valenceValue == -1 || arousalValue == -1 || fearValue == -1 || 
            stressValue == -1 || sadValue == -1 || boredValue == -1 || 
            calmValue == -1 || contentValue == -1 || happyValue == -1 || excitedValue == -1)        
            return false;        
        else
            return true;
    }
}
