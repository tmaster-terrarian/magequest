using System;

namespace MageQuest;

[Flags]
public enum ActorTags : uint
{
    PlayerCollidable = 1,
}

public static class TagExtensions
{
    public static void Add(this Tag tag, ActorTags actorTags) => tag.Add((uint)actorTags);

    public static void Remove(this Tag tag, ActorTags actorTags) => tag.Remove((uint)actorTags);

    public static void Add<T>(this Tag tag, T enumValue) where T : struct, Enum, IConvertible
        => tag.Add(enumValue.ToUInt32(null));

    public static void Remove<T>(this Tag tag, T enumValue) where T : struct, Enum, IConvertible
        => tag.Remove(enumValue.ToUInt32(null));
}
