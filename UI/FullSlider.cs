using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class FullSlider : MonoBehaviour
{
    Text label;
    Slider slider;
    public string valueFormat = "{0:0}";
    public float valueToUIFactor=1;
    float min;
    float max;
    float step;

    public Slider.SliderEvent onValueChanged;

    // valore reale
    public float value
    {
        get => min + slider.value / valueToUIFactor;
        set => slider.value = (value - min)* valueToUIFactor;
    }

    private void Awake()
    {
        label = transform.FindInChildren("SliderValueText").GetComponent<Text>();
        slider = GetComponentInChildren<Slider>();
    }

    public void Initialize(float min, float max, float step, string valueFormat,float startValue)
    {
        this.step = step;
        this.min = min;
        this.max = max;
        this.valueFormat = valueFormat;
        double total = ((double)max - (double)min) / (double)step;
        slider.minValue = 0;
        slider.maxValue = (float)total;
        valueToUIFactor = 1f / step;
        value = startValue;
    }

    private void Start()
    {
        label.text = string.Format(valueFormat, value);

        GetComponentInChildren<Slider>().onValueChanged.AddListener((v) =>
       {
           label.text = string.Format(valueFormat, min + v  / valueToUIFactor);
           onValueChanged.Invoke(value);
       });
    }
}

