using System;

namespace NM;

public record struct Vector2Int(int X, int Y)
{
    public int X = X;
    public int Y = Y;
    
    public int LengthSquared => X * X + Y * Y;
    public double Length => Math.Sqrt(LengthSquared);
    public Vector2Int Normalized()
    {
        var len = Length;
        if (len == 0) return new Vector2Int(0, 0);
        return new Vector2Int((int)(X / len), (int)(Y / len));
    }
    public static Vector2Int Zero => new Vector2Int(0, 0);
    public static Vector2Int One => new Vector2Int(1, 1);
    
    public static Vector2Int operator +(Vector2Int a, Vector2Int b)
    {
        return new Vector2Int(a.X + b.X, a.Y + b.Y);
    }
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
}