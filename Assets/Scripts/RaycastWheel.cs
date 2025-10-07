using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastWheel : MonoBehaviour
{
    // Note to self: Draw all Debug lines in LateUpdate to make sure they are rendered correctly (right now they lag one frame behind)
    [Header("Wheel Properties")]
    public float rayMaxLength = 1f;
    public float springStrength = 100f;
    public float springDamping = 2f;
    public float restDistance = 0.5f;
    public float overExtend = 0f;
    public float wheelRadius = 0.4f;
    public float zTraction = 0.05f; // Use a curve later
    public float zBrakeTracion = 0.25f; // Use a curve later

    // private float sideFriction = 1f; // How grippy tires are laterally
    private float longFriction = 1f; // Longitudinal Friction Factor, will change according to motor input

    [Header("Motor/Steer Properties")]
    public bool isMotor = false;
    public bool isSteer = false;
    public AnimationCurve gripCurve;

    [Header("Debug")]
    public bool showDebug = false;
    public Vector3 accelForce = Vector3.zero; // Public for debug and UI purposes

    [Header("References")]
    public Transform wheelMesh;
    public Rigidbody carRb;

    // private float engineForce = 0f;
    public float gripFactor = 0f;
    public bool isBraking = false;
    public bool isGrounded = false;

    public void ApplyWheelPhysics(RaycastCar car)
    {
        // Rotate wheel visuals Here Please
        isGrounded = false;
        Vector3 origin = transform.position;
        Vector3 springDirection = transform.up;
        if (Physics.Raycast(origin, -springDirection, out RaycastHit hit, rayMaxLength + wheelRadius + overExtend))
        {
            // Now the wheel is touching the ground, set isGrounded for other purposes (very bad practice but quick and dirty for now)
            isGrounded = true;

            // Suspension
            float springLen = Mathf.Max(0f, hit.distance - wheelRadius);
            float offset = restDistance - springLen;

            // Adjust Wheel Visual Position based on suspension compression (springLen)
            Vector3 wheelMeshLocalPos = wheelMesh.localPosition;
            wheelMeshLocalPos.y = Mathf.Lerp(wheelMeshLocalPos.y, -springLen, 5f * Time.fixedDeltaTime); // Lerping so it doesn't snap
            wheelMesh.localPosition = wheelMeshLocalPos;

            // Spring Forces
            float springForce = springStrength * offset;
            Vector3 tireVel = carRb.GetPointVelocity(origin);
            float tireRelativeVel = Vector3.Dot(springDirection, tireVel);
            float dampingForce = springDamping * tireRelativeVel;

            Vector3 yForce = (springForce - dampingForce) * hit.normal; // Used to be * springDirection instead of hit.normal

            carRb.AddForceAtPosition(yForce, hit.point);

            // Acceleration
            Vector3 forwardDir = transform.forward;
            float vel = Vector3.Dot(forwardDir, carRb.velocity);
            // Wheel Rotation —— Wheel Mesh's rotation must be 0 on y axis
            wheelMesh.Rotate(Vector3.right, vel / wheelRadius * Mathf.Rad2Deg * Time.fixedDeltaTime, Space.Self);
            // wheel.Rotate(vel / (2f * Mathf.PI * wheelRadius) * 360f * Time.fixedDeltaTime, 0f, 0f);
            /* // Also Rotate wheel when car is in air or flipped over and user is pressing acceleration/braking
            if (!isGrounded && isMotor && motorInput != 0) wheel.Rotate(motorInput * 200f * Time.fixedDeltaTime, 0f, 0f); */
            accelForce = Vector3.zero; // Reset accelForce for UI purposes
            if (isMotor && car.motorInput != 0)
            {
                float speedRatio = Mathf.Clamp01(vel / car.maxSpeed);
                float ac = car.accelCurve.Evaluate(speedRatio); // Use a different curve for braking?
                accelForce = forwardDir * car.acceleration * car.motorInput * ac;
                // add force at tire contact point
                carRb.AddForceAtPosition(accelForce, hit.point); // Should force be applied at wheel mesh's center?
                if (showDebug) Debug.DrawLine(hit.point, hit.point + accelForce / carRb.mass, Color.red);
            }

            // Tire X traction (Steering)
            Vector3 steerSideDir = transform.right;
            float steeringXVel = Vector3.Dot(steerSideDir, tireVel);
            gripFactor = Mathf.Abs(steeringXVel / tireVel.magnitude);
            float xTraction = gripCurve.Evaluate(gripFactor);

            if (!car.handBreakAction && gripFactor < 0.2)
            {
                car.isSlipping = false;
            }

            if (car.handBreakAction)
            {
                xTraction = 0.01f; // Reduce lateral tracton when handbrake is applied
            }
            else if (car.isSlipping)
            {
                xTraction = 0.1f;
            }

            // Lateral Friction  — Simple version
            float gravity = Physics.gravity.magnitude;
            Vector3 xForce = -steerSideDir * steeringXVel * xTraction * (carRb.mass * gravity / car.totalWheels); // Assume 4 wheels share the load equally
            Vector3 forcePos = wheelMesh.transform.position;  // should apply at hit.point for more suspension realism / bouncy effect
            carRb.AddForceAtPosition(xForce, forcePos);

            // Longitudinal Friction — Simple version
            if (car.motorInput != 0) longFriction = 0.1f; // Reduce longitudinal friction when accelerating/braking
            else longFriction = 1f;
            float fVel = Vector3.Dot(transform.forward, tireVel);
            float zFriction = zTraction;
            if (isBraking)
            {
                /* if (isBraking || (car.motorInput < 0f && vel > 1f) || (car.motorInput > 0f && vel < -1f)) */
                zFriction = zBrakeTracion; // Increase longitudinal friction when braking
            }
            Vector3 zForce = -transform.forward * longFriction * fVel * zFriction * (carRb.mass * gravity / car.totalWheels);
            carRb.AddForceAtPosition(zForce, hit.point); // you can apply at carRb position for more arcady feeling and spring compression but I'm going with realistic approach by applying directly at the tire contact point

            /*
            // Lateral Friction: F = M * A = M * dV/dt (Realistic but very intensive on CPU)
            float desiredAccel = (steeringXVel * xTraction) / Time.fixedDeltaTime;
            Vector3 xForce = -steerSideDir * desiredAccel * (rb.mass / 4f); // Assume 4 wheels share the load equally
            rb.AddForceAtPosition(xForce, hit.point);
            */
            if (!showDebug) return;
            Debug.DrawLine(origin, origin + -springDirection * (rayMaxLength + wheelRadius), Color.green); // Max raycast line
            Debug.DrawLine(hit.point, origin + yForce / carRb.mass, Color.blue); // Spring force debug line
            Debug.DrawLine(forcePos, forcePos + xForce / carRb.mass, Color.green); // Lateral friction debug line
            Debug.DrawLine(hit.point, hit.point + zForce / carRb.mass, Color.magenta); // Longitudinal friction debug line

        }

    }


}
