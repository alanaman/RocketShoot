using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] private float accelerationMax = 300;
    [SerializeField] private float velocityMax = 3000;
    [SerializeField] private float lateralDamp = 0.5f;
    [SerializeField] private float inertia = 125;


    [SerializeField] private GameInput gameInput;

    private Vector3 dirTarget = new Vector3(0, 0, 1);

    PidController pidController;

    private void Start()
    {
        var rb = GetComponent<Rigidbody>();
        rb.inertiaTensor = new Vector3(0, inertia, 0);
        pidController = new PidController(transform);
    }


    private void FixedUpdate()
    {
        //get input
        Vector2 dir_input2d = gameInput.getDirectionVector();
        if (dir_input2d.magnitude > 1) { dir_input2d.Normalize(); }
        Vector3 dir_input = new Vector3(dir_input2d.x, 0, dir_input2d.y);
        float acc = gameInput.getAccelaration() * accelerationMax;


        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 dirCurrent = transform.rotation * new Vector3(0, 0, 1);
        dirCurrent.Normalize();

        //linear
        Vector3 force = acc * dirCurrent;
        //force -= force * Vector3.Dot(dir_current, rb.velocity) * (1 - Mathf.Exp(-rb.velocity.magnitude * LINEAR_DAMP));

        if (acc > 0.01)
        {
            Vector3 longitudinal_comp = dirCurrent * Vector3.Dot(dirCurrent, rb.velocity);
            Vector3 lateral_velocity = rb.velocity - longitudinal_comp;
            force -= lateralDamp * lateral_velocity;
        }

        //damp constant k = maxforce/maxspeed
        force -= (accelerationMax/velocityMax) *  rb.velocity;

        rb.AddForce(force);

        //rotational
        if (dir_input.magnitude > 0.1)
            dirTarget = dir_input.normalized;

        float delta_angle = Vector3.SignedAngle(dirTarget, dirCurrent, -Vector3.up) / Mathf.PI;

        var sign = Mathf.Sign(Vector3.Dot(Vector3.Cross(dirCurrent, dirTarget), Vector3.up));
        delta_angle = Mathf.Acos(Vector3.Dot(dirTarget, dirCurrent))*sign/Mathf.PI;

        pidController.ApplyTorque(dirCurrent, dirTarget);
    }
}
