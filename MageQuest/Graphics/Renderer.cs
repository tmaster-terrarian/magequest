using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MageQuest.Graphics;

public static class Renderer
{
    private static GraphicsDeviceManager _graphics;
    private static RendererState _state = RendererState.Idle;
    private static bool _initialized;
    private static int pixelScale = 3;
    private static bool _dirty;

    private enum RendererState { Idle, BaseDrawBegin, BaseDraw, BaseDrawEnd, UIDrawBegin, UIDraw, UIDrawEnd, Finalizing }

    public static GraphicsDevice GraphicsDevice { get; private set; }
    public static SpriteBatch SpriteBatch { get; private set; }
    public static RenderTarget2D RenderTarget { get; private set; }
    public static RenderTarget2D UIRenderTarget { get; private set; }

    public static GameWindow Window { get; private set; }

    /// <summary>
    /// How large a native pixel is on the client's screen.
    /// </summary>
    public static int PixelScale {
        get => pixelScale;
        set {
            pixelScale = MathHelper.Max(value, 1);
            _dirty = true;
        }
    }

    /// <summary>
    /// The native render resolution.
    /// </summary>
    public static Point ScreenSize { get; set; } = new Point(640, 360);

    /// <summary>
    /// Represents a missing (empty) <see cref="Texture2D"/>.
    /// </summary>
    public static Texture2D EmptyTexture { get; private set; }

    /// <summary>
    /// Represents a single white pixel.
    /// </summary>
    public static Texture2D PixelTexture { get; private set; }

    public static GraphicsDeviceManager GetDefaultGraphicsDeviceManager(Game game) => new(game)
    {
        PreferMultiSampling = false,
        SynchronizeWithVerticalRetrace = true,
        PreferredBackBufferWidth = ScreenSize.X * PixelScale,
        PreferredBackBufferHeight = ScreenSize.Y * PixelScale,
        GraphicsProfile = GraphicsProfile.HiDef,
    };

    private static void CheckInitialized()
    {
        if(!_initialized) throw new InvalidOperationException("Renderer has not been initialized");
    }

    public static void Initialize(GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, GameWindow window)
    {
        if(_initialized) throw new InvalidOperationException("Renderer already initialized");

        _graphics = graphics;
        GraphicsDevice = graphicsDevice;
        Window = window;

        RenderTarget = new RenderTarget2D(GraphicsDevice, ScreenSize.X, ScreenSize.Y);
        UIRenderTarget = new RenderTarget2D(GraphicsDevice, ScreenSize.X, ScreenSize.Y);

        EmptyTexture = new Texture2D(GraphicsDevice, 1, 1);
        EmptyTexture.SetData([Color.Transparent]);

        PixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        PixelTexture.SetData([Color.White]);

        Window.Position = new((GraphicsDevice.DisplayMode.Width - _graphics.PreferredBackBufferWidth) / 2, (GraphicsDevice.DisplayMode.Height - _graphics.PreferredBackBufferHeight) / 2);

        if(GraphicsDevice.DisplayMode.Height == _graphics.PreferredBackBufferHeight)
        {
            Window.Position = Point.Zero;
            Window.IsBorderless = true;
        }

        _graphics.ApplyChanges();

        _initialized = true;
    }

    public static void LoadContent()
    {
        CheckInitialized();

        SpriteBatch = new SpriteBatch(GraphicsDevice);
    }

    public static void BeginDraw(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null)
    {
        CheckInitialized();
        if(_state != RendererState.Idle) throw new InvalidOperationException("Invalid render state");
        _state = RendererState.BaseDrawBegin;

        if(_dirty)
        {
            _graphics.PreferredBackBufferWidth = ScreenSize.X * PixelScale;
            _graphics.PreferredBackBufferHeight = ScreenSize.Y * PixelScale;

            RenderTarget = new RenderTarget2D(GraphicsDevice, ScreenSize.X, ScreenSize.Y);
            UIRenderTarget = new RenderTarget2D(GraphicsDevice, ScreenSize.X, ScreenSize.Y);

            if(GraphicsDevice.DisplayMode.Height == _graphics.PreferredBackBufferHeight)
            {
                Window.Position = Point.Zero;
                Window.IsBorderless = true;
            }
            else if(Window.IsBorderless)
            {
                Window.Position = new((GraphicsDevice.DisplayMode.Width - _graphics.PreferredBackBufferWidth) / 2, (GraphicsDevice.DisplayMode.Height - _graphics.PreferredBackBufferHeight) / 2);
                Window.IsBorderless = false;
            }

            _graphics.ApplyChanges();

            _dirty = false;
        }

        GraphicsDevice.SetRenderTarget(RenderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);

        SpriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);

        _state = RendererState.BaseDraw;
    }

    public static void EndDraw()
    {
        CheckInitialized();
        if(_state != RendererState.BaseDraw) throw new InvalidOperationException("Invalid render state");

        SpriteBatch.End();

        _state = RendererState.BaseDrawEnd;
    }

    public static void BeginDrawUI()
    {
        CheckInitialized();
        if(_state != RendererState.BaseDrawEnd) throw new InvalidOperationException("Invalid render state");
        _state = RendererState.UIDrawBegin;

        GraphicsDevice.SetRenderTarget(UIRenderTarget);
        GraphicsDevice.Clear(Color.Transparent);

        SpriteBatch.Begin(samplerState: SamplerState.PointWrap);

        _state = RendererState.UIDraw;
    }

    public static void EndDrawUI()
    {
        CheckInitialized();
        if(_state != RendererState.UIDraw) throw new InvalidOperationException("Invalid render state");

        SpriteBatch.End();

        _state = RendererState.UIDrawEnd;
    }

    public static void FinalizeDraw()
    {
        CheckInitialized();
        if(_state != RendererState.UIDrawEnd) throw new InvalidOperationException("Invalid render state");
        _state = RendererState.Finalizing;

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        SpriteBatch.Begin(samplerState: SamplerState.PointWrap, blendState: BlendState.Opaque);
        SpriteBatch.Draw(RenderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, PixelScale, SpriteEffects.None, 0);
        SpriteBatch.End();

        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        SpriteBatch.Draw(UIRenderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, PixelScale, SpriteEffects.None, 0);
        SpriteBatch.End();

        _state = RendererState.Idle;
    }
}
