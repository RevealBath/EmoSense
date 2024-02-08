using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextChanger_Joy : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stressText;
    public float timeRemaining = 10;
    public int x = 0;
    public string[] phraseArray = new string[] {"You're Doing\nGreat!", "10 Bonus\nPoints!", "Keep Up The\nGood Work!", "Good Job!", "You've Got\nLots of Coins!", "Level Up!", "You Are\nThe Best!"};
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