using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class TrailLineMeshGenerator : MonoBehaviour
{

    // CUrve data
    Vector3[] vertices;
    Vector3[] velocities;
    float[] cumulativeLength;


    // CUrve Params
    [SerializeField] uint maxParticles = 16;
    [SerializeField] uint resolution = 8;
    [SerializeField] float interval = 1;
    [SerializeField] float speed = 1;
    [SerializeField] AnimationCurve thicknessCurve = AnimationCurve.Linear(0,0,1,1);


    int currIdx = 0;
    float timer = 0;
    TransformData lastFrameTransform;

    // Generated Mesh Data
    Vector3[] meshVertices;
    Vector3[] meshNormals;
    Vector2[] meshUvs;
    int[] meshIndices;

    private void Awake()
    {
        currIdx = 0;
        vertices = new Vector3[(int)maxParticles];
        velocities = new Vector3[(int)maxParticles];
        cumulativeLength = new float[(int)maxParticles];

        Mesh newMesh = new Mesh();
        GetComponent<MeshFilter>().sharedMesh = newMesh;
        newMesh.MarkDynamic();
        newMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10);


        meshVertices = new Vector3[((int)maxParticles + 1) * (int)(resolution + 1)];
        meshNormals = new Vector3[((int)maxParticles + 1) * (int)(resolution + 1)];
        meshUvs = new Vector2[((int)maxParticles + 1) * (int)(resolution + 1)];
        meshIndices = new int[((int)maxParticles) * (int)(resolution + 1) * 6];

        //the point for the first edge ring doesnt exist in curve data
        for(int j = 0; j<= resolution;j++)
        {
            float t = (float)j / (float)resolution;
            Vector3 offset = Mathf.Sin(2 * t * Mathf.PI) * Vector3.right + Mathf.Cos(2 * t * Mathf.PI) * Vector3.up;
            meshVertices[j] = offset;
            meshUvs[j] = new Vector2((float)j / (float)resolution, 0);
        }

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

        //Generate Mesh
        Vector3 prevTangent = Vector3.forward;
        for (int i = 1; i <= maxParticles; i++)
        {

            int idx = (currIdx + (int)maxParticles - i) % (int)maxParticles;
            Vector3 tangent = vertices[idx] - vertices[(idx+1)%maxParticles];
            
            if(i==1)
            {
                tangent = Vector3.forward;
            }

            //This is to minimize flipping between adjacent edge rings
            if(Vector3.Dot(tangent, prevTangent) < 0)
            {
                tangent = -tangent;
            }
            prevTangent = tangent;

            Vector3 right;
            right = Vector3.Cross(Vector3.up, tangent).normalized;


            for (int j = 0; j <= resolution; j++)
            {
                float profileT = (float)j / (float)resolution;
                float curveT = this.cumulativeLength[idx] / ((float)maxParticles * interval * speed);
                Vector3 offset = Mathf.Sin(2 * profileT * Mathf.PI) * right + Mathf.Cos(2 * profileT * Mathf.PI) * Vector3.up;
                meshVertices[i * (int)(resolution + 1) + j] = vertices[idx] + offset * thicknessCurve.Evaluate(curveT);
                meshUvs[i * (int)(resolution + 1) + j] = new Vector2(profileT, curveT);
            }
            for (int j = 0; j < resolution; j++)
            {
                int v1 = i * (int)(resolution + 1) + j;
                int v2 = i * (int)(resolution + 1) + (j+1);
                int v3 = (i - 1) * (int)(resolution + 1) + j;
                int v4 = (i - 1) * (int)(resolution + 1) + (j + 1);

                int baseIdx = ((i - 1) * (int)resolution + j) * 6;
                meshIndices[baseIdx] = v1;
                meshIndices[baseIdx + 1] = v2;
                meshIndices[baseIdx + 2] = v3;
                meshIndices[baseIdx + 3] = v3;
                meshIndices[baseIdx + 4] = v2;
                meshIndices[baseIdx + 5] = v4;
            }
        }

        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        mesh.vertices = meshVertices;
        mesh.uv = meshUvs;
        mesh.SetIndices(meshIndices, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();

        lastFrameTransform = new TransformData(transform);
    }
}
