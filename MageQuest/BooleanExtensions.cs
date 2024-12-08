using System.IO;

namespace MageQuest;

public static class BooleanExtensions
{
    public static byte ToByte(this bool value)
    {
        return (byte)(value ? 1 : 0);
    }

    public static int ToInt32(this bool value)
    {
        return value ? 1 : 0;
    }

    public static uint ToUInt32(this bool value)
    {
        return (uint)(value ? 1 : 0);
    }

    public static long ToInt64(this bool value)
    {
        return value ? 1 : 0;
    }

    public static ulong ToUInt64(this bool value)
    {
        return (ulong)(value ? 1 : 0);
    }

    public static float ToSingle(this bool value)
    {
        return value ? 1.0f : 0.0f;
    }

    public static double ToDouble(this bool value)
    {
        return value ? 1.0 : 0.0;
    }
}
