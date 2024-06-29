using System.Drawing;
using UnityEngine;

class Rectangle2d : ISpatialEntity2d
{
    readonly RectangleF _bounds;

    public RectangleF bounds
    {
        get
        {
            return _bounds;
        }
    }

    public Rectangle2d(Bounds bounds)
    {
        _bounds.X = bounds.min.x;
        _bounds.Y = bounds.min.z;
        _bounds.Width = bounds.size.x;
        _bounds.Height = bounds.size.z;
    }
}