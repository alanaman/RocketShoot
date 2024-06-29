using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UIElements;

public class NavTriangle : ISpatialEntity2d
{
    public int startIdx;
    public int closestSparsePointIdx;
    Vector3 v1;
    Vector3 v2;
    Vector3 v3;

    public Bounds2d bounds
    {
        get
        {
            float minX = Mathf.Min(Mathf.Min(v1.x, v2.x), v3.x);
            float maxX = Mathf.Max(Mathf.Max(v1.x, v2.x), v3.x);
            float minZ = Mathf.Min(Mathf.Min(v1.z, v2.z), v3.z);
            float maxZ = Mathf.Max(Mathf.Max(v1.z, v2.z), v3.z);
            return new Bounds2d(minX, minZ, maxX - minX, maxZ - minZ);
        }
    }

    public NavTriangle(int startIdx, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        this.startIdx = startIdx;
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
    }

    public bool ContainsPoint(float x, float y)
    {
        float x1 = v1.x;
        float y1 = v1.z;
        float x2 = v2.x;
        float y2 = v2.z;
        float x3 = v3.x;
        float y3 = v3.z;

        // Compute vectors
        float v0x = x3 - x1;
        float v0y = y3 - y1;
        float v1x = x2 - x1;
        float v1y = y2 - y1;
        float v2x = x - x1;
        float v2y = y - y1;

        // Compute dot products
        float dot00 = v0x * v0x + v0y * v0y;
        float dot01 = v0x * v1x + v0y * v1y;
        float dot02 = v0x * v2x + v0y * v2y;
        float dot11 = v1x * v1x + v1y * v1y;
        float dot12 = v1x * v2x + v1y * v2y;

        // Compute barycentric coordinates
        float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        // Check if point is in triangle
        return (u >= -0.01) && (v >= -0.01) && (u + v <= 1.01);
    }

    float PointLinedistance(Vector2 P, Vector2 A, Vector2 B)
    {
        Vector2 AB = B - A;
        Vector2 AP = P - A;
        float t = Vector2.Dot(AP, AB) / Vector2.Dot(AB, AB);
        t = Mathf.Clamp(t, 0, 1);
        Vector2 projection = A + t * AB;
        return (P - projection).magnitude;
    }
    public float GetDistanceFromPoint(float x, float y)
    {
        if (ContainsPoint(x, y))
        {
            return 0;
        }
        Vector2 p = new Vector2(x, y);

        Vector2 a = new Vector2(v1.x, v1.z);
        Vector2 b = new Vector2(v2.x, v2.z);
        Vector2 c = new Vector2(v3.x, v3.z);

        float minDist = PointLinedistance(p, a, b);
        minDist = Mathf.Min(minDist, PointLinedistance(p, b, c));
        minDist = Mathf.Min(minDist, PointLinedistance(p, c, a));

        return minDist;
    }
}
