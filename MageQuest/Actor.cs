using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MageQuest;

public class Actor : IDisposable
{
    private static ulong _lastId;

    private static readonly List<Actor> actors = [];
    private static readonly HashSet<(Solid, int)> solids = [];

    private static bool _locked = false;

    private static Comparison<Actor> CompareDepth => (a, b) => Math.Sign(b.Depth - a.Depth);

    private static List<Actor> ToDraw
    {
        get
        {
            if(!_drawOrderDirty) return toDraw;

            toDraw = [..actors];
            toDraw.Sort(CompareDepth);

            _drawOrderDirty = false;

            return toDraw;
        }
    }

    private static bool _drawOrderDirty = true;

    private static List<Actor> toDraw;

    public static void DoStart()
    {
        // as a for loop it can naturally expand, since the list
        // will never shrink and items are appended to the end
        // (at least i hope)
        for(int i = 0; i < actors.Count; i++)
        {
            var actor = actors[i];

            if(!actor.Enabled) continue;
            actor.PreStart();
        }

        _locked = true;

        foreach(var actor in actors)
        {
            if(!actor.Enabled) continue;
            actor.Start();
        }
    }

    public static void Cleanup()
    {
        foreach(var actor in actors)
        {
            if(actor._disposed) continue;
            actor.Dispose();
        }

        toDraw.Clear();
        solids.Clear();
        actors.Clear();

        GC.Collect();

        _lastId = 0;
        _locked = false;
    }

    public static void DoUpdate()
    {
        foreach(var actor in actors)
        {
            if(!actor.Enabled) continue;

            actor.Update();
        }
    }

    public static void DoDraw()
    {
        foreach(var actor in ToDraw)
        {
            if(!actor.Enabled) continue;
            actor.Draw();
        }
    }

    public static void DoDrawUI()
    {
        foreach(var actor in ToDraw)
        {
            if(!actor.Enabled) continue;
            actor.DrawUI();
        }
    }

    public static T Initialize<T>(T actor) where T : Actor
    {
        if(_locked)
        {
            throw new InvalidOperationException("Cannot create new actors after the level is loaded");
        }

        actor._id = _lastId++;
        actor._index = actors.Count;

        actors.Add(actor);

        if(actor is Solid solid)
        {
            solids.Add((solid, solid.Index));
        }

        return actor;
    }

    public static bool SolidMeeting(FRectangle rectangle) => SolidPlace(rectangle) is not null;

    public static Solid? SolidPlace(FRectangle rectangle)
    {
        if(solids.Count == 0) return null;
        foreach(var solid in solids)
        {
            if(solid.Item1.Enabled && solid.Item1.IsColliding(rectangle))
            {
                return solid.Item1;
            }
        }
        return null;
    }

    public static bool SolidMeeting(FRectangle rectangle, Tag matchTags, TagFilter filter)
        => SolidPlace(rectangle, matchTags, filter) is not null;

    public static Solid? SolidPlace(FRectangle rectangle, Tag matchTags, TagFilter filter)
    {
        if(solids.Count == 0) return null;
        foreach(var solid in solids)
        {
            if(solid.Item1.Enabled && solid.Item1.Tag.Matches(matchTags, filter) && solid.Item1.IsColliding(rectangle))
            {
                return solid.Item1;
            }
        }
        return null;
    }

    public static List<Solid> SolidsPlace(FRectangle rectangle)
    {
        List<Solid> list = [];
        if(solids.Count == 0) return list;

        foreach(var solid in solids)
        {
            if(solid.Item1.Enabled && solid.Item1.IsColliding(rectangle))
            {
                list.Add(solid.Item1);
            }
        }

        return list;
    }

    public static List<Solid> SolidsPlace(FRectangle rectangle, Tag matchTags, TagFilter filter)
    {
        List<Solid> list = [];
        if(solids.Count == 0) return list;

        foreach(var solid in solids)
        {
            if(solid.Item1.Enabled && solid.Item1.Tag.Matches(matchTags, filter) && solid.Item1.IsColliding(rectangle))
            {
                list.Add(solid.Item1);
            }
        }

        return list;
    }

    private bool _disposed;

    private bool _enabled = true;

    private ulong _id;

    private int _index;

    private int _depth;

    private FRectangle _hitbox;

    public event Action Disposed;

    public int Index => _index;

    public Tag Tag { get; set; }

    public FRectangle Hitbox { get => _hitbox; set => _hitbox = value; }

    public FPoint Position { get => _hitbox.Location; set => _hitbox.Location = value; }
    public int X { get => _hitbox.X; set => _hitbox.X = value; }
    public int Y { get => _hitbox.Y; set => _hitbox.Y = value; }

    public FPoint Size { get => _hitbox.Size; set => _hitbox.Size = value; }
    public int Width { get => _hitbox.Width; set => _hitbox.Width = value; }
    public int Height { get => _hitbox.Height; set => _hitbox.Height = value; }

    public bool OnGround { get; protected set; }

    public int Depth {
        get => _depth;
        set {
            if(_depth != value)
            {
                _depth = value;
                _drawOrderDirty = true;
            }
        }
    }

    public bool Enabled {
        get => _enabled;
        set {
            if(_enabled != value)
            {
                _enabled = value;
                if(value == false)
                {
                    if(this is Solid solid)
                    {
                        solids.Remove((solid, solid.Index));
                    }

                    this.Disabled();
                }
                else
                {
                    if(this is Solid solid)
                    {
                        solids.Add((solid, solid.Index));
                    }
                }
            }
        }
    }

    public ulong ID => _id;

    protected virtual void PreStart() {} // can create actors here

    protected virtual void Start() {}

    protected virtual void Update() {}

    protected virtual void Draw() {}

    protected virtual void DrawUI() {}

    protected virtual void Disabled() {}

    public void Dispose()
    {
        if(_disposed) return;
        _disposed = true;

        GC.SuppressFinalize(this);
        Disposed?.Invoke();
    }

    protected virtual bool CheckColliding(int offsetX, int offsetY, Tag matchTags = default, TagFilter filter = TagFilter.NoFiltering)
        => false;

    protected virtual void MoveX(int move, Action? onCollide)
    {
        if(move != 0)
        {
            int sign = Math.Sign(move);
            while(move != 0)
            {
                bool col1 = CheckColliding(sign, 0);
                if(col1 && !CheckColliding(sign, -1))
                {
                    // slope up
                    _hitbox.X += sign;
                    _hitbox.Y -= 1;
                    move -= sign;
                }
                else if(!col1)
                {
                    // slope down
                    if(!CheckColliding(sign, 1) && CheckColliding(sign, 2))
                        _hitbox.Y += 1;

                    _hitbox.X += sign;
                    move -= sign;
                }
                else
                {
                    onCollide?.Invoke();
                    break;
                }
            }
        }
    }

    protected virtual void MoveY(int move, Action? onCollide)
    {
        if(move != 0)
        {
            int sign = Math.Sign(move);
            while(move != 0)
            {
                if(!CheckColliding(0, sign))
                {
                    _hitbox.Y += sign;
                    move -= sign;
                    continue;
                }

                onCollide?.Invoke();
                break;
            }
        }
    }

    protected virtual bool IsRiding(Solid solid)
    {
        ArgumentNullException.ThrowIfNull(solid, nameof(solid));

        return solid.IsColliding(Hitbox.Shift(0, 1));
    }

    public virtual void Squished() {}
}
