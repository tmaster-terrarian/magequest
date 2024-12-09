using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MageQuest.Graphics;

public static class ScreenFade
{
    private static Func<Texture2D> texture;

    public const int MaxFade = 60;

    public static int FadeProgress { get; set; }

    public static TransitionStates TransitionState { get; private set; }

    public enum TransitionStates
    {
        Idle,
        FadeIn,
        FadeOut,
        IdleIn,
        IdleOut,
    }

    public static void LoadContent()
    {
        texture = () => ContentLoader.Load<Texture2D>("graphics/ui/fade");
    }

    public static void SetState(TransitionStates state)
    {
        if(TransitionState != state)
        {
            TransitionState = state;
            FadeProgress = 0;
        }
    }

    public static void Update()
    {
        switch(TransitionState)
        {
            case TransitionStates.FadeIn:
            {
                FadeProgress = MathHelper.Min(FadeProgress + 1, MaxFade);

                if(FadeProgress == MaxFade)
                    TransitionState = TransitionStates.IdleIn;

                break;
            }
            case TransitionStates.FadeOut:
            {
                FadeProgress = MathHelper.Min(FadeProgress + 1, MaxFade);

                if(FadeProgress == MaxFade)
                    TransitionState = TransitionStates.IdleOut;

                break;
            }
        }
    }

    public static void DrawUI()
    {
        switch(TransitionState)
        {
            case TransitionStates.IdleOut:
            {
                BaseRenderer.SpriteBatch.Draw(
                    BaseRenderer.PixelTexture,
                    new Rectangle(Point.Zero, BaseRenderer.ScreenSize),
                    ColorUtil.CreateFromHex(0x000020)
                );
                break;
            }
            case TransitionStates.FadeOut:
            {
                var frame = FadeProgress;

                for(int y = 0; y < Consts.ScreenHeightPixels / 16; y++)
                {
                    for(int x = 0; x < Consts.ScreenWidthPixels / 16; x++)
                    {
                        BaseRenderer.SpriteBatch.Draw(
                            texture(),
                            new Vector2(x * 16, y * 16),
                            new Rectangle(MathHelper.Min(MathHelper.Max(0, (frame/2) - y) * 16, 240), 0, 16, 16),
                            Color.White
                        );
                    }
                }

                break;
            }
            case TransitionStates.FadeIn:
            {
                var frame = FadeProgress;

                for(int y = 0; y < Consts.ScreenHeightPixels / 16; y++)
                {
                    for(int x = 0; x < Consts.ScreenWidthPixels / 16; x++)
                    {
                        BaseRenderer.SpriteBatch.Draw(
                            texture(),
                            new Vector2(x * 16, y * 16),
                            new Rectangle(MathHelper.Min(MathHelper.Max(0, (MaxFade/2) - (frame/2) - ((Consts.ScreenHeightPixels/16) - y)) * 16, 240), 0, 16, 16),
                            Color.White
                        );
                    }
                }

                break;
            }
        }
    }

    public static IEnumerator FadeInOut()
    {
        SetState(TransitionStates.FadeOut);

        while(TransitionState != TransitionStates.IdleOut)
        {
            yield return null;
        }

        SetState(TransitionStates.FadeIn);

        while(TransitionState != TransitionStates.IdleIn)
        {
            yield return null;
        }

        SetState(TransitionStates.Idle);
    }

    public static IEnumerator FadeOut()
    {
        SetState(TransitionStates.FadeOut);

        while(TransitionState != TransitionStates.IdleOut)
        {
            yield return null;
        }

        SetState(TransitionStates.Idle);
    }

    public static IEnumerator FadeIn()
    {
        SetState(TransitionStates.FadeIn);

        while(TransitionState != TransitionStates.IdleIn)
        {
            yield return null;
        }

        SetState(TransitionStates.Idle);
    }
}
