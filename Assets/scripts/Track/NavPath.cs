using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class NavPath
{
    readonly Mesh navMesh;

    PairwiseDistance distances;
    List<HashSet<int>> edges;

    Quadtree quadtree;

    public LayerMask rayCastLayerMask;

    public NavPath(Mesh navMesh)
    {
        if(navMesh.subMeshCount != 1 && navMesh.GetTopology(0) != MeshTopology.Lines)
        {
            Debug.LogError("Incompatible navMesh provided");
        }

        this.navMesh = navMesh;

        navMesh.RecalculateBounds();
        Bounds bounds = navMesh.bounds;
        bounds.Expand(100);

        Bounds2d bounds2d = new Bounds2d(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z);

        //quadtree
        quadtree = new Quadtree(bounds2d);
        int[] indices = navMesh.GetIndices(0);
        for (int i = 0; i < indices.Length; i+=2)
        {
            NavLine line = new NavLine(
                indices[i],
                indices[i + 1],
                navMesh.vertices[indices[i]],
                navMesh.vertices[indices[i + 1]]
                );
            quadtree.Insert(line);
        }

        //distances
        distances = new PairwiseDistance(navMesh.vertexCount);
        Vector3[] vertices = navMesh.vertices;
        for (int i = 0; i < indices.Length; i += 2)
        {
            float distance = Vector3.Distance(vertices[indices[i]], vertices[indices[i + 1]]);
            distances.AddEdge(indices[i], indices[i + 1], distance);
        }
        //distances.SetInitial();
        InitializeDistances();


        //edges
        edges = new List<HashSet<int>>(navMesh.vertexCount);
        for (int i = 0; i < navMesh.vertexCount; i++)
            edges.Add(new HashSet<int>());
        
        for (int i=0; i<indices.Length;i+=2)
        {
            edges[indices[i]].Add(indices[i+1]);
            edges[indices[i+1]].Add(indices[i]);
        }
    }

    public bool IsTargetNear(int startIdx, int endIdx)
    {
        if (edges[startIdx].Contains(endIdx) && distances[startIdx, endIdx] != Mathf.Infinity)
            return true;
        return false;
    }

    public int GetNextSparsePointIdx(int startIdx, int endIdx)
    {
        if (startIdx == endIdx)
            return startIdx;

        int nextIdx = -1;
        float currentDistance = Mathf.Infinity;

        foreach(int idx in edges[startIdx])
        {
            float dist = distances[idx, endIdx] + distances[startIdx, idx];
            if (dist < currentDistance)
            {
                currentDistance = dist;
                nextIdx = idx;
            }
        }
        return nextIdx;
    }

    public List<int> GetPath(int startIdx, int endIdx, int maxPoints)
    {
        List<int> path = new List<int>{ startIdx };
        int currentIdx = startIdx;
        while (currentIdx != endIdx && path.Count < maxPoints)
        {
            currentIdx = GetNextSparsePointIdx(currentIdx, endIdx);
            path.Add(currentIdx);
        }
        return path;
    }

    public float GetDistance(int startIdx, int endIdx)
    {
        return distances[startIdx, endIdx];
    }

    private void InitializeDistances()
    {
        List<ISpatialEntity2d> possibleIntersections = quadtree.GetElements();
        int layer = 1 << LayerMask.NameToLayer("Obstacle");
        foreach (NavLine line in possibleIntersections)
        {
            Ray r = new Ray(line.v1, line.v2 - line.v1);
            //TODO: add layer mask
            if (Physics.Raycast(
                ray: r,
                hitInfo: out RaycastHit hit,
                maxDistance: (line.v2 - line.v1).magnitude,
                layer,
                QueryTriggerInteraction.Ignore)
                )
            {
                distances.AddEdge(line.idx1, line.idx2, Mathf.Infinity);
            }
        }
        distances.CalculateShortestDistanceImmediate();
    }

    public void UpdateObstacle(Collider collider)
    {
        Bounds bounds = collider.bounds;
        Bounds2d bounds2d = new Bounds2d(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z);

        List<ISpatialEntity2d> possibleIntersections = quadtree.FindCollisions(bounds2d);
        int layer = 1 << LayerMask.NameToLayer("Obstacle");
        foreach (NavLine line in possibleIntersections)
        {
            Ray r = new Ray(line.v1, line.v2 - line.v1);
            //TODO: add layer mask
            if (Physics.Raycast(r, out _, (line.v2 - line.v1).magnitude, layer, QueryTriggerInteraction.Ignore))
            {
                distances.AddEdge(line.idx1, line.idx2, Mathf.Infinity);

            }
            else
            {
                distances.AddEdge(line.idx1, line.idx2, Vector3.Distance(line.v1, line.v2));
            }
        }
        distances.CalculateShortestDistance();
    }
}
