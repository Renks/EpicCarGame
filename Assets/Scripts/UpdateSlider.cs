using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateSlider : MonoBehaviour
{
    public Slider slider;
    public TMP_Text sliderText;
    public float sliderValue = 0f;
    public RaycastCar raycastCar;

    void Start()
    {
        // Please assign this script to the wheel that you want to monitor
        if (raycastCar == null)
        {
            raycastCar = GetComponent<RaycastCar>();
        }
    }
    void Update()
    {

        sliderValue = Mathf.Abs(raycastCar.accelForceMag / raycastCar.acceleration); // Normalize to 0-1 range
        slider.value = sliderValue;
        sliderText.text = (sliderValue * 100).ToString("F0") + "%";
    }
}
