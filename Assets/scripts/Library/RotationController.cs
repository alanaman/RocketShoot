using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationController
{
    Transform transform;

    public float angularSpeed = 100.0f;
    public Vector3 preferredAxis = Vector3.up;

    public RotationController(Transform transform)
    {
        this.transform = transform;
    }

    //To be called every frame (in FixedUpdate)
    public void ApplyRotation(Vector3 currentDir, Vector3 targetDir)
    {
        float deltaAngle = Vector3.Angle(targetDir, currentDir);

        Vector3 axis = Vector3.Cross(currentDir, targetDir).normalized;
        if (deltaAngle > 160)
            axis = Vector3.up;

        var rot = Quaternion.AngleAxis(Time.deltaTime * angularSpeed, axis);
        transform.Rotate(axis, Time.deltaTime * angularSpeed, Space.World);

    }

}
