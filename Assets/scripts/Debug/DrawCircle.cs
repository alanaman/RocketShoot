using UnityEngine;

class Debug : UnityEngine.Debug
{
    public static void DrawCircle(Vector3 position, float radius, int segments, Color color)
    {
        // If either radius or number of segments are less or equal to 0, skip drawing
        if (radius <= 0.0f || segments <= 0)
        {
            return;
        }

        // Single segment of the circle covers (360 / number of segments) degrees
        float angleStep = (360.0f / segments);

        // Result is multiplied by Mathf.Deg2Rad constant which transforms degrees to radians
        // which are required by Unity's Mathf class trigonometry methods

        angleStep *= Mathf.Deg2Rad;

        // lineStart and lineEnd variables are declared outside of the following for loop
        Vector3 lineStart = Vector3.zero;
        Vector3 lineEnd = Vector3.zero;

        for (int i = 0; i < segments; i++)
        {
            // Line start is defined as starting angle of the current segment (i)
            lineStart.x = Mathf.Cos(angleStep * i);
            lineStart.z = Mathf.Sin(angleStep * i);

            // Line end is defined by the angle of the next segment (i+1)
            lineEnd.x = Mathf.Cos(angleStep * (i + 1));
            lineEnd.z = Mathf.Sin(angleStep * (i + 1));

            // Results are multiplied so they match the desired radius
            lineStart *= radius;
            lineEnd *= radius;

            // Results are offset by the desired position/origin 
            lineStart += position;
            lineEnd += position;

            // Points are connected using DrawLine method and using the passed color
            DrawLine(lineStart, lineEnd, color);
        }
    }

    public static void DrawX(Vector3 position, float radius, int segments, Color color)
    {
        float radScaled = radius / Mathf.Sqrt(2);
        Vector3 northwest = new Vector3(radScaled, 0, -radScaled);
        Vector3 northEast = new Vector3(radScaled, 0, radScaled);

        DrawLine(position + northwest, position - northwest, color);
        DrawLine(position + northEast, position - northEast, color);
    }

    public static void DrawPlus(Vector3 position, float radius, int segments, Color color)
    {
        Vector3 right = new Vector3(radius, 0, 0);
        Vector3 top = new Vector3(0, 0, radius);

        DrawLine(position + right, position - right, color);
        DrawLine(position + top, position - top, color);
    }

}