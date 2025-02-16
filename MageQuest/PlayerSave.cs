using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;

namespace MageQuest;

public static class PlayerSave
{
    private static readonly string _savePath = Path.Combine(FileLocations.ProgramPath, "saves");
    private static readonly List<SaveData> _saves = [];

    public static SaveData Current
    {
        get => _saves[CurrentIndex];
        private set => _saves[CurrentIndex] = value;
    }

    public static int CurrentIndex { get; private set; }

    public static ReadOnlyCollection<SaveData> Saves => _saves.AsReadOnly();

    public class SaveData
    {
        public Flags Flags { get; set; }

        public string Level { get; set; } = "test";

        public FPoint Position { get; set; }
    }

    [Flags]
    public enum Flags : ulong
    {
        None = 0,
        HasStaff = 1,
    }

    public static void Initialize()
    {
        LoadAll();
    }

    public static void LoadAll()
    {
        Directory.CreateDirectory(_savePath);
        _saves.Clear();

        List<int> paths = [];
        foreach(var fullPath in Directory.EnumerateFiles(_savePath))
        {
            paths.Add(int.Parse(Path.GetFileName(fullPath)));
            _saves.Add(null);
        }

        if(paths.Count == 0)
        {
            paths.Add(0);
            _saves.Add(null);
        }

        paths.Sort((s1, s2) => s1 - s2);

        for(int i = 0; i < paths.Count; i++)
        {
            LoadFile(i);
        }
    }

    public static bool Load(int saveNum)
    {
        if(saveNum >= _saves.Count || _saves[saveNum] is null)
        {
            LoadAll();

            if(saveNum >= _saves.Count || _saves[saveNum] is null)
            {
                Main.Logger.LogError("failed to load save, aborting");
                return false;
            }
        }

        CurrentIndex = saveNum;
        return true;
    }

    public static void LoadFile(int saveNum)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(saveNum, _saves.Count);
        Main.Logger.LogInfo($"reading save {saveNum}");

        var path = Path.Combine(_savePath, $"{saveNum}");
        if(!File.Exists(path))
        {
            _saves[saveNum] = new();
            Main.Logger.LogWarning($"failed to load save data slot {saveNum}: file missing");
            Save(saveNum);
            return;
        }

        try
        {
            SaveData data = new();
            using BinaryReader reader = new(File.Open(path, FileMode.Open, FileAccess.Read));

            data.Flags = (Flags)reader.ReadUInt64();
            data.Level = reader.ReadString();
            data.Position = new() {
                X = reader.ReadInt32(),
                Y = reader.ReadInt32()
            };

            _saves[saveNum] = data;

            Main.Logger.LogInfo($"loaded save {saveNum}");
        }
        catch(Exception e)
        {
            _saves[saveNum] = new();
            Main.Logger.LogError($"failed to load save {saveNum}: error reading file\ncaused by: {e}");
        }
    }

    public static void Save(int saveNum)
    {
        var path = Path.Combine(_savePath, $"{saveNum}");
        Directory.CreateDirectory(_savePath);

        try
        {
            var data = _saves[saveNum];
            using BinaryWriter writer = new(File.Open(path, FileMode.Create, FileAccess.Write));

            writer.Write((ulong)data.Flags);
            writer.Write(data.Level);
            writer.Write(data.Position.X);
            writer.Write(data.Position.Y);

            Main.Logger.LogInfo($"saved to slot {saveNum}");
        }
        catch(Exception e)
        {
            Main.Logger.LogError($"failed to save to slot {saveNum}: error writing file\ncaused by: {e}");
        }
    }
}
