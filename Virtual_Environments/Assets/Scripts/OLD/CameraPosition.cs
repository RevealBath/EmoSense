using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    public Camera userCamera;
    public Transform neutralPosition;
    public Transform stressPosition;
    public Transform joyPosition;
    public Transform sadnessPosition;
    public Transform serenityPosition;
    public float sSpeed = 10.0f;
    public Vector3 dist;
    //public Transform lookTarget;

    public GameObject neutralScene;
    public GameObject stressScene;
    public GameObject joyScene;
    public GameObject sadnessScene;
    public GameObject serenityScene;

    private int currentPosition = 0;
    private int sceneTransition;
    private Transform cameraPosition;
    float elapsed = 0f;
    public bool userReady = false;
    
    public CollectingCoins collectedCoins;
    [SerializeField] private TextMeshProUGUI stressCoinText;
    [SerializeField] private TextMeshProUGUI joyCoinText;
    [SerializeField] private TextMeshProUGUI sadCoinText;
    [SerializeField] private TextMeshProUGUI serenityCoinText;
    
    public int[] cameraOrder;
    private int sceneNumber = 0;

    public void Start()
    {
        SetCameraTarget(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            userReady = true;
            SwitchCamera();
        }

        if (userReady)
        {
            elapsed += Time.deltaTime;

            if (elapsed >= 60.0f)
            {
                elapsed = 0f;
                SwitchCamera();
            }
        }
    }

    void FixedUpdate() {
        Vector3 dPos = cameraPosition.position + dist;
        //Vector3 sPos = Vector3.Lerp(transform.position, dPos, sSpeed * Time.deltaTime);
        transform.position = dPos;
        //transform.LookAt(lookTarget.position);
    }
    
    public void SetCameraTarget(int num){
        switch(num){
            case 0 :
                stressScene.SetActive(false);
                joyScene.SetActive(false);
                sadnessScene.SetActive(false);
                serenityScene.SetActive(false);
                collectedCoins.coins = 0;
                cameraPosition = neutralPosition.transform;
                neutralScene.SetActive(true);
                break;
            case 1 :
                neutralScene.SetActive(false);
                cameraPosition = stressPosition.transform;
                stressCoinText.text = collectedCoins.coins.ToString();
                stressScene.SetActive(true);
                break;
            case 2 :
                neutralScene.SetActive(false);
                cameraPosition = joyPosition.transform;
                joyCoinText.text = collectedCoins.coins.ToString();
                joyScene.SetActive(true);
                break;
            case 3 :
                neutralScene.SetActive(false);
                cameraPosition = sadnessPosition.transform;
                sadCoinText.text = collectedCoins.coins.ToString();
                sadnessScene.SetActive(true);
                break;
            case 4 :
                neutralScene.SetActive(false);
                cameraPosition = serenityPosition.transform;
                serenityCoinText.text = collectedCoins.coins.ToString();
                serenityScene.SetActive(true);
                break;
        }
    }
	
    public void SwitchCamera(){
        if (currentPosition < 8)
        {
            if (currentPosition%2 == 0)
            {
                sceneTransition = cameraOrder[sceneNumber];
                sceneNumber += 1;
            }
            else
            {
                sceneTransition = 0;
            }
            currentPosition++;
        }
        else
        {
            currentPosition = 1;
        }
        SetCameraTarget(sceneTransition);
    }
}