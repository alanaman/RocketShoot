using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TrailRig : MonoBehaviour
{
    //[SerializeField] Transform root;

    [SerializeField] Transform[] bones;

    // CUrve data
    Vector3[] vertices;
    Vector3[] velocities;
    float[] cumulativeLength;


    // Curve Params
    [SerializeField] uint maxParticles = 16;
    [SerializeField] uint resolution = 8;
    [SerializeField] float interval = 1;
    [SerializeField] float speed = 1;
    [SerializeField] AnimationCurve thicknessCurve = AnimationCurve.Linear(0,0,1,1);


    int currIdx = 0;
    float timer = 0;
    TransformData lastFrameTransform;
    float boneDistance = 1;
    private void Awake()
    {
        currIdx = 0;
        vertices = new Vector3[(int)maxParticles];
        velocities = new Vector3[(int)maxParticles];
        cumulativeLength = new float[(int)maxParticles];
    }

    private void OnValidate()
    {
        if (resolution < 3)
            resolution = 3;
        if(resolution > 32)
            resolution = 32;

        if (maxParticles < 3)
            maxParticles = 3;
        if (maxParticles > 32)
            maxParticles = 32;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        //Spawn New curve point
        if (timer > interval)
        {
            timer = 0;
            vertices[currIdx] = Vector3.zero;
            velocities[currIdx] = (transform.forward * speed);
            velocities[currIdx].Scale(transform.lossyScale);
            
            Rigidbody rb = GetComponentInParent<Rigidbody>();
            if (rb!=null)
            {
                velocities[currIdx] += rb.velocity;
            }

            this.cumulativeLength[currIdx] = 0;

            currIdx = (currIdx + 1) % (int)maxParticles;
        }

        //Update curve point positions
        if (lastFrameTransform == null)
            lastFrameTransform = new TransformData(transform);

        for (int i = 0; i < maxParticles; i++)
        {
            Vector3 positionWS = lastFrameTransform.TransformPoint(vertices[i]);
            positionWS += (velocities[i] * Time.deltaTime);
            vertices[i] = transform.InverseTransformPoint(positionWS);
        }

        // calculate cumulative length
        float lengthSum = 0;
        for (int i = 1; i <= maxParticles; i++)
        {
            int idx = (currIdx + (int)maxParticles - i) % (int)maxParticles;
            lengthSum += (vertices[idx] - vertices[(idx + 1) % (maxParticles)]).magnitude;
            if (i == 1)
                lengthSum = vertices[idx].magnitude;
            cumulativeLength[idx] = lengthSum;
        }


        List<Vector3> sampledPoints = new List<Vector3>();
        sampledPoints.Add(Vector3.zero);
        float distance = (cumulativeLength[currIdx % maxParticles])/(2*bones.Length);
        boneDistance = Mathf.Lerp(boneDistance, distance, 0.01f);

        float accumulatedDistance = 0;
        for (int i = 1; i <= maxParticles; i++)
        {
            int idx = (currIdx + (int)maxParticles - i) % (int)maxParticles;
            Vector3 p1 = vertices[(idx + 1) % (int)maxParticles];
            Vector3 p2 = vertices[idx];
            float segmentLength = (p1 - p2).magnitude;
            if (i == 1)
            {
                segmentLength = vertices[idx].magnitude;
                p1 = Vector3.zero;
            }
            while (accumulatedDistance + segmentLength >= boneDistance)
            {
                if (sampledPoints.Count == bones.Length)
                    break;
                float remainingDistance = boneDistance - accumulatedDistance;
                float t = remainingDistance / segmentLength;
                if(segmentLength == 0)
                {
                    t = 0;
                }
                Vector3 newPoint = Vector3.Lerp(p1, p2, t);
                sampledPoints.Add(newPoint);
                segmentLength -= remainingDistance;
                accumulatedDistance = 0.0f;
            }
            accumulatedDistance += segmentLength;

        }
        for(int i= 0; i < bones.Length; i++)
        { 
            bones[i].position = Vector3.Lerp(bones[i].position, transform.TransformPoint(sampledPoints[i]), 0.1f);
        }
        for (int i = 0; i < bones.Length-1; i++)
        {
            bones[i].LookAt(bones[i + 1]);
            bones[i].Rotate(90, 0, 0);
        }

            sampledPoints = null;

        lastFrameTransform = new TransformData(transform);
    }
}
