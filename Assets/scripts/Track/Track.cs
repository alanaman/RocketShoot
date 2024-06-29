using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using UnityEngine.Windows;

public class Track : MonoBehaviour
{

    [SerializeField]
    TextAsset boundary;

    [SerializeField] private GameObject corner;
    [SerializeField] private GameObject edge;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("CreateMesh")]
    void CreateMesh()
    {
        //read text file
        string[] lines = boundary.text.Split('\n');
        int num_verts = int.Parse(lines[0]);
        int num_edges = int.Parse(lines[num_verts+1]);
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> edges = new List<int>();

        for(int i = 0; i< num_verts; i++)
        {
            var coords = lines[i+1].Split(',');
            //position
            Vector3 vertex = new Vector3(
                float.Parse(coords[0]),
                float.Parse(coords[1]),
                float.Parse(coords[2])
                );
            vertices.Add(vertex);
            //normal
            Vector3 normal = new Vector3(
                float.Parse(coords[3]),
                float.Parse(coords[4]),
                float.Parse(coords[5])
                );
            normals.Add(normal);
        }

        for(int i=0;i< num_edges; i++)
        {
            var indices = lines[i+2+num_verts].Split(',');
            edges.Add(int.Parse(indices[0]));
            edges.Add(int.Parse(indices[1]));
        }
        var mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.SetIndices(edges.ToArray(), MeshTopology.Lines, 0);
        Debug.Log("mesh attached");
        string filePath =
            EditorUtility.SaveFilePanelInProject("Save Procedural Mesh", "Procedural Mesh", "asset", "");
        AssetDatabase.CreateAsset(mesh, filePath);
    }

    [ContextMenu("BuildTrack")]
    void BuildTrack()
    {
        while(transform.childCount!=0)
        {
            var child = transform.GetChild(0);
            DestroyImmediate(child.gameObject);
        }

        var mesh = GetComponent<MeshFilter>().sharedMesh;
        var verts = mesh.vertices;
        var norms = mesh.normals;
        var vert_edges = new List<List<int>>();

        var indices = mesh.GetIndices(0);
        Debug.Log($"[{string.Join(",", indices)}]");

        for (int i = 0; i < verts.Length; i++)
            vert_edges.Add(new List<int>());

        for (int i = 0; i < indices.Length; i+=2)
        {
            vert_edges[indices[i]].Add(indices[i + 1]);
            vert_edges[indices[i+1]].Add(indices[i]);
        }
        /*for (int i = 0; i < indices.Length; i+=2)
        {
            vert_edges[indices[i+1]].Add(indices[i]);
        }*/

        /*for (int i = 0; i < verts.Length; i++)
            Debug.Log($"[{string.Join(",", vert_edges[i].ToList())}]");

        Debug.Log($"[{string.Join(",", verts)}]");
        */

        //corners
        List<TrackCorner> corners = new List<TrackCorner>();
        for (int i =0;i< verts.Length;i++)
        {
            GameObject cornerInst = Instantiate(corner, verts[i], Quaternion.identity);
            cornerInst.transform.parent = transform;
            List<int> otherIdx = vert_edges[i].ToList();
            int i1 = otherIdx[0];
            int i2 = otherIdx[1];
            Assert.AreEqual(2,otherIdx.Count);

            TrackCorner trackCorner = cornerInst.GetComponent<TrackCorner>();
            trackCorner.Align(verts[i1], verts[i], verts[i2], norms[i]);   
            corners.Add(trackCorner);
        }

        //edges
        for (int i = 0; i < indices.Length; i += 2)
        {
            var edgeInst = Instantiate(edge, Vector3.zero, Quaternion.identity);
            edgeInst.transform.parent = transform;
            edgeInst.GetComponent<TrackEdge>().Align(corners[indices[i]], corners[indices[i + 1]]);
        }
    }

/*    [ContextMenu("AlignCorner")]
    void AlignCorner()
    {
        for(int i=0;i<transform.childCount;i++)
        {
            var child = transform.GetChild(i);
            var verts = GetComponent<MeshFilter>().sharedMesh.vertices;

            child.GetComponent<TrackCorner>().Align(verts[(verts.Length + i - 1)%verts.Length], verts[i], verts[(i + 1)%verts.Length]);
        }
    }*/

    private void OnValidate()
    {
        //Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
        //if (mesh != null)
        //    DestroyImmediate(mesh, true);


        //CreateMesh();
    }
}
