﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using MageQuest.Graphics;
using MageQuest.IO;
using DebugRenderers = MageQuest.Graphics.DebugRenderers;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGameReload.Assets;

namespace MageQuest;

public class Main : Game
{
    private static GraphicsDeviceManager _graphics;

    public static Camera Camera { get; private set; }

    public static bool Paused { get; set; }

    public static Logger Logger { get; set; } = new("main");

    public static CoroutineRunner GlobalCoroutines { get; } = new();
    public static CoroutineRunner Coroutines { get; private set; } = new();

    public static long Frame { get; private set; }
    public static long GlobalFrame { get; private set; }
    public static long DrawCalls { get; private set; }

    public static List<DebugRenderer> DebugRenderers { get; private set; } =
    [
        new DebugRenderers.FpsMonitorRenderer(targetFramerate: 50),
    ];

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
            Logger.LogError($"failed to initialize audio: {e}");
        }

        Window.Title = "Mage Quest!";

        TargetElapsedTime = TimeSpan.FromMicroseconds(20000);
    }

    protected override void Initialize()
    {
        Logger.LogInfo("entering main loop");

        {
            bool configExists = File.Exists(Path.Combine(FileLocations.ProgramPath, "config.ini"));

            PlayerData.ReadConfig();

            if(!configExists)
            {
                Logger.LogInfo("regenerating config file");
                PlayerData.SaveConfig();
            }
        }

        PlayerSave.Initialize();

        _graphics.PreferredBackBufferWidth = BaseRenderer.ScreenSize.X * BaseRenderer.PixelScale;
        _graphics.PreferredBackBufferHeight = BaseRenderer.ScreenSize.Y * BaseRenderer.PixelScale;
        BaseRenderer.Initialize(_graphics, GraphicsDevice, Window);

        ContentReloader.Initialize(Content, GraphicsDevice, TargetPlatform.DesktopGL);
        ContentReloader.FileWatcher.IgnoreType = ~AssetType.Texture;

        ContentLoader.Initialize(Content);

        Camera = new();

        Exiting += Game_Exiting;

        ImGuiRenderer.Initialize(this);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // TODO: use this.Content to load your game content here
        BaseRenderer.LoadContent();
        Fonts.LoadContent();
        ScreenFade.LoadContent();

        foreach(var r in DebugRenderers)
        {
            r.Init();
        }
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

        PlayerSave.Load(0);

        if(File.Exists(Path.Combine(FileLocations.Data, "levels", "base", $"{PlayerSave.Current.Level}.ldtkl")))
        {
            LevelLoader.Load(PlayerSave.Current.Level);

            if(PlayerSave.Current.Position != FPoint.Zero)
            {
                var l = Actor.GetAll<Actors.Player>();
                if(l.Count != 0)
                {
                    l[0].Position = PlayerSave.Current.Position;
                }
            }
        }
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

        if(Input.GetPressed(Keys.R))
            GlobalCoroutines.TryRun("fade", ResetLevel(), out var _);

        GlobalCoroutines.Update();

        UpdatePausables();

        ScreenFade.Update();

        GlobalFrame++;

        base.Update(gameTime);
    }

    private void UpdatePausables()
    {
        if(Paused) return;

        Coroutines?.Update();

        if(LevelLoader.ActiveLevel != null)
            Actor.DoUpdate();

        Camera.Update();

        Frame++;
    }

    protected override void Draw(GameTime gameTime)
    {
        BaseRenderer.BeginDraw(samplerState: SamplerState.PointClamp, transformMatrix: Camera.Transform, blendState: BlendState.AlphaBlend);

        Actor.DoDraw();

        foreach(var r in DebugRenderers)
        {
            r.Draw(BaseRenderer.SpriteBatch, gameTime);
        }

        BaseRenderer.EndDraw();
        BaseRenderer.BeginDrawUI();

        Actor.DoDrawUI();

        foreach(var r in DebugRenderers)
        {
            r.DrawUI(BaseRenderer.SpriteBatch, gameTime);
        }

        ScreenFade.DrawUI();

        BaseRenderer.SpriteBatch.DrawStringSpacesFix(Fonts.Regular, "testing hi", new FPoint(1, 1).ToVector2(), Color.White, 6);

        BaseRenderer.EndDrawUI();
        BaseRenderer.FinalizeDraw();

        base.Draw(gameTime);

        foreach(var r in DebugRenderers)
        {
            r.PostDraw();
        }

        ImGuiRenderer.Draw(gameTime);

        DrawCalls++;
    }

    private void Game_Exiting(object sender, ExitingEventArgs e)
    {
        PlayerData.SaveConfig();
    }

    static IEnumerator ResetLevel()
    {
        var lvl = LevelLoader.ActiveLevel;
        var ent = LevelLoader.CurrentEntrance;

        yield return ScreenFade.FadeOut(ScreenFade.TransitionStyles.Diamond);
        ScreenFade.SetState(ScreenFade.TransitionStates.IdleOut);

        Coroutines.StopAll();
        Coroutines = new();

        LevelLoader.Load(lvl, ent);

        yield return ScreenFade.FadeIn(ScreenFade.TransitionStyles.DiamondInverse);
    }
}
