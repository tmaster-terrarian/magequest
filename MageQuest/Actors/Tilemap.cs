using System;
using System.Collections.Generic;
using MageQuest.Graphics;
using Microsoft.Xna.Framework;

namespace MageQuest.Actors;

public class Tilemap : Solid
{
    private readonly int[] _values;

    public int[] Values => _values;

    public int TileSize { get; set; } = Consts.OneTile;

    FRectangle[,] _collisions;

    FRectangle[,] Collisions {
        get {
            if(_collisions != null && _collisions.LongLength == _values.LongLength) return _collisions;

            FRectangle[,] rectangles = new FRectangle[Width, Height];

            for(int y = 0; y < Height; y++)
            {
                for(int x = 0; x < Width; x++)
                {
                    var tile = _values[(y * Width) + x];
                    FRectangle rect = FRectangle.Empty;

                    if(tile != 0)
                        rect = new FRectangle(x * TileSize, y * TileSize, TileSize, TileSize, false);

                    rectangles[x, y] = rect;
                }
            }

            return _collisions = rectangles;
        }
    }

    public Tilemap(int width, int height, int[] values)
    {
        Width = width;
        Height = height;
        _values = [..values];

        Disposed += OnDisposed;
    }

    public override bool IsCollidingWith(FRectangle rect)
    {
        FRectangle[,] cols = Collisions;

        FRectangle newRect = rect;
        newRect.X = rect.X / TileSize;
        newRect.Y = rect.Y / TileSize;
        newRect.Width = MathHelper.Max(1, MathUtil.CeilToInt((rect.X + rect.Width) / (float)TileSize) - newRect.X);
        newRect.Height = MathHelper.Max(1, MathUtil.CeilToInt((rect.Y + rect.Height) / (float)TileSize) - newRect.Y);

        for(int y = newRect.Y; y < newRect.Y + newRect.Height; y++)
        {
            for(int x = newRect.X; x < newRect.X + newRect.Width; x++)
            {
                if(!InWorld(x, y)) continue;
                if(_values[(y * Width) + x] == 0) continue;

                if(rect.Intersects(cols[x, y]))
                {
                    return true;
                }
            }
        }

        return false;
    }

    protected override bool CheckColliding(int offsetX, int offsetY, Tag matchTags = default, TagFilter filter = TagFilter.NoFiltering)
        => false;
    public override void MoveX(int move, Action onCollide) {}
    public override void MoveY(int move, Action onCollide) {}
    public override bool IsRiding(Solid solid) => false;

    public static bool InWorld(Tilemap tilemap, int x, int y)
    {
        return x >= 0 && x < tilemap.Width && y >= 0 && y < tilemap.Height;
    }

    public static bool InWorld(Tilemap tilemap, Point pos)
    {
        return InWorld(tilemap, pos.X, pos.Y);
    }

    public bool InWorld(int x, int y) => InWorld(this, x, y);

    public bool InWorld(Point pos) => InWorld(this, pos.X, pos.Y);

    private void OnDisposed()
    {
        // idk
    }

    protected override void Draw()
    {
        for(int x = 0; x < Width; x++)
        {
            for(int y = 0; y < Height; y++)
            {
                BaseRenderer.SpriteBatch.DrawNineSlice(
                    BaseRenderer.OutlineTexture,
                    Collisions[x, y].ToRectangle(),
                    null,
                    new Point(1),
                    new Point(1),
                    Color.Red
                );
            }
        }
    }
}
