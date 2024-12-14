using System;
using System.IO;
using MageQuest.Actors;
using MageQuest.Graphics;
using MageQuest.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameReload;
using MonoGameReload.Assets;

namespace MageQuest;

public class Main : Game
{
    private static GraphicsDeviceManager _graphics;

    public static Camera Camera  { get; private set; }

    public static bool Paused { get; set; }

    public static Logger Logger { get; set; } = new("main");

    public static CoroutineRunner GlobalCoroutines { get; } = new();
    public static CoroutineRunner LevelCoroutines { get; private set; } = new();

    public static long Frame { get; private set; }
    public static long GlobalFrame { get; private set; }

    public Main()
    {
        BaseRenderer.ScreenSize = new Point(Consts.ScreenWidthPixels, Consts.ScreenHeightPixels);
        _graphics = BaseRenderer.GetDefaultGraphicsDeviceManager(this);

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

        _graphics.PreferredBackBufferWidth = BaseRenderer.ScreenSize.X * BaseRenderer.PixelScale;
        _graphics.PreferredBackBufferHeight = BaseRenderer.ScreenSize.Y * BaseRenderer.PixelScale;
        BaseRenderer.Initialize(_graphics, GraphicsDevice, Window);

        ContentReloader.Initialize(Content, GraphicsDevice, TargetPlatform.DesktopGL);
        ContentReloader.FileWatcher.IgnoreType = ~AssetType.Texture;

        ContentLoader.Initialize(Content);

        Camera = new();

        Exiting += Game_Exiting;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // TODO: use this.Content to load your game content here
        BaseRenderer.LoadContent();
        Fonts.LoadContent();
        ScreenFade.LoadContent();
    }

    protected override void BeginRun()
    {
        // Actor.Initialize(new Player {
        //     Position = new(0, 50)
        // });

        // Actor.Initialize(new SolidBox {
        //     Hitbox = new(
        //         new FPoint(0, 64),
        //         new FPoint(512, 16)
        //     ),
        //     Depth = 50,
        //     Tag = new((uint)ActorTags.PlayerCollidable)
        // });
        // Actor.Initialize(new SolidBox {
        //     Hitbox = new(
        //         new FPoint(64, 16),
        //         new FPoint(32, 16)
        //     ),
        //     Depth = 50,
        //     Tag = new((uint)ActorTags.PlayerCollidable)
        // });

        // Actor.DoStart();

        LevelLoader.Load("test", 0);
    }

    protected override void Update(GameTime gameTime)
    {
        Input.InputDisabled = !IsActive;

        Input.RefreshKeyboardState();
        Input.RefreshMouseState();
        Input.RefreshGamePadState();

        Input.UpdateTypingInput(gameTime);

        // if(PlayerData.Config.Keybinds.Pause.Pressed)
        //     Exit();

        if(Input.GetPressed(Keys.OemPlus))
            BaseRenderer.PixelScale++;
        if(Input.GetPressed(Keys.OemMinus))
            BaseRenderer.PixelScale--;

        GlobalCoroutines.Update();

        UpdatePausables();

        ScreenFade.Update();

        GlobalFrame++;

        base.Update(gameTime);
    }

    private void UpdatePausables()
    {
        if(Paused) return;

        LevelCoroutines?.Update();

        Actor.DoUpdate();

        Camera.Update();

        Frame++;
    }

    protected override void Draw(GameTime gameTime)
    {
        BaseRenderer.BeginDraw(samplerState: SamplerState.PointClamp, transformMatrix: Camera.Transform, blendState: BlendState.AlphaBlend);

        Actor.DoDraw();

        BaseRenderer.EndDraw();
        BaseRenderer.BeginDrawUI();

        Actor.DoDrawUI();

        ScreenFade.DrawUI();

        BaseRenderer.SpriteBatch.DrawStringSpacesFix(Fonts.Regular, "testing hi", new FPoint(1, 1).ToVector2(), Color.White, 6);

        BaseRenderer.EndDrawUI();
        BaseRenderer.FinalizeDraw();

        base.Draw(gameTime);
    }

    private void Game_Exiting(object sender, ExitingEventArgs e)
    {
        PlayerData.SaveConfig();
    }
}
