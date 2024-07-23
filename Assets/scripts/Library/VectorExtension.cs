using System.Runtime.CompilerServices;
using UnityEngine;

public static class Vector3Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 xy(this Vector3 aVector) => new Vector2(aVector.x, aVector.y);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 xz(this Vector3 aVector) => new Vector2(aVector.x, aVector.z);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 yz(this Vector3 aVector) => new Vector2(aVector.y, aVector.z);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 yx(this Vector3 aVector) => new Vector2(aVector.y, aVector.x);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 zx(this Vector3 aVector) => new Vector2(aVector.z, aVector.x);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 zy(this Vector3 aVector) => new Vector2(aVector.z, aVector.y);
}

public static class Vector2Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 To3d(this Vector2 aVector) => new Vector3(aVector.x, 0, aVector.y);
}
