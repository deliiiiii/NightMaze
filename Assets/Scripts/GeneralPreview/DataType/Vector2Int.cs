using System;
using System.Diagnostics;
using UnityEngine;

namespace GeneralPreview;
[DebuggerStepThrough][Serializable]
public record struct Vector2Int(int X, int Y) : IComparable<Vector2Int>
{
    public int X = X;
    public int Y = Y;

    public static implicit operator Vector3(Vector2Int v) => new(v.X, v.Y);
    public static implicit operator Vector2(Vector2Int v) => new(v.X, v.Y);
    
    public int LengthSquared => X * X + Y * Y;
    public double Length => Math.Sqrt(LengthSquared);
    public Vector2Int Normalized()
    {
        var len = Length;
        if (len == 0) return new Vector2Int(0, 0);
        return new Vector2Int((int)(X / len), (int)(Y / len));
    }
    public static Vector2Int Zero = new (0, 0);
    public static Vector2Int One = new (1, 1);
    public static Vector2Int MinusOne = new (-1, -1);
    
    public static Vector2Int Up = new (0, 1);
    public static Vector2Int Down = new (0, -1);
    public static Vector2Int Left = new (-1, 0);
    public static Vector2Int Right = new (1, 0);
    public static Vector2Int MaxValue = new (int.MaxValue, int.MaxValue);
    public static Vector2Int MinValue = new (int.MinValue, int.MinValue);
    
    public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator +(Vector2Int a, Vector2 b) => new(a.X + b.x, a.Y + b.y);

    public static Vector2Int operator -(Vector2Int a, Vector2Int b)
    {
        return new Vector2Int(a.X - b.X, a.Y - b.Y);
    }
    public static Vector2Int operator *(Vector2Int a, int b)
    {
        return new Vector2Int(a.X * b, a.Y * b);
    }
    public static Vector2Int operator *(int a, Vector2Int b)
    {
        return new Vector2Int(a * b.X, a * b.Y);
    }
    public static Vector2Int operator /(Vector2Int a, int b)
    {
        return new Vector2Int(a.X / b, a.Y / b);
    }
    [DebuggerStepThrough]
    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public int CompareTo(Vector2Int other)
    {
        var xComparison = X.CompareTo(other.X);
        return xComparison != 0 ? xComparison : Y.CompareTo(other.Y);
    }
}