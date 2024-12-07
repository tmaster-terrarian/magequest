using System;
using System.Collections.Generic;

using MageQuest.Actors;

namespace MageQuest;

public class Actor : IDisposable
{
    private static ulong _lastId;

    private static readonly List<Actor> actors = [];
    private static readonly HashSet<(Solid, int)> solids = [];

    private static bool _locked = false;

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
        foreach(var actor in actors)
        {
            if(!actor.Enabled) continue;
            actor.Draw();
        }
    }

    public static void Disable(Actor actor)
    {
        if(actor.Enabled)
        {
            if(actor is Solid solid)
            {
                solids.Remove((solid, solid.SolidID));
            }

            actor.Disabled();
        }
    }

    public static void Enable(Actor actor)
    {
        if(actor is Solid solid && !actor.Enabled)
        {
            solids.Add((solid, solid.SolidID));
        }

        actor.Enabled = true;
    }

    public static T Initialize<T>(T actor) where T : Actor
    {
        if(_locked)
        {
            throw new InvalidOperationException("Cannot create new actors after the level is loaded");
        }

        actor._id = _lastId++;

        actors.Add(actor);

        if(actor is Solid solid)
        {
            solid.SolidID = actors.Count - 1;
            solids.Add((solid, solid.SolidID));
        }

        return actor;
    }

    private bool _disposed;

    private ulong _id;

    public event Action Disposed;

    public bool Enabled { get; private set; } = true;

    public ulong ID => _id;

    protected virtual void PreStart() {} // can create actors here

    protected virtual void Start() {}

    protected virtual void Update() {}

    protected virtual void Draw() {}

    protected virtual void Disabled() {}

    public void Dispose()
    {
        if(_disposed) return;
        _disposed = true;

        GC.SuppressFinalize(this);
        Disposed?.Invoke();
    }
}
