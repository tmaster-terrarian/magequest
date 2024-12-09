using System;

namespace MageQuest;

public struct Tag(uint bitmask) : IEquatable<Tag>
{
    public uint Bitmask { get; set; } = bitmask;

    private static readonly Tag _emptyTag = new(0u);

    public static Tag Empty => _emptyTag;

    public static bool Matches(Tag tag1, Tag tag2, TagFilter filter) => filter switch
    {
        TagFilter.NoFiltering => true,
        TagFilter.One => (tag1.Bitmask & tag2.Bitmask) != 0u,
        TagFilter.All => (tag1.Bitmask & tag2.Bitmask) == tag2.Bitmask,
        TagFilter.None => (tag1.Bitmask & tag2.Bitmask) == 0u,
        _ => false,
    };

    public static implicit operator Tag(uint value) => new(value);

    public static explicit operator uint(Tag value) => value.Bitmask;

    public static Tag operator ~(Tag value) => new(~value.Bitmask);

    public static Tag operator &(Tag left, Tag right) => new(left.Bitmask & right.Bitmask);

    public static Tag operator |(Tag left, Tag right) => new(left.Bitmask | right.Bitmask);

    public static Tag operator ^(Tag left, Tag right) => new(left.Bitmask ^ right.Bitmask);

    public readonly bool Matches(Tag other, TagFilter filter) => Matches(this, other, filter);

    public static Tag Add(Tag tag, Tag value) => tag with {Bitmask = tag.Bitmask | value.Bitmask};

    public static Tag Add(Tag tag, uint value) => tag with {Bitmask = tag.Bitmask | value};

    public static Tag Remove(Tag tag, Tag value) => tag with {Bitmask = tag.Bitmask & ~value.Bitmask};

    public static Tag Remove(Tag tag, uint value) => tag with {Bitmask = tag.Bitmask & ~value};

    public readonly bool Equals(Tag other)
    {
        return other.Bitmask == Bitmask;
    }

    public override readonly bool Equals(object obj)
    {
        return obj is Tag tag && Equals(tag);
    }

    public static bool operator ==(Tag left, Tag right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Tag left, Tag right)
    {
        return !(left == right);
    }

    public override readonly string ToString()
    {
        return Bitmask.ToString("b32");
    }
}

public struct TagQuery(Tag matchTags, TagFilter filter)
{
    public Tag MatchTags { get; set; } = matchTags;
    public TagFilter Filter { get; set; } = filter;

    public readonly bool Matches(Tag tag)
    {
        return tag.Matches(MatchTags, Filter);
    }
}
