using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

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
        _config = new(_configPath);

        Config.Keybinds.Right = ReadMappedInputData(_config.Read("right",   ConfigSection_Keybinds, "KB:Right")) ?? Config.Keybinds.Right;
        Config.Keybinds.Left = ReadMappedInputData(_config.Read("left",   ConfigSection_Keybinds, "KB:Left")) ?? Config.Keybinds.Left;
        Config.Keybinds.Down = ReadMappedInputData(_config.Read("down",   ConfigSection_Keybinds, "KB:Down")) ?? Config.Keybinds.Down;
        Config.Keybinds.Up = ReadMappedInputData(_config.Read("up",   ConfigSection_Keybinds, "KB:Up")) ?? Config.Keybinds.Up;

        Config.Keybinds.Jump = ReadMappedInputData(_config.Read("jump",   ConfigSection_Keybinds, "KB:Z")) ?? Config.Keybinds.Jump;
        Config.Keybinds.Attack = ReadMappedInputData(_config.Read("attack",   ConfigSection_Keybinds, "KB:X")) ?? Config.Keybinds.Attack;

        Config.Keybinds.Pause = ReadMappedInputData(_config.Read("pause",   ConfigSection_Keybinds, "KB:Escape")) ?? Config.Keybinds.Pause;

        if(int.TryParse(_config.Read("scale", ConfigSection_Graphics, "1"), out int value))
        {
            Graphics.Renderer.PixelScale = value;
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

        _config.Write("scale", Graphics.Renderer.PixelScale.ToString(), ConfigSection_Graphics);
    }

    public class Save
    {
        
    }

    public static class Config
    {
        public static class Keybinds
        {
            public static MappedInput Right;
            public static MappedInput Left;
            public static MappedInput Down;
            public static MappedInput Up;

            public static MappedInput Jump;
            public static MappedInput Attack;

            public static MappedInput Pause;
        }

        public static class Graphics
        {
            
        }
    }

    private static MappedInput ReadMappedInputData(string value)
    {
        if(value is null) return null;

        switch(value[..2])
        {
            case "KB":
                return new MappedInput.Keyboard(MapKeyboard(value[3..]));
            case "GP":
                return new MappedInput.GamePad(MapGamepad(value[3..]), PlayerIndex.One);
            case "MB":
                return new MappedInput.Mouse(MapMouse(value[3..]));
            default:
                return null;
        }
    }

    private static void WriteMappedInputData(string key, MappedInput mappedInput)
    {
        _config.Write(key, $"{mappedInput}", ConfigSection_Keybinds);
    }

    private static Keys MapKeyboard(string name)
    {
        return Enum.GetValues<Keys>()
            .First(key => Enum.GetName(key).Equals(name));
    }

    private static Buttons MapGamepad(string name)
    {
        return Enum.GetValues<Buttons>()
            .First(key => Enum.GetName(key).Equals(name));
    }

    private static MouseButtons MapMouse(string name)
    {
        return Enum.GetValues<MouseButtons>()
            .First(key => Enum.GetName(key).Equals(name));
    }
}
