using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextChanger_Serenity : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stressText;
    public float timeRemaining = 10;
    public int x = 0;
    public string[] phraseArray = new string[] {"Gently pedal\nfor points", "Breathe in...", "And out...", "There is\nno rush", "Relax your\nbreathing", "Calm your\nmind"};
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