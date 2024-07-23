using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

[Serializable]
public class PidController
{
    //[SerializeField] float ACC_SCALE = 300;
    //[SerializeField] float LATERAL_DAMP = 0.5f;
    //[SerializeField] float INERTIA = 125;

    [SerializeField] float Kp = 4000;
    [SerializeField] float Ki = 1000;
    [SerializeField] float Kd = 1500;
    [SerializeField] float N = 5;


    float[] err = { 0, 0, 0 };
    float tau;
    float d0 = 0, d1 = 0, fd0 = 0, fd1 = 0, torque = 0;

    Vector3 prevAxis = Vector3.up;

    Transform transform;

    public PidController(Transform transform)
    {
        err = new float[3];
        d0 = 0; d1 = 0; fd0 = 0; fd1 = 0; torque = 0;
        this.transform = transform;
        tau = Kd / (Kp * N);
    }

    public void Init(Transform transform)
    {
        err = new float[3];
        d0 = 0; d1 = 0; fd0 = 0; fd1 = 0; torque = 0;
        this.transform = transform;
        tau = Kd / (Kp * N);
    }

    //To be called ONCE every frame (in FixedUpdate)
    public void ApplyTorque(Vector3 currentDir, Vector3 targetDir)
    {
        float deltaAngle = Vector3.Angle(targetDir, currentDir) * Mathf.Deg2Rad/Mathf.PI;

        Vector3 axis = Vector3.Cross(currentDir, targetDir).normalized;

        if(Vector3.Angle(axis, prevAxis) > Vector3.Angle(-axis, prevAxis))
        {
            axis = -axis;
            deltaAngle = -deltaAngle;
        }

        prevAxis = axis;

        CalculatePidTorque(deltaAngle, Time.deltaTime);
        transform.GetComponent<Rigidbody>().AddTorque(torque * axis);
    }
    
    //To be called every frame (in FixedUpdate)
    public void ApplyRotation(Vector3 currentDir, Vector3 targetDir)
    {
        float deltaAngle = Vector3.Angle(targetDir, currentDir) * Mathf.Deg2Rad/Mathf.PI;

        Vector3 axis = Vector3.Cross(currentDir, targetDir).normalized;

        if(Vector3.Angle(axis, prevAxis) > Vector3.Angle(-axis, prevAxis))
        {
            axis = -axis;
            deltaAngle = -deltaAngle;
        }

        prevAxis = axis;

        CalculatePidTorque(deltaAngle, Time.deltaTime);
        transform.Rotate(axis, torque / 125 * Time.deltaTime, Space.World);
    }


    private void CalculatePidTorque(float deltaAngle, float dt)
    {
        err[2] = err[1];
        err[1] = err[0];
        err[0] = deltaAngle;

        var a0 = Kp + Ki * dt;
        var a1 = -Kp;
        var a0d = Kd / dt;
        var a1d = -2 * Kd / dt;
        var a2d = Kd / dt;
        var alpha = dt / (2 * tau);


        d1 = d0;
        d0 = (a0d * err[0]) + (a1d * err[1]) + (a2d * err[2]);
        fd1 = fd0;
        fd0 = (alpha / (1 + alpha)) * (d0 + d1) + ((1 - alpha) / (1 + alpha)) * fd1;

        var P = a0 * err[0];
        var I = a1 * err[1];
        var D = fd0;
        torque = torque + P + I + D;

    }
}
