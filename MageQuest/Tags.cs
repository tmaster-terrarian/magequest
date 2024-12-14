using System;

namespace MageQuest;

[Flags]
public enum ActorTags : uint
{
    PlayerCollidable = 1,
    EnemyCollidable = 2,
}

public static class TagExtensions
{
    public static void Add(this Tag tag, ActorTags actorTags) => Tag.Add(tag, (uint)actorTags);

    public static void Remove(this Tag tag, ActorTags actorTags) => Tag.Remove(tag, (uint)actorTags);

    public static void Add<T>(this Tag tag, T enumValue) where T : struct, Enum, IConvertible
        => Tag.Add(tag, enumValue.ToUInt32(null));

    public static void Remove<T>(this Tag tag, T enumValue) where T : struct, Enum, IConvertible
        => Tag.Remove(tag, enumValue.ToUInt32(null));
}
