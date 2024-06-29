using UnityEngine;

public struct Bounds2d
{
    private Vector2 _location;
    private Vector2 _size;

    public Bounds2d(Vector2 location, Vector2 size)
    {
        _location = location;
        _size = size;
    }
    public Bounds2d(float x, float y, float width, float height)
    {
        _location.x = x;
        _location.y = y;
        _size.x = width;
        _size.y = height;
    }
    public Vector2 Size { get => _size; set => _size = value; }
    public Vector2 Location { get => _location; set => _location = value; }
    public float Width { get => _size.x; set => _size.x = value; }
    public float Height { get => _size.y; set => _size.y = value; }
    public float X { get => _location.x; set => _location.x = value; }
    public float Y { get => _location.y; set => _location.y = value; }
    public float Left { get => _location.x; }
    public float Right { get => _location.x + _size.x; }
    public float Bottom { get => _location.y; }
    public float Top { get => _location.y + _size.y; }
    public Vector2 Center { get => _location + _size / 2; }


    public bool Contains(Vector2 point)
    {
        return point.x >= Left && point.x <= Right && point.y >= Bottom && point.y <= Top;
    }
    public bool Contains(float x, float y)
    {
        return x >= Left && x <= Right && y >= Bottom && y <= Top;
    }

    public bool Contains(Bounds2d bounds)
    {
        return bounds.Left >= Left && bounds.Right <= Right && bounds.Bottom >= Bottom && bounds.Top <= Top;
    }

    public bool Intersects(Bounds2d bounds)
    {
        return Left < bounds.Right && Right > bounds.Left && Bottom < bounds.Top && Top > bounds.Bottom;
    }


}