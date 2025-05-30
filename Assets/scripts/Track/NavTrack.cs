using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;

public class NavTrack : MonoBehaviour
{
    [SerializeField] GameObject plane;
    [SerializeField] Mesh navPathMesh;

    [SerializeField] TextAsset closestSparsePoint;

    NavPath navPath;
    Quadtree tree;

    [SerializeField] LayerMask rayCastLayerMask;

    private void Awake()
    {
        ConstructTree();
        navPath = new NavPath(navPathMesh);
        navPath.rayCastLayerMask = rayCastLayerMask;
    }

    public Vector3 GetNextTargetPosition(in Vector3 seeker, in Vector3 target)
    {
        int idxSeeker = GetSparsePoint(seeker);
        int idxTarget = GetSparsePoint(target);

        int nextIdx = navPath.GetNextSparsePointIdx(idxSeeker, idxTarget);

        return transform.TransformPoint(navPathMesh.vertices[nextIdx]);        
    }

    public List<Vector3> GetPath(in Vector3 seeker, in Vector3 target, int maxPoints)
    {
        int idxSeeker = GetSparsePoint(seeker);
        int idxTarget = GetSparsePoint(target);

        List<int> pathIdx = navPath.GetPath(idxSeeker, idxTarget, maxPoints);
        List<Vector3> path = new List<Vector3>(pathIdx.Count);
        foreach (int idx in pathIdx)
        {
            path.Add(transform.TransformPoint(navPathMesh.vertices[idx]));
        }
        return path;
    }

    public bool IsTargetNear(in Vector3 seeker, in Vector3 target)
    {
        int idxSeeker = GetSparsePoint(seeker);
        int idxTarget = GetSparsePoint(target);
        if (idxSeeker == idxTarget)
            return true;

        return navPath.IsTargetNear(idxSeeker, idxTarget);
    }

    public float GetLossyDistance(in Vector3 seeker, in Vector3 target)
    {
        int idxSeeker = GetSparsePoint(seeker);
        int idxTarget = GetSparsePoint(target);

        return navPath.GetDistance(idxSeeker, idxTarget);
    }

    private int GetSparsePoint(in Vector3 target)
    {
        NavTriangle tri = GetTriangleContaining(target);
        return tri.closestSparsePointIdx;
    }

    private NavTriangle GetTriangleContaining(in Vector3 targetPos)
    {
        NavPoint point = new NavPoint(targetPos.x, targetPos.z);

        if (tree == null)
            ConstructTree();

        List<ISpatialEntity2d> tris = tree.FindCollisions(point.bounds);

        foreach (NavTriangle tri in tris)
        {
            if (tri.ContainsPoint(point.x, point.y))
            {
                return tri;
            }
        }
        Debug.LogWarning("target is not in any triangle. Maybe it's outside the track?");

        return null;
    }


    [ContextMenu("ConstructTree")]
    public void ConstructTree()
    {
        if(!TryGetComponent(out MeshFilter meshFilter))
        {
            Debug.LogError("No MeshFilter found on object");
            return;
        }
        if(navPathMesh == null)
        {
            Debug.LogError("No navPath found on object");
            return;
        }

        Mesh trackMesh = meshFilter.sharedMesh;

        trackMesh.RecalculateBounds();
        Bounds bounds = trackMesh.bounds;
        bounds.Expand(10);

        Bounds2d bounds2d = new Bounds2d(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z);

        tree = new Quadtree(bounds2d);

        Vector3[] tVertices = trackMesh.vertices;
        int[] tTriangles = trackMesh.triangles;

        for (int i = 0; i < tTriangles.Length; i += 3)
        {
            NavTriangle tri = new NavTriangle(
                i,
                tVertices[tTriangles[i]],
                tVertices[tTriangles[i + 1]],
                tVertices[tTriangles[i + 2]]
                );

            tree.Insert(tri);
        }

        Vector3[] pVertices = navPathMesh.vertices;
        List<ISpatialEntity2d> navTriangles = tree.GetElements();


        string[] lines = closestSparsePoint.text.Split('\n');
        foreach (NavTriangle navTriangle in navTriangles)
        {
            int idx = navTriangle.startIdx / 3;
            navTriangle.closestSparsePointIdx = int.Parse(lines[idx]);
        }

    }

    private void CreateViz(Quadtree tree, float height)
    {
        var inst = Instantiate(plane, transform);
        inst.transform.localScale = new Vector3(tree.bounds.Width, tree.bounds.Height, 1);
        var pos = inst.transform.position;
        pos.y = height;
        pos.x = tree.bounds.Center.x;
        pos.z = tree.bounds.Center.x;
        inst.transform.position = pos;

        inst.GetComponent<MeshRenderer>().material.color = new UnityEngine.Color(Random.value, Random.value, Random.value);

        foreach (Quadtree child in tree.GetChildren())
        {
            CreateViz(child, height+100);
        }
        
    }

#if UNITY_EDITOR
    [ContextMenu("AddVertexColors")]
    void AddVertexColors()
    {
        if (tree == null)
            ConstructTree();

        if (navPathMesh == null)
        {
            Debug.LogError("navPath not set");
            return;
        }

        Mesh trackMesh = GetComponent<MeshFilter>().sharedMesh;

        Vector3[] tVertices = trackMesh.vertices;
        int[] tTriangles = trackMesh.triangles;
        Vector3[] pVertices = navPathMesh.vertices;


        UnityEngine.Color[] colors = new UnityEngine.Color[tVertices.Length];
        var randColors = new UnityEngine.Color[pVertices.Length];
        for (int i = 0; i < pVertices.Length; i++)
        {
            randColors[i] = new UnityEngine.Color(Random.value, Random.value, Random.value);
        }
        List<ISpatialEntity2d> navTriangles = tree.GetElements();

        foreach (NavTriangle tri in navTriangles)
        {
            int i = tri.startIdx;
            colors[tTriangles[i]] = randColors[tri.closestSparsePointIdx];
            colors[tTriangles[i + 1]] = randColors[tri.closestSparsePointIdx];
            colors[tTriangles[i + 2]] = randColors[tri.closestSparsePointIdx];
        }

        trackMesh.colors = colors;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif
    public void UpdateObstacle(Collider collider)
    {
        navPath.UpdateObstacle(collider);
    }
}
