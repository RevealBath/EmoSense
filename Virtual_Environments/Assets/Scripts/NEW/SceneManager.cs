using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public StudyManager.ShownScene[] scenes;
    public bool configured;

    public Camera mainCamera;
    public Skybox skybox;

    public GameObject joyScene;
    public GameObject stressScene;
    public GameObject sadnessScene;
    public GameObject serenityScene;
    public GameObject neutralScene;

    public Transform joyPos;
    public Transform stressPos;
    public Transform sadnessPos;
    public Transform serenityPos;
    public Transform neutralPos;

    public AudioSource backgroundMusic;
    public AudioClip stressSoundtrack;
    public AudioClip joySoundtrack;
    public AudioClip sadnessSoundtrack;
    public AudioClip serenitySoundtrack;

    public GameObject fadeObject;

    public ColorScheme previousColourScheme;
    public ColorScheme currentColourScheme;

    public Color Skybox_TopColour;
    public Color Skybox_MiddleColour;
    public Color Skybox_BottomColour;

    public CollectingCoins cc;

    private StudyManager studyManager;

    public bool lerpSkyboxColour;
    private float skyboxLerpTimer;

    public bool screenFadeRunning;
    public float fadeEffectDuration = 1.0f;

    private PoliceSiren ps;
    private Timer stressTimer;

    // Start is called before the first frame update
    void Start()
    {
        studyManager = GetComponentInParent<StudyManager>();
        previousColourScheme = ColorScheme.Standard;
        currentColourScheme = ColorScheme.Standard;

        SetSkyboxColours();

        configured = false;
        lerpSkyboxColour = false;
        screenFadeRunning = false;

        ps = stressScene.GetComponentInChildren<PoliceSiren>();
        stressTimer = stressScene.GetComponentInChildren<Timer>();
    }

    public void ConfigureSceneOrder(StudyManager.ConditionOrder order)
    {
        switch (order)
        {
            case StudyManager.ConditionOrder.One: //Joy, Serenity, Stress, Sadness
                {
                    scenes = new StudyManager.ShownScene[4]{ StudyManager.ShownScene.Joy, StudyManager.ShownScene.Serenity, StudyManager.ShownScene.Stress, StudyManager.ShownScene.Sadness};
                    break;
                }
            case StudyManager.ConditionOrder.Two: //Serenity, Stress, Sadness, Joy
                {
                    scenes = new StudyManager.ShownScene[4] { StudyManager.ShownScene.Serenity, StudyManager.ShownScene.Stress, StudyManager.ShownScene.Sadness, StudyManager.ShownScene.Joy };
                    break;
                }
            case StudyManager.ConditionOrder.Three: //Sadness, Joy, Serenity, Stress
                {
                    scenes = new StudyManager.ShownScene[4] { StudyManager.ShownScene.Sadness, StudyManager.ShownScene.Joy, StudyManager.ShownScene.Serenity, StudyManager.ShownScene.Stress };
                    break;
                }
            case StudyManager.ConditionOrder.Four: //Stress, Sadness, Joy, Serenity
                {
                    scenes = new StudyManager.ShownScene[4] { StudyManager.ShownScene.Stress, StudyManager.ShownScene.Sadness, StudyManager.ShownScene.Joy, StudyManager.ShownScene.Serenity };
                    break;
                }
        }
        configured = true;
    }       


    // Update is called once per frame
    void Update()
    {
        // check if we should lerp the skybox --- or the fade the screen
        if (!lerpSkyboxColour)
            return;

        if (skyboxLerpTimer <= 1)
        {
            skyboxLerpTimer += .002f;
            LerpSkyboxColours();
            skybox.material.SetColor("_SkyColor1", Skybox_TopColour);
            skybox.material.SetColor("_SkyColor2", Skybox_MiddleColour);
            skybox.material.SetColor("_SkyColor3", Skybox_BottomColour);            
        }
        else
        {
            //lerp done
            lerpSkyboxColour = false;
            skyboxLerpTimer = 0f;
        }
    }

    public void TriggerNeutralScene()
    {
        stressScene.SetActive(false);
        joyScene.SetActive(false);
        sadnessScene.SetActive(false);
        serenityScene.SetActive(false);
        neutralScene.SetActive(true);

        backgroundMusic.Stop();

        // BETWEEN scene coin reset
        cc.coins = 0;

        mainCamera.transform.position = neutralPos.position;
        
        previousColourScheme = currentColourScheme;
        currentColourScheme = ColorScheme.Standard;
        lerpSkyboxColour = true;

        studyManager.shownScene = StudyManager.ShownScene.Neutral;
    }

    public void TriggerEmotionScene(int emotionCount)
    {
        switch (scenes[emotionCount])
        {
            case StudyManager.ShownScene.Joy:
                {
                    neutralScene.SetActive(false);
                    joyScene.SetActive(true);

                    mainCamera.transform.position = joyPos.position;

                    backgroundMusic.Stop();
                    backgroundMusic.clip = joySoundtrack;
                    backgroundMusic.Play();

                    //previousColourScheme = currentColourScheme;
                    //currentColourScheme = ColorScheme.Joy;
                    //SetSkyboxColours();

                    studyManager.shownScene = StudyManager.ShownScene.Joy;
                    cc.SetCoinText(studyManager.shownScene);

                    break;
                }
            case StudyManager.ShownScene.Stress:
                {
                    neutralScene.SetActive(false);
                    stressScene.SetActive(true);

                    mainCamera.transform.position = stressPos.position;

                    backgroundMusic.Stop();
                    backgroundMusic.clip = stressSoundtrack;
                    backgroundMusic.Play();

                    //previousColourScheme = currentColourScheme;
                    //currentColourScheme = ColorScheme.Stress;
                    //SetSkyboxColours();

                    ps.play = true;
                    stressTimer.play = true;

                    studyManager.shownScene = StudyManager.ShownScene.Stress;
                    cc.SetCoinText(studyManager.shownScene);

                    break;
                }
            case StudyManager.ShownScene.Sadness:
                {
                    neutralScene.SetActive(false);
                    sadnessScene.SetActive(true);

                    mainCamera.transform.position = sadnessPos.position;                    

                    backgroundMusic.Stop();
                    backgroundMusic.clip = sadnessSoundtrack;
                    backgroundMusic.Play();

                    //previousColourScheme = currentColourScheme;
                    //currentColourScheme = ColorScheme.Sadness;
                    //SetSkyboxColours();

                    studyManager.shownScene = StudyManager.ShownScene.Sadness;
                    cc.SetCoinText(studyManager.shownScene);

                    break;
                }
            case StudyManager.ShownScene.Serenity:
                {
                    neutralScene.SetActive(false);
                    serenityScene.SetActive(true);
                    mainCamera.transform.position = serenityPos.position;

                    cc.SetCoinText(StudyManager.ShownScene.Serenity);

                    backgroundMusic.Stop();
                    backgroundMusic.clip = serenitySoundtrack;
                    backgroundMusic.Play();

                    //previousColourScheme = currentColourScheme;
                    //currentColourScheme = ColorScheme.Serenity;
                    //SetSkyboxColours();

                    studyManager.shownScene = StudyManager.ShownScene.Serenity;
                    cc.SetCoinText(studyManager.shownScene);

                    break;
                }
        }
    }

    public void TriggerEmotionSceneSkybox(int nextScene)
    {
        previousColourScheme = currentColourScheme;

        switch (scenes[nextScene])
        {
            case StudyManager.ShownScene.Serenity:
                {
                    currentColourScheme = ColorScheme.Serenity;
                    break;
                }
            case StudyManager.ShownScene.Stress:
                {
                    currentColourScheme = ColorScheme.Stress;
                    break;
                }
            case StudyManager.ShownScene.Joy:
                {
                    currentColourScheme = ColorScheme.Joy;
                    break;
                }
            case StudyManager.ShownScene.Sadness:
                {
                    currentColourScheme = ColorScheme.Sadness;
                    break;
                }
        }        
        lerpSkyboxColour = true;
    }

    private void LerpSkyboxColours()
    {
        Color[] prevColors = Colors.GetColorScheme(previousColourScheme);
        Color[] colors = Colors.GetColorScheme(currentColourScheme);
        Skybox_TopColour = Color.Lerp(prevColors[0], colors[0], skyboxLerpTimer);
        Skybox_MiddleColour = Color.Lerp(prevColors[1], colors[1], skyboxLerpTimer);
        Skybox_BottomColour = Color.Lerp(prevColors[2], colors[2], skyboxLerpTimer);
    }
    
    private void SetSkyboxColours()
    {
        Color[] colors = Colors.GetColorScheme(currentColourScheme); //gets the colours of the first scheme you want
        Skybox_TopColour = colors[0];
        Skybox_MiddleColour = colors[1];
        Skybox_BottomColour = colors[2];

        skybox.material.SetColor("_SkyColor1", Skybox_TopColour);
        skybox.material.SetColor("_SkyColor2", Skybox_MiddleColour);
        skybox.material.SetColor("_SkyColor3", Skybox_BottomColour);
    }

    public void triggerFadeEffect() 
    { 
        fadeObject.SetActive(true);
        screenFadeRunning = true;
    }

    public GameObject GetNextScene(int sceneCount)
    {
        switch (scenes[sceneCount])
        {
            case StudyManager.ShownScene.Serenity:
                {
                    return serenityScene;
                }
            case StudyManager.ShownScene.Stress:
                {
                    return stressScene;
                    
                }
            case StudyManager.ShownScene.Joy:
                {
                    return joyScene;
                    
                }
            case StudyManager.ShownScene.Sadness:
                {
                    return sadnessScene;
                    
                }
        }
        return null;
    }
}
