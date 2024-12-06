using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using Microsoft.Xna.Framework;

namespace MageQuest;

[DataContract]
[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct FPoint : IEquatable<FPoint>
{
    private static readonly FPoint zeroFPoint = new();
    private static readonly FPoint oneFPoint = new(1);
    private static readonly FPoint unitXFPoint = new(1, 0);
    private static readonly FPoint unitYFPoint = new(0, 1);

    public static FPoint Zero => zeroFPoint;
    public static FPoint One => oneFPoint;
    public static FPoint UnitX => unitXFPoint;
    public static FPoint UnitY => unitYFPoint;

    public const int SHFT = 9;
    public const int MULT = 0b1 << SHFT;

    [DataMember]
    [JsonInclude]
    public int X;

    [DataMember]
    [JsonInclude]
    public int Y;

    [JsonIgnore]
    public readonly int XPixels => X >> SHFT;

    [JsonIgnore]
    public readonly int YPixels => Y >> SHFT;

    [JsonIgnore]
    internal readonly string DebugDisplayString => $"{XPixels}.{Math.Abs(X & 0x1FF)}  {YPixels}.{Math.Abs(Y & 0x1FF)}";

    /// <summary>
    /// Constructs a point with X and Y from two values
    /// </summary>
    /// <param name="x">The x coordinate in 2d-space.</param>
    /// <param name="y">The y coordinate in 2d-space.</param>
    public FPoint(int x, int y, bool shift)
    {
        X = shift ? x << SHFT : x;
        Y = shift ? y << SHFT : y;
    }

    /// <summary>
    /// Constructs a point with X and Y from two values
    /// </summary>
    /// <param name="x">The x coordinate in 2d-space.</param>
    /// <param name="y">The y coordinate in 2d-space.</param>
    public FPoint(int x, int y)
    {
        X = x << SHFT;
        Y = y << SHFT;
    }

    /// <summary>
    /// Constructs a point with X and Y set to the same value.
    /// </summary>
    /// <param name="value">The x and y coordinates in 2d-space.</param>
    public FPoint(int value, bool shift)
    {
        X = shift ? value << SHFT : value;
        Y = X;
    }

    /// <summary>
    /// Constructs a point with X and Y set to the same value.
    /// </summary>
    /// <param name="value">The x and y coordinates in 2d-space.</param>
    public FPoint(int value)
    {
        X = value << SHFT;
        Y = X;
    }

    public static FPoint operator -(FPoint value)
    {
        value.X = -value.X;
        value.Y = -value.Y;
        return value;
    }

    public static FPoint operator +(FPoint left, FPoint right)
    {
        left.X += right.X;
        left.Y += right.Y;
        return left;
    }

    public static FPoint operator -(FPoint left, FPoint right)
    {
        left.X -= right.X;
        left.Y -= right.Y;
        return left;
    }

    public static FPoint operator *(FPoint left, FPoint right)
    {
        left.X *= right.X;
        left.Y *= right.Y;
        return left;
    }

    public static FPoint operator /(FPoint left, FPoint right)
    {
        left.X /= right.X;
        left.Y /= right.Y;
        return left;
    }

    public static FPoint operator *(FPoint value, int scalar)
    {
        value.X *= scalar;
        value.Y *= scalar;
        return value;
    }

    public static FPoint operator /(FPoint value, int scalar)
    {
        value.X /= scalar;
        value.Y /= scalar;
        return value;
    }

    public static FPoint operator >>(FPoint value, int shift)
    {
        value.X >>= shift;
        value.Y >>= shift;
        return value;
    }

    public static FPoint operator <<(FPoint value, int shift)
    {
        value.X <<= shift;
        value.Y <<= shift;
        return value;
    }

    public static FPoint Clamp(FPoint value, FPoint min, FPoint max)
    {
        return new(MathHelper.Clamp(value.X, min.X, max.X), MathHelper.Clamp(value.Y, min.Y, max.Y), false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Point ToPoint()
    {
        return new(XPixels, YPixels);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 ToVector2()
    {
        return new((float)X / MULT, (float)Y / MULT);
    }

    public readonly bool Equals(FPoint other)
    {
        return other.X == X && other.Y == Y;
    }

    public override readonly bool Equals(object obj)
    {
        return obj is FPoint point && Equals(point);
    }

    public static bool operator ==(FPoint left, FPoint right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(FPoint left, FPoint right)
    {
        return !left.Equals(right);
    }

    public override readonly int GetHashCode()
    {
        return (17 * 23 + X.GetHashCode()) * 23 + Y.GetHashCode();
    }

    public override readonly string ToString()
    {
        return $"{{X:{XPixels}.{Math.Abs(X & 0x1FF)} Y:{YPixels}.{Math.Abs(Y & 0x1FF)}}}";
    }

    public readonly void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }
}

public static class FPointExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FPoint ToFPoint(this Vector2 value)
    {
        return new((int)(value.X * FPoint.MULT), (int)(value.Y * FPoint.MULT), false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FPoint ToFPoint(this Point value)
    {
        return new(value.X << FPoint.SHFT, value.Y << FPoint.SHFT, false);
    }
}
