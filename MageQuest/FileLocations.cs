using System;
using System.IO;
using System.Reflection;

namespace MageQuest;

public static class FileLocations
{
    private static readonly Assembly assembly = Assembly.GetEntryAssembly();

    public static string ProgramPath => Path.GetDirectoryName(assembly.Location);

    public static string Data => Path.Combine(ProgramPath, "data");

    public static string Saves => Path.Combine(ProgramPath, "saves");
}
