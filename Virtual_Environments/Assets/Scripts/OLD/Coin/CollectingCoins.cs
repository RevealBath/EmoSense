using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CollectingCoins : MonoBehaviour
{
    public enum CoinCollidedWith
    {
        NoCollision,
        NormalCoin,
        Heart,
        RustyCoin,
        Skull
    }
    public CoinCollidedWith coinCollisionStatus;
    public int coins = 0;
    [SerializeField] private TextMeshProUGUI stressCoinText;
    [SerializeField] private TextMeshProUGUI joyCoinText;
    [SerializeField] private TextMeshProUGUI sadCoinText;
    [SerializeField] private TextMeshProUGUI serenityCoinText;
    public AudioSource coinSource;
    public AudioSource skullSource;
    public AudioSource rustySource;
    
    public GameObject serenityScene;
    protected float Timer;
    private int delayAmount = 2; //second count
    private bool collided;
    private float collisionTimer;

    public void Start()
    {
        coinCollisionStatus = CoinCollidedWith.NoCollision;
        collided = false;
        collisionTimer = 0.0f;
    }

    public void Update()
    {
        if (serenityScene.activeSelf)
        {
            Timer += Time.deltaTime;
            
            if (Timer >= delayAmount)
            {
                Timer = 0f;
                coins += 1;
                serenityCoinText.text = coins.ToString();
            }
        }

        if (!collided)
            return;

        collisionTimer += Time.deltaTime;

        if (collisionTimer < 0.05f)
            return;

        //print("resetting");
        coinCollisionStatus = CoinCollidedWith.NoCollision;
        collided = false;
        collisionTimer = 0.0f;
    }

    public void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Coin")
        {
            coins += 1;
            //col.gameObject.SetActive(false);
            coinSource.Play();
            Destroy(col.gameObject);
            stressCoinText.text = coins.ToString();
            joyCoinText.text = coins.ToString();
            coinCollisionStatus = CoinCollidedWith.NormalCoin;
        }
        
        if (col.gameObject.tag == "Skull")
        {
            coins -= 10;
            //col.gameObject.SetActive(false);
            skullSource.Play();
            Destroy(col.gameObject);
            stressCoinText.text = coins.ToString();
            coinCollisionStatus = CoinCollidedWith.Skull;
         }
        
        if (col.gameObject.tag == "Rusty")
        {
            coins += 1;
            //col.gameObject.SetActive(false);
            rustySource.Play();
            Destroy(col.gameObject);
            sadCoinText.text = coins.ToString();
            coinCollisionStatus = CoinCollidedWith.RustyCoin;            
        }
        
        if (col.gameObject.tag == "Heart")
        {
            coins += 10;
            //col.gameObject.SetActive(false);
            coinSource.Play();
            Destroy(col.gameObject);
            joyCoinText.text = coins.ToString();
            coinCollisionStatus = CoinCollidedWith.Heart;            
        }

        collided = true;
    }

    public void SetCoinText(StudyManager.ShownScene scene)
    {
        switch(scene){
           case StudyManager.ShownScene.Joy:
                {
                    joyCoinText.text = coins.ToString();
                    break;
                }
            case StudyManager.ShownScene.Sadness:
                {
                    sadCoinText.text = coins.ToString();
                    break;
                }
            case StudyManager.ShownScene.Stress:
                {
                    stressCoinText.text = coins.ToString();
                    break;
                }
            case StudyManager.ShownScene.Serenity:
                {
                    serenityCoinText.text = coins.ToString();
                    break;
                }
        }
    }
}
