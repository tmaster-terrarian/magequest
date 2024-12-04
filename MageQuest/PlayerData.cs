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

        ReadMappedInputData(_config.Read("right",   ConfigSection_Keybinds, "KB:Right"),    ref Config.Keybinds.Right);
        ReadMappedInputData(_config.Read("left",    ConfigSection_Keybinds, "KB:Left"),     ref Config.Keybinds.Left);
        ReadMappedInputData(_config.Read("down",    ConfigSection_Keybinds, "KB:Down"),     ref Config.Keybinds.Down);
        ReadMappedInputData(_config.Read("up",      ConfigSection_Keybinds, "KB:Up"),       ref Config.Keybinds.Up);

        ReadMappedInputData(_config.Read("jump",    ConfigSection_Keybinds, "KB:Z"),        ref Config.Keybinds.Jump);
        ReadMappedInputData(_config.Read("attack",  ConfigSection_Keybinds, "KB:X"),        ref Config.Keybinds.Attack);

        ReadMappedInputData(_config.Read("pause",   ConfigSection_Keybinds, "KB:Escape"),   ref Config.Keybinds.Pause);

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

    private static void ReadMappedInputData(string value, ref MappedInput mappedInput)
    {
        if(value is null) return;

        string[] split = value.Split(':', 2);
        switch(split[0])
        {
            case "KB":
                mappedInput = new MappedInput.Keyboard(MapKeyboard(split[1]));
                break;
            case "GP":
                mappedInput = new MappedInput.GamePad(MapGamepad(split[1]), PlayerIndex.One);
                break;
            case "M":
                mappedInput = new MappedInput.Mouse(MapMouse(split[1]));
                break;
        }
    }

    private static void WriteMappedInputData(string key, MappedInput mappedInput)
    {
        _config.Write(key, $"{mappedInput}", ConfigSection_Keybinds);
    }

    private static Keys MapKeyboard(string name)
    {
        return Enum.GetValues<Keys>()
            .First(key => key.ToString().Equals(name));
    }

    private static Buttons MapGamepad(string name)
    {
        return Enum.GetValues<Buttons>()
            .First(key => key.ToString().Equals(name));
    }

    private static MouseButtons MapMouse(string name)
    {
        return Enum.GetValues<MouseButtons>()
            .First(key => key.ToString().Equals(name));
    }
}
