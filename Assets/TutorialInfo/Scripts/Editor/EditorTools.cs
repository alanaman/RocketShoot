using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


public class EditorTools : Editor
{
    [MenuItem("Assets/Utilities/Remesh Skin")]
    static void RemeshSkin()
    {
        var skinMesh = Selection.activeGameObject.transform.GetComponent<SkinnedMeshRenderer>();
        skinMesh.ResetBounds();

        foreach (var x in skinMesh.bones)
        {
            Debug.Log(x.name);
        }

        skinMesh.bones = skinMesh.rootBone.GetComponentsInChildren<Transform>();
        skinMesh.ResetBounds();

    }

    static Transform[] BuildBonesArray(Transform rootBone, Transform[] bones)
    {
        Transform[] boneList = rootBone.GetComponentsInChildren<Transform>();
        //ExtractBonesRecursively(rootBone, ref boneList);

        foreach (Transform bone in boneList)
        {
            Debug.Log("Bonelist:" + bone.name);
        }

        foreach (Transform bone in bones)
        {
            Debug.Log("Bone:" + bone.name);
        }

        List<Transform> Reorder = new List<Transform>();
        foreach (Transform bone in bones)
        {
            Debug.Log("Src:"+bone.name);
            foreach (Transform extractbone in boneList)
            {
                if (bone.name == extractbone.name)
                {
                    Debug.Log("Dst:" + extractbone.name);
                    Reorder.Add(extractbone);
                }
            }

        }

        return Reorder.ToArray();
    }

    static void ExtractBonesRecursively(Transform bone, ref List<Transform> boneList)
    {
        boneList.Add(bone);
        //boneList = bone.GetComponentsInChildren<Transform>();
        for (int i = 0; i < bone.childCount; i++)
        {
            ExtractBonesRecursively(bone.GetChild(i), ref boneList);
        }
    }

}





