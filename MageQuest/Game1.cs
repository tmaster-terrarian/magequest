using System;
using System.IO;
using MageQuest.Graphics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MageQuest;

public class Main : Game
{
    private static GraphicsDeviceManager _graphics;

    private static Camera camera;

    public static Camera Camera => camera;

    public static bool Paused { get; set; }

    public static Logger Logger { get; set; } = new("main");

    public static CoroutineRunner GlobalCoroutines { get; } = new();
    public static CoroutineRunner LevelCoroutines { get; } = new();

    public Main()
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

        {
            bool configExists = File.Exists(Path.Combine(FileLocations.ProgramPath, "config.ini"));

            PlayerData.ReadConfig();

            if(!configExists)
            {
                Logger.LogInfo("Regenerating config file");
                PlayerData.SaveConfig();
            }
        }

        _graphics.PreferredBackBufferWidth = Renderer.ScreenSize.X * Renderer.PixelScale;
        _graphics.PreferredBackBufferHeight = Renderer.ScreenSize.Y * Renderer.PixelScale;
        Renderer.Initialize(_graphics, GraphicsDevice, Window);

        ContentLoader.Initialize(Content);

        camera = new();

        Exiting += Game_Exiting;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // TODO: use this.Content to load your game content here
        Renderer.LoadContent();
        Fonts.LoadContent();
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

        UpdatePausables();

        GlobalCoroutines.Update();

        base.Update(gameTime);
    }

    private void UpdatePausables()
    {
        if(Paused) return;

        LevelCoroutines.Update();

        // TODO: Add your update logic here
    }

    protected override void Draw(GameTime gameTime)
    {
        Renderer.BeginDraw(samplerState: SamplerState.PointClamp, transformMatrix: Camera.Transform);


        Renderer.EndDraw();
        Renderer.BeginDrawUI();

        Renderer.SpriteBatch.DrawStringSpacesFix(Fonts.Regular, "testing hi", new FPoint(1, 1).ToVector2(), Color.White, 6);

        Renderer.EndDrawUI();

        Renderer.FinalizeDraw();

        base.Draw(gameTime);
    }

    private void Game_Exiting(object sender, ExitingEventArgs e)
    {
        PlayerData.SaveConfig();
    }
}
