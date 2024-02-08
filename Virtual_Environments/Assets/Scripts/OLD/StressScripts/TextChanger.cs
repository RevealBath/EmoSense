using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextChanger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stressText;
    public float timeRemaining = 10;
    public int x = 0;
    public string[] phraseArray = new string[] {"Don't Mess\nUp!", "The Police\nAre Coming!", "Time Is\nRunning Out!", "Collect The\nCoins Quickly!", "You Need\nMore Coins!", "Hurry!", "You Are\nBeing Chased!"};

    void Start()
    {
        DisplayText();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            x += 1;
            DisplayText();
            timeRemaining = 10;

        }
    }
    void DisplayText()
    {
        stressText.text = phraseArray[x];
    }
}