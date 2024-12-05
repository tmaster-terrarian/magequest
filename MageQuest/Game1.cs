using System;
using System.IO;
using MageQuest.Graphics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MageQuest;

public class Game1 : Game
{
    private static GraphicsDeviceManager _graphics;

    private static Camera camera;

    public static Camera Camera => camera;

    public static bool Paused { get; set; }

    public static Logger Logger { get; set; } = new("main");

    public Game1()
    {
        Renderer.ScreenSize = new Point(320, 240);
        _graphics = Renderer.GetDefaultGraphicsDeviceManager(this);

        Content.RootDirectory = "data";
        IsMouseVisible = true;

        try
        {
            SoundEffect.Initialize();
        }
        catch(Exception e)
        {
            Logger.LogError($"Failed to initialize audio: {e}");
        }

        Window.Title = "Mage Quest!";
    }

    protected override void Initialize()
    {
        Logger.LogInfo("Entering main loop");

        bool configExists = File.Exists(Path.Combine(FileLocations.ProgramPath, "config.ini"));

        PlayerData.ReadConfig();

        _graphics.PreferredBackBufferWidth = Renderer.ScreenSize.X * Renderer.PixelScale;
        _graphics.PreferredBackBufferHeight = Renderer.ScreenSize.Y * Renderer.PixelScale;
        Renderer.Initialize(_graphics, GraphicsDevice, Window);

        if(!configExists)
        {
            Logger.LogInfo("Regenerating config file");
            PlayerData.SaveConfig();
        }

        camera = new();

        Exiting += Game_Exiting;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // TODO: use this.Content to load your game content here
        Renderer.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        Input.InputDisabled = !IsActive;

        Input.RefreshKeyboardState();
        Input.RefreshMouseState();
        Input.RefreshGamePadState();

        Input.UpdateTypingInput(gameTime);

        if(PlayerData.Config.Keybinds.Pause?.Pressed ?? false)
            Exit();

        if(Input.GetPressed(Keys.OemPlus))
            Renderer.PixelScale++;
        if(Input.GetPressed(Keys.OemMinus))
            Renderer.PixelScale--;

        // TODO: Add your update logic here

        UpdatePausable();

        base.Update(gameTime);
    }

    private void UpdatePausable()
    {
        if(Paused) return;

        // TODO: Add your update logic here
    }

    protected override void Draw(GameTime gameTime)
    {
        Renderer.BeginDraw(samplerState: SamplerState.PointClamp, transformMatrix: Camera.Transform);


        Renderer.EndDraw();
        Renderer.BeginDrawUI();


        Renderer.EndDrawUI();

        base.Draw(gameTime);
    }

    protected override void EndDraw()
    {
        Renderer.FinalizeDraw();
    }

    private void Game_Exiting(object sender, ExitingEventArgs e)
    {
        PlayerData.SaveConfig();
    }
}
