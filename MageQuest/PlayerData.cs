using System;
using System.IO;
using System.Linq;

using MageQuest.Graphics;
using MageQuest.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MageQuest;

public static class PlayerData
{
    private static IniFile _config;

    public static IniFile ConfigFile => _config;

    private const string ConfigSection_Keybinds = "Keybinds";
    private const string ConfigSection_Graphics = "Graphics";

    private static readonly string _configPath = Path.Combine(FileLocations.ProgramPath, "config.ini");

    public static void ReadConfig()
    {
        _config = new(File.Exists(_configPath) ? File.ReadAllText(_configPath) : "");

        Config.Keybinds.Right  = ReadMappedInputData(_config.Get("right",  ConfigSection_Keybinds, "KB:Right"))  ?? Config.Keybinds.Right;
        Config.Keybinds.Left   = ReadMappedInputData(_config.Get("left",   ConfigSection_Keybinds, "KB:Left"))   ?? Config.Keybinds.Left;
        Config.Keybinds.Down   = ReadMappedInputData(_config.Get("down",   ConfigSection_Keybinds, "KB:Down"))   ?? Config.Keybinds.Down;
        Config.Keybinds.Up     = ReadMappedInputData(_config.Get("up",     ConfigSection_Keybinds, "KB:Up"))     ?? Config.Keybinds.Up;
        Config.Keybinds.Jump   = ReadMappedInputData(_config.Get("jump",   ConfigSection_Keybinds, "KB:Z"))      ?? Config.Keybinds.Jump;
        Config.Keybinds.Attack = ReadMappedInputData(_config.Get("attack", ConfigSection_Keybinds, "KB:X"))      ?? Config.Keybinds.Attack;
        Config.Keybinds.Pause  = ReadMappedInputData(_config.Get("pause",  ConfigSection_Keybinds, "KB:Escape")) ?? Config.Keybinds.Pause;

        if(int.TryParse(_config.Get("scale", ConfigSection_Graphics, "1"), out int value))
        {
            BaseRenderer.PixelScale = value;
        }
    }

    public static void SaveConfig()
    {
        WriteMappedInputData("right", Config.Keybinds.Right);
        WriteMappedInputData("left", Config.Keybinds.Left);
        WriteMappedInputData("down", Config.Keybinds.Down);
        WriteMappedInputData("up", Config.Keybinds.Up);

        WriteMappedInputData("jump", Config.Keybinds.Jump);
        WriteMappedInputData("attack", Config.Keybinds.Attack);

        WriteMappedInputData("pause", Config.Keybinds.Pause);

        _config.Set("scale", BaseRenderer.PixelScale.ToString(), ConfigSection_Graphics);

        _config.Write(_configPath);
    }

    public static class Config
    {
        public static class Keybinds
        {
            public static MappedInput Right = new MappedInput.Keyboard(Keys.Right);
            public static MappedInput Left = new MappedInput.Keyboard(Keys.Left);
            public static MappedInput Down = new MappedInput.Keyboard(Keys.Down);
            public static MappedInput Up = new MappedInput.Keyboard(Keys.Up);

            public static MappedInput Jump = new MappedInput.Keyboard(Keys.Z);
            public static MappedInput Attack = new MappedInput.Keyboard(Keys.X);

            public static MappedInput Pause = new MappedInput.Keyboard(Keys.Escape);
        }
    }

    private static MappedInput ReadMappedInputData(string value)
    {
        return MappedInput.Parse(value, null);
    }

    private static void WriteMappedInputData(string key, MappedInput mappedInput)
    {
        _config.Set(key, $"{mappedInput}", ConfigSection_Keybinds);
    }
}
