using System;
using Microsoft.Xna.Framework;

namespace MageQuest;

public struct FRectangle : IEquatable<FRectangle>
{
    private static FRectangle emptyRectangle = new();

    public static FRectangle Empty => emptyRectangle;

    public FPoint Location {
        readonly get => new(X, Y, false);
        set {
            X = value.X;
            Y = value.Y;
        }
    }

    public FPoint Size {
        readonly get => new(Width, Height, false);
        set {
            Width = value.X;
            Height = value.Y;
        }
    }

    public int X;

    public int Y;

    public int Width;

    public int Height;

    public readonly int Left => X;

    public readonly int Right => X + Width;

    public readonly int Top => Y;

    public readonly int Bottom => Y + Height;

    public readonly int XPixels => X / Consts.MULT;

    public readonly int YPixels => Y / Consts.MULT;

    public readonly int WidthPixels => Width / Consts.MULT;

    public readonly int HeightPixels => Height / Consts.MULT;

    public readonly FPoint Center => new(X + Width / 2, Y + Height / 2, false);

    public readonly bool IsEmpty
    {
        get
        {
            if (Width == 0 && Height == 0 && X == 0)
            {
                return Y == 0;
            }

            return false;
        }
    }

    public FRectangle(int x, int y, int w, int h, bool shift = true)
    {
        X = shift ? x << Consts.SHFT : x;
        Y = shift ? y << Consts.SHFT : y;
        Width = shift ? w << Consts.SHFT : w;
        Height = shift ? h << Consts.SHFT : h;
    }

    public FRectangle(Point location, Point size, bool shift = true)
    {
        X = shift ? location.X << Consts.SHFT : location.X;
        Y = shift ? location.Y << Consts.SHFT : location.Y;
        Width = shift ? size.X << Consts.SHFT : size.X;
        Height = shift ? size.Y << Consts.SHFT : size.Y;
    }

    public FRectangle(FPoint location, FPoint size)
    {
        X = location.X;
        Y = location.Y;
        Width = size.X;
        Height = size.Y;
    }

    public static bool operator ==(FRectangle a, FRectangle b)
    {
        if (a.X == b.X && a.Y == b.Y && a.Width == b.Width)
        {
            return a.Height == b.Height;
        }

        return false;
    }

    public static bool operator !=(FRectangle a, FRectangle b)
    {
        return !(a == b);
    }

    public readonly bool Contains(int x, int y)
    {
        if (X <= x && x < X + Width && Y <= y)
        {
            return y < Y + Height;
        }

        return false;
    }

    public readonly bool Contains(FPoint value)
    {
        if (X <= value.X && value.X < X + Width && Y <= value.Y)
        {
            return value.Y < Y + Height;
        }

        return false;
    }

    public readonly bool Contains(FRectangle value)
    {
        if (X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y)
        {
            return value.Y + value.Height <= Y + Height;
        }

        return false;
    }

    public override readonly bool Equals(object obj)
    {
        return obj is FRectangle rectangle && this == rectangle;
    }

    public readonly bool Equals(FRectangle other)
    {
        return this == other;
    }

    public override readonly int GetHashCode()
    {
        return (((17 * 23 + X.GetHashCode()) * 23 + Y.GetHashCode()) * 23 + Width.GetHashCode()) * 23 + Height.GetHashCode();
    }

    public readonly Rectangle ToRectangle()
    {
        return new(XPixels, YPixels, WidthPixels, HeightPixels);
    }

    public void Inflate(int horizontalAmount, int verticalAmount)
    {
        this.X -= horizontalAmount;
        this.Y -= verticalAmount;
        this.Width += horizontalAmount * 2;
        this.Height += verticalAmount * 2;
    }

    public readonly bool Intersects(FRectangle value)
    {
        if (value.Left < Right && Left < value.Right && value.Top < Bottom)
        {
            return Top < value.Bottom;
        }

        return false;
    }

    public static FRectangle Intersect(FRectangle value1, FRectangle value2)
    {
        Intersect(ref value1, ref value2, out var result);
        return result;
    }

    public static void Intersect(ref FRectangle value1, ref FRectangle value2, out FRectangle result)
    {
        if (value1.Intersects(value2))
        {
            int num = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
            int num2 = Math.Max(value1.X, value2.X);
            int num3 = Math.Max(value1.Y, value2.Y);
            int num4 = Math.Min(value1.Y + value1.Height, value2.Y + value2.Height);
            result = new(num2, num3, num - num2, num4 - num3, false);
        }
        else
        {
            result = new(0, 0, 0, 0);
        }
    }

    public static FRectangle Union(FRectangle value1, FRectangle value2)
    {
        int num = Math.Min(value1.X, value2.X);
        int num2 = Math.Min(value1.Y, value2.Y);
        return new FRectangle(num, num2, Math.Max(value1.Right, value2.Right) - num, Math.Max(value1.Bottom, value2.Bottom) - num2, false);
    }

    public static void Union(ref FRectangle value1, ref FRectangle value2, out FRectangle result)
    {
        result.X = Math.Min(value1.X, value2.X);
        result.Y = Math.Min(value1.Y, value2.Y);
        result.Width = Math.Max(value1.Right, value2.Right) - result.X;
        result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
    }

    public readonly FRectangle Shift(int offsetX, int offsetY)
    {
        FRectangle rect = this;
        rect.X += offsetX;
        rect.Y += offsetY;
        return rect;
    }

    public readonly FRectangle Shift(FPoint amount)
    {
        FRectangle rect = this;
        rect.X += amount.X;
        rect.Y += amount.Y;
        return rect;
    }

    public void Offset(int offsetX, int offsetY)
    {
        this.X += offsetX;
        this.Y += offsetY;
    }

    public void Offset(FPoint amount)
    {
        this.X += amount.X;
        this.Y += amount.Y;
    }

    public override readonly string ToString()
    {
        return $"{{X:{XPixels}.{Math.Abs(X & 0x1FF)} Y:{YPixels}.{Math.Abs(Y & 0x1FF)} Width:{WidthPixels}.{Math.Abs(Width & 0x1FF)} Height:{HeightPixels}.{Math.Abs(Height & 0x1FF)}}}";
    }

    public readonly void Deconstruct(out int x, out int y, out int width, out int height)
    {
        x = X;
        y = Y;
        width = Width;
        height = Height;
    }
}
