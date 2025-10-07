using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateCarUI : MonoBehaviour
{
    public RaycastCar raycastCar;
    private Rigidbody carRigidbody;
    public TMP_Text txtCarSpeed;
    public TMP_Text txtCarAccelForce;
    public Toggle toggleHandBreak;
    public Toggle toggleIsSlipping;
    public Toggle toggleIsBraking;

    // Start is called before the first frame update
    void Start()
    {
        if (raycastCar == null)
        {
            raycastCar = GetComponent<RaycastCar>();
        }
        if (carRigidbody == null)
        {
            carRigidbody = raycastCar.gameObject.GetComponent<Rigidbody>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        txtCarSpeed.text = "Car Speed: " + (carRigidbody.velocity.magnitude * 3.6f).ToString("F1") + " km/h\n";
        txtCarAccelForce.text = "AccelForce: " + raycastCar.accelForceMag.ToString("F2") + " / " + raycastCar.acceleration.ToString("F2") + "\n";
        toggleHandBreak.isOn = raycastCar.handBreakAction;
        toggleIsSlipping.isOn = raycastCar.isSlipping;
        toggleIsBraking.isOn = raycastCar.wheels[0].isBraking;
        /* toggleIsBraking.isOn = car.wheels[0].isBraking || car.wheels[1].isBraking || car.wheels[2].isBraking || car.wheels[3].isBraking; */
    }
}
