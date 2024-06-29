using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//TODO:check correctness
public class TransformData
{

    private Vector3 position;
    private Quaternion rotation;
    private Vector3 scale;

    public TransformData(Transform transform)
    {
        position = transform.position;
        rotation = transform.rotation;
        scale = transform.localScale;
    }

    public TransformData()
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;
        scale = Vector3.one;
    }
    
    public Vector3 Position
    {
        get => position;
        set => position = value;
    }

    public Quaternion Rotation
    {
        get => rotation;
        set => rotation = value;
    }

    public Vector3 Scale
    {
        get => scale;
        set => scale = value;
    }

    public Matrix4x4 LocalToWorldMatrix
    {
        get => Matrix4x4.Translate(position) * Matrix4x4.Rotate(rotation) * Matrix4x4.Scale(scale);
    }

    public Matrix4x4 WorldToLocalMatrix
    {
        get => Matrix4x4.Inverse(LocalToWorldMatrix);
    }

    public Vector3 TransformPoint(Vector3 point)
    {
        return LocalToWorldMatrix.MultiplyPoint(point);
    }

    public Vector3 InverseTransformPoint(Vector3 point)
    {
        return WorldToLocalMatrix.MultiplyPoint(point);
    }

    public Vector3 TransformDirection(Vector3 direction)
    {
        return Matrix4x4.Rotate(rotation).MultiplyVector(direction);
    }

    public Vector3 InverseTransformDirection(Vector3 direction)
    {
        return Matrix4x4.Inverse(Matrix4x4.Rotate(rotation)).MultiplyVector(direction);
    }

    public Vector3 TransformVector(Vector3 vector)
    {
        return Matrix4x4.Scale(scale)*Matrix4x4.Rotate(rotation).MultiplyVector(vector);
    }

    public Vector3 InverseTransformVector(Vector3 vector)
    {
        return  Matrix4x4.Inverse(Matrix4x4.Scale(scale)*Matrix4x4.Rotate(rotation)).MultiplyVector(vector);
    }

    public void Translate(Vector3 translation)
    {
        position += translation;
    }
    public void Rotate(Quaternion deltaRotation)
    {
        rotation = deltaRotation * rotation;
    }

}
