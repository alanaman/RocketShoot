using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LineMeshImporter : Editor
{
    private static string _progressTitle = "Extracting Meshes";
    private static string _sourceExtension = ".linemesh";
    //private static string _targetExtension = ".asset";


    [MenuItem("Assets/Utilities/Extract Line Meshes", validate = true)]
    private static bool ExtractMeshesMenuItemValidate()
    {
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            if (!AssetDatabase.GetAssetPath(Selection.objects[i]).EndsWith(_sourceExtension))
                return false;
        }
        return true;
    }

    [MenuItem("Assets/Utilities/Extract Line Meshes")]
    private static void ExtractMeshesMenuItem()
    {
        EditorUtility.DisplayProgressBar(_progressTitle, "", 0);
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            EditorUtility.DisplayProgressBar(_progressTitle, Selection.objects[i].name, (float)i / (Selection.objects.Length - 1));
            ExtractMeshes(Selection.objects[i]);
        }
        EditorUtility.ClearProgressBar();
    }

    private static void ExtractMeshes(Object selectedObject)
    {
        //Create Folder Hierarchy
        string selectedObjectPath = AssetDatabase.GetAssetPath(selectedObject);
        //string parentfolderPath = selectedObjectPath.Substring(0, selectedObjectPath.Length - (selectedObject.name.Length + 5));
        //string objectFolderName = selectedObject.name;
        //string objectFolderPath = parentfolderPath + "/" + objectFolderName;
        //string meshFolderName = "Meshes";
        //string meshFolderPath = objectFolderPath + "/" + meshFolderName;

        /*        if (!AssetDatabase.IsValidFolder(objectFolderPath))
                {
                    AssetDatabase.CreateFolder(parentfolderPath, objectFolderName);

                    if (!AssetDatabase.IsValidFolder(meshFolderPath))
                    {
                        AssetDatabase.CreateFolder(objectFolderPath, meshFolderName);
                    }
                }*/

        //Create Meshes
        Object[] objects = AssetDatabase.LoadAllAssetsAtPath(selectedObjectPath);

        Debug.Log("menu item clicked");
        Debug.Log(selectedObjectPath);

        //for (int i = 0; i < objects.Length; i++)
        //{
        //    EditorUtility.DisplayProgressBar(_progressTitle, selectedObject.name + " : " + objects[i].name, (float)i / (objects.Length - 1));

            CreateMesh(selectedObjectPath);
        //}

        //Cleanup
        //AssetDatabase.MoveAsset(selectedObjectPath, objectFolderPath + "/" + selectedObject.name + _sourceExtension);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


    static void CreateMesh(string lineMeshPath)
    {
        Debug.Log("create mesh");

        //read text file
        string loadedText = System.IO.File.ReadAllText(lineMeshPath);

        string[] lines = loadedText.Split('\n');
        int num_verts = int.Parse(lines[0]);
        int num_edges = int.Parse(lines[num_verts + 1]);
        List<Vector3> vertices = new List<Vector3>();
        List<int> edges = new List<int>();

        for (int i = 0; i < num_verts; i++)
        {
            var coords = lines[i + 1].Split(',');
            //position
            Vector3 vertex = new Vector3(
                float.Parse(coords[0]),
                float.Parse(coords[1]),
                float.Parse(coords[2])
                );
            vertices.Add(vertex);
        }

        for (int i = 0; i < num_edges; i++)
        {
            var indices = lines[i + 2 + num_verts].Split(',');
            edges.Add(int.Parse(indices[0]));
            edges.Add(int.Parse(indices[1]));
        }
        var mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.SetIndices(edges.ToArray(), MeshTopology.Lines, 0);
        Debug.Log("mesh attached");
        string filePath =
            EditorUtility.SaveFilePanelInProject("Save Procedural Mesh", "Procedural Mesh", "asset", "");
        AssetDatabase.CreateAsset(mesh, filePath);
    }
}
