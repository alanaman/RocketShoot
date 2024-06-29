using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackCorner : MonoBehaviour
{
    [SerializeField] private Transform root;
    [SerializeField] private Transform leftWing;
    [SerializeField] private Transform rightWing;
    [SerializeField] private Transform leftEndpoint;
    [SerializeField] private Transform rightEndpoint;
    public void Align(Vector3 left, Vector3 center, Vector3 right, Vector3 normal)
    {
        var v1 = (left - center).normalized;
        var v2 = (right - center).normalized;
        //Debug.Log(left.ToString()+ center.ToString()+ right.ToString());
        //Debug.Log(v1.ToString()+v2.ToString());

        var cosTheta = Vector3.Dot(v1, normal);
        var theta = Mathf.PI - Mathf.Acos(cosTheta);
        Debug.Log(theta.ToString());

        var dist = 0.4f*Mathf.Sin(theta) - (0.5f-0.4f*Mathf.Cos(theta))/Mathf.Tan(theta);
        root.localPosition += normal * dist;

        //root.rotation *= Quaternion.FromToRotation(Vector3.forward, normal);
        root.rotation *= Quaternion.LookRotation(normal, Vector3.up);

        if (Vector3.Angle(root.right, v2) <= 90)
        {
            rightWing.rotation = Quaternion.FromToRotation(rightWing.rotation * Vector3.up, v2) * rightWing.rotation;
            leftWing.rotation = Quaternion.FromToRotation(leftWing.rotation * Vector3.up, v1) * leftWing.rotation;
        }
        else
        {
            rightWing.rotation = Quaternion.FromToRotation(rightWing.rotation * Vector3.up, v1) * rightWing.rotation;
            leftWing.rotation = Quaternion.FromToRotation(leftWing.rotation * Vector3.up, v2) * leftWing.rotation;
        }

    }

    public Vector3 GetRightEndpoint()
    {
        return rightEndpoint.position;
    }
    public Vector3 GetLeftEndpoint()
    {
        return leftEndpoint.position;
    }
}
