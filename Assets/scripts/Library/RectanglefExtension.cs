using System;
using System.Drawing;

static class RectangleUtils
{
    public static PointF Center(this RectangleF rectangle)
    {
        return new PointF(rectangle.X + rectangle.Width / 2,
                          rectangle.Y - rectangle.Height / 2);
    }
}