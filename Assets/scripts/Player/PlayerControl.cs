using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerControl : MonoBehaviour
{
    public ShipStats stats;

    //[SerializeField] private float accelerationMax = 300;
    //[SerializeField] private float velocityMax = 3000;
    //[SerializeField] private float lateralDamp = 0.5f;
    //[SerializeField] private float inertia = 125;


    [SerializeField] private GameInput gameInput;

    private Vector3 dirTarget = new Vector3(0, 0, 1);

    PidController pidController;

    private void Start()
    {
        var rb = GetComponent<Rigidbody>();
        rb.inertiaTensor = new Vector3(0, stats.inertia, 0);
        pidController = new PidController(transform);

    }
    private void FixedUpdate()
    {
        //get input
        Vector2 dirInput2d = gameInput.GetDirectionVector();
        if (dirInput2d.magnitude > 1) { dirInput2d.Normalize(); }
        Vector3 dirInput = new Vector3(dirInput2d.x, 0, dirInput2d.y);
        float acc = gameInput.GetAccelaration() * stats.maxAcceleration1;


        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 dirCurrent = transform.rotation * new Vector3(0, 0, 1);
        dirCurrent.Normalize();

        //linear
        Vector3 force = stats.mass * acc * dirCurrent;
        //force -= force * Vector3.Dot(dir_current, rb.velocity) * (1 - Mathf.Exp(-rb.velocity.magnitude * LINEAR_DAMP));

        Debug.DrawRay(transform.position, dirInput * 50, Color.green);
        Debug.DrawRay(transform.position, dirCurrent * 100, Color.blue);
        if (acc > 0.01)
        {
            Vector3 longitudinal_comp = dirCurrent * Vector3.Dot(dirCurrent, rb.velocity);
            Vector3 lateral_velocity = rb.velocity - longitudinal_comp;
            force -= stats.lateralDamping * lateral_velocity;
        }

        //damp constant k = maxforce/maxspeed
        force -= stats.dampingConstant *  rb.velocity;

        rb.AddForce(force);

        //rotational
        if (dirInput.magnitude > 0.1)
            dirTarget = dirInput.normalized;

        float delta_angle = Vector3.SignedAngle(dirTarget, dirCurrent, -Vector3.up) / Mathf.PI;

        var sign = Mathf.Sign(Vector3.Dot(Vector3.Cross(dirCurrent, dirTarget), Vector3.up));
        delta_angle = Mathf.Acos(Vector3.Dot(dirTarget, dirCurrent))*sign/Mathf.PI;

        pidController.ApplyTorque(dirCurrent, dirTarget);
    }
}
