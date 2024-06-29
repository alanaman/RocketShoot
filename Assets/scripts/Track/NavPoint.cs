using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class NavPoint : ISpatialEntity2d
{
    public float x;
    public float y;
    public Bounds2d bounds
    {
        get
        {
            return new Bounds2d(x, y, 0, 0);
        }
    }

    public NavPoint(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
}