using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastCar : MonoBehaviour
{
    private Rigidbody rb;
    public RaycastWheel[] wheels = new RaycastWheel[4];
    public float acceleration = 600f;
    public float maxSpeed = 20f; // in meters per second
    public AnimationCurve accelCurve;
    public float tireTurnSpeed = 2f; // in radians per second
    public float tireMaxTurnDegrees = 25f;
    public TrailRenderer[] skidMarks = new TrailRenderer[4];
    public bool showDebug = false;
    [HideInInspector] public float totalWheels = 4f; // Just in case we want to make a 6 wheeler or something
    public float motorInput = 0f;
    public float turnInput = 0f;
    public float wheelAngle = 0f; // target angle of the wheel
    public bool handBreakAction = false;
    public bool isSlipping = false;
    public bool isGrounded = false;
    public Vector3 centerOfMass = Vector3.zero;
    private GameObject debugSphere;
    public float accelForceMag = 0f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        centerOfMass = rb.centerOfMass; // Let designer set it in the inspector instead of using default rb.centerOfMass;

        totalWheels = wheels.Length;

        if (totalWheels > 0)
        {
            foreach (RaycastWheel wheel in wheels)
            {
                if (wheel.carRb == null) wheel.carRb = rb;
                if (wheel.wheelMesh == null && wheel.transform.childCount > 0) wheel.wheelMesh = wheel.transform.GetChild(0);
                wheel.showDebug = showDebug;
            }

        }
        // For Debugging purposes only
        debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugSphere.transform.parent = transform;
        debugSphere.transform.localScale = Vector3.one * 0.15f;
        debugSphere.transform.position = Vector3.zero;
        debugSphere.transform.localPosition = Vector3.zero;
        debugSphere.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("Plastic");
        debugSphere.GetComponent<Collider>().enabled = false;

    }

    // Update is called once per frame
    void Update()
    {
        motorInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
        if (Input.GetKey(KeyCode.Space))
        {
            handBreakAction = true;
            isSlipping = true;
        }
        else
        {
            handBreakAction = false;
        }


        // Just for testing purposes
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = Vector3.up * 2f;
            transform.rotation = Quaternion.identity;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }


    void FixedUpdate()
    {
        int idx = 0;
        isGrounded = false;
        foreach (RaycastWheel wheel in wheels)
        {
            wheel.ApplyWheelPhysics(this);
            // UI purposes only
            accelForceMag = wheel.accelForce.magnitude;
            BasicSteeringRotation(wheel);

            // Braking
            if (Input.GetKey(KeyCode.LeftShift))
            {
                // Note to self: Make sure the car is grounded before applying brake force
                wheel.isBraking = true;
            }
            else
            {
                wheel.isBraking = false;
            }

            // Skid marks
            if (!handBreakAction && wheel.gripFactor < 0.2)
            {
                isSlipping = false;
                skidMarks[idx].emitting = false;
            }

            if (handBreakAction && !skidMarks[idx].emitting) skidMarks[idx].emitting = true;
            if (wheel.isGrounded) isGrounded = true; // If at least one wheel is grounded, the car is grounded
            idx += 1;
        }

        // Grounding will return true if at least one wheel is grounded (very basic implementation for now)
        if (isGrounded)
        {
            rb.centerOfMass = centerOfMass;
        }
        else
        {
            // Lower the center of mass when in the air to make it more stable (maybe gradually using lerp?)
            rb.centerOfMass = centerOfMass + (Vector3.down * 0.45f);

            // Honestly, our centerOfMass calc is very very very bad.
        }



    }


    private void BasicSteeringRotation(RaycastWheel wheel)
    {
        if (!wheel.isSteer) return;

        if (turnInput != 0)
        {
            wheelAngle = Mathf.Lerp(wheelAngle, tireMaxTurnDegrees * turnInput, Time.deltaTime * tireTurnSpeed);
            wheel.transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
        }
        else
        {
            wheelAngle = Mathf.Lerp(wheelAngle, 0f, Time.deltaTime * tireTurnSpeed);
            wheel.transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
        }

    }

    void LateUpdate()
    {
        if (!showDebug) return;
        Debug.DrawLine(transform.position, transform.position + rb.velocity, Color.yellow); // Car velocity debug line
        // Visualize COM with sphere
        debugSphere.transform.position = transform.TransformPoint(rb.centerOfMass);
    }

}
