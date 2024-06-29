using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class NavPoint : ISpatialEntity2d
{
    public float x;
    public float y;
    public RectangleF bounds
    {
        get
        {
            return new RectangleF(x, y, 0, 0);
        }
    }

    public NavPoint(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
}