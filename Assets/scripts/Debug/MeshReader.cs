using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class MeshReader : MonoBehaviour
{
    [SerializeField] Mesh mesh;

    [ContextMenu("PrintMesh")]
    void PrintMesh()
    {
        if (mesh == null)
        {
            mesh = GetComponent<MeshFilter>().mesh;
        }
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3[] normals = mesh.normals;
        Color[] color = mesh.colors;

        for (int i = 0; i < vertices.Length; i++)
        {
            Debug.Log("Vertex: " + vertices[i] + " Normal: " + normals[i]);
        }

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Debug.Log("Triangle: " + vertices[triangles[i]] + " " + vertices[triangles[i + 1]] + " " + vertices[triangles[i + 2]]);
        }

        for(int i = 0; i < color.Length; i++)
        {
            Debug.Log("Color: " + color[i]);
        }
    }
}
