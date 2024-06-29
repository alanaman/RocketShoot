using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public interface ISpatialEntity2d
{
    RectangleF bounds { get; }
}