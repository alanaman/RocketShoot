using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Slider slider;

    public void SetHealth(float health)
    {
        slider.value = health;
    }

    public void SetMaxHeath(float health)
    {
        slider.maxValue = health;
        slider.value = health;
    }
}
