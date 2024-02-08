using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{
    [SerializeField] private Slider _slider;

    [SerializeField] private TextMeshProUGUI _sliderText;

    void Start ()
    {
        _slider.onValueChanged.AddListener((v) =>
        {
            _sliderText.text = v.ToString("0");
        });
    }

    public void SubtractTen()
    {
        _slider.value -= 10;
        _sliderText.text = _slider.value.ToString("0");
    }

    public void AddTen()
    {
        _slider.value += 10;
        _sliderText.text = _slider.value.ToString("0");
    }
}
