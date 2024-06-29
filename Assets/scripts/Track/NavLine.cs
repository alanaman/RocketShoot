using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UIElements;

public class NavLine : ISpatialEntity2d
{
    //first index of edge
    public readonly int idx1, idx2;
    
    public readonly Vector3 v1;
    public readonly Vector3 v2;

    public Bounds2d bounds
    {
        get
        {
            float minX = Mathf.Min(v1.x, v2.x);
            float maxX = Mathf.Max(v1.x, v2.x);
            float minZ = Mathf.Min(v1.z, v2.z);
            float maxZ = Mathf.Max(v1.z, v2.z);
            return new Bounds2d(minX, minZ, maxX - minX, maxZ - minZ);
        }
    }

    public NavLine(int idx1, int idx2, Vector3 v1, Vector3 v2)
    {
        this.idx1 = idx1;
        this.idx2 = idx2;
        this.v1 = v1;
        this.v2 = v2;
    }
}
