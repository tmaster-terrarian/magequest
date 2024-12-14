using System;
using System.Collections;
using MageQuest.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MageQuest.Actors;

public class Player : Actor
{
    public FPoint Velocity { get => vel; set => vel = value; }

    FPoint vel;

    public const int MaxFallingSpeed = Consts.MaxFallingSpeed;
    public const int Gravity = 0x50;
    public const int JumpingGravity = 0x20;
    public const int JumpingSpeed = 0x500;

    public const int MaxWalkingSpeed = 0x400;
    public const int GroundAccel = 0x55;
    public const int AirAccel = 0x20;
    public const int Fric = 0x33;

    enum Animations { none }

    enum StaffAnimations { none, flutter, eye_blink }

    public int Facing { get; private set; } = 1;

    SpriteEffects SpriteEffects => Facing == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

    Func<Texture2D> mainTexture;
    Func<Texture2D> staffTexture;
    Func<Texture2D> staffEyeTexture;

    FPoint drawOffset;
    int frameX = 0;
    int frameY = 0;
    int staffFrameX = 0;
    int staffFrameY = 0;
    int staffEyeFrameX = 0;
    int staffEyeFrameY = 0;

    Animations anim = Animations.none;
    StaffAnimations staffAnim = StaffAnimations.none;
    StaffAnimations staffEyeAnim = StaffAnimations.none;

    int walkFrameCounter;
    int walkFrame;

    int moveDir;
    int lookDir;

    int camDistanceX = 0;
    int camDistanceY = 0;

    Tag collisionMask = new();

    bool debugDrawCamDeadzone;
    bool debugDrawHitbox;

    protected override void PreStart()
    {
        mainTexture = () => ContentLoader.Load<Texture2D>("graphics/gameplay/entities/player/player");
        staffTexture = () => ContentLoader.Load<Texture2D>("graphics/gameplay/entities/player/staff");
        staffEyeTexture = () => ContentLoader.Load<Texture2D>("graphics/gameplay/entities/player/staff_eye");

        Size = new(8, 14);

        collisionMask = Tag.Add(collisionMask, (uint)ActorTags.PlayerCollidable);

        Main.Camera.TargetPosition = new(
            X + (4 << Consts.SHFT),
            Y + (6 << Consts.SHFT),
            false
        );
        Main.Camera.Position = Main.Camera.TargetPosition;
    }

    protected override void Update()
    {
        CheckDebugKeys();

        int lastLookDir = lookDir;

        moveDir = PlayerData.Config.Keybinds.Right.IsDown.ToInt32() - PlayerData.Config.Keybinds.Left.IsDown.ToInt32();
        lookDir = PlayerData.Config.Keybinds.Down.IsDown.ToInt32() - PlayerData.Config.Keybinds.Up.IsDown.ToInt32();

        OnGround = CheckColliding(0, 1, collisionMask, TagFilter.One);

        if(moveDir == 1)
        {
            Facing = 1;

            vel.X += OnGround ? GroundAccel : AirAccel;

            camDistanceX += (0x6000 - camDistanceX) / 0x38;
        }
        else if(moveDir == -1)
        {
            Facing = -1;

            vel.X -= OnGround ? GroundAccel : AirAccel;

            camDistanceX += (-0x6000 - camDistanceX) / 0x38;
        }

        if(moveDir != 0)
        {
            if(++walkFrameCounter > 6)
            {
                walkFrameCounter = 0;
                if(++walkFrame > 3)
                    walkFrame = 0;
            }
        }
        else
        {
            camDistanceX += -camDistanceX / 0x30;

            walkFrameCounter = 0;
            walkFrame = 0;
        }

        var _lookDir = 3 * (lookDir switch {
            1 => 2,
            -1 => 1,
            _ => 0,
        });

        if(OnGround)
        {
            vel.X = MathUtil.Approach(vel.X, 0, Fric);
            vel.Y = 0;

            frameX = _lookDir + (walkFrame switch {
                1 => 1,
                3 => 2,
                _ => 0,
            });
        }
        else
        {
            walkFrame = vel.Y > 0 ? 1 : 3;

            frameX = _lookDir + (walkFrame switch {
                1 => 1,
                3 => 2,
                _ => 0,
            });

            vel.Y += PlayerData.Config.Keybinds.Jump.IsDown && vel.Y <= 0
                ? JumpingGravity
                : Gravity;
        }

        drawOffset.Y = walkFrame switch {
            1 or 3 => -1,
            _ => 0,
        } << Consts.SHFT;

        if(PlayerData.Config.Keybinds.Jump.Pressed)
        {
            if(OnGround)
            {
                vel.Y = -JumpingSpeed;
            }
        }

        // if(lookDir != 0)
        // {
        //     if(lastLookDir == 0)
        //     {
        //         if(lookDir == 1)
        //         {
        //             camDistanceY = Main.Camera.Deadzone.Height;
        //         }
        //         else
        //         {
        //             camDistanceY = -Main.Camera.Deadzone.Y;
        //         }

        //         camDistanceY += Main.Camera.Position.Y - (Y + 0xC00);
        //     }

        //     camDistanceY += ((lookDir * 0x10000) - camDistanceY) / 0x30;
        // }
        // else
        // {
        //     if(lastLookDir != 0)
        //     {
        //         if(lastLookDir == 1)
        //         {
        //             camDistanceY = -Main.Camera.Deadzone.Y;
        //         }
        //         else
        //         {
        //             camDistanceY = Main.Camera.Deadzone.Height;
        //         }

        //         camDistanceY += Main.Camera.Position.Y - (Y + 0xC00);
        //     }

        //     camDistanceY += -camDistanceY / 0x40;

        //     if(Math.Abs(camDistanceY) > 0x1000)
        //     {
        //         Main.Camera.Position += new FPoint(0, (camDistanceY + Y + 0xC00 - Main.Camera.Position.Y) / 0x40, false);
        //     }
        // }

        vel.Y = MathHelper.Min(vel.Y, MaxFallingSpeed);
        vel.X = MathHelper.Clamp(vel.X, -MaxWalkingSpeed, MaxWalkingSpeed);

        MoveY(vel.Y, () => vel.Y = 0);
        MoveX(vel.X, () => vel.X = 0);

        if(Input.GetPressed(Keys.R))
        {
            Main.GlobalCoroutines.TryRun("fade", ResetPositionAndFade(), out var _);
        }

        Main.Camera.TargetPosition = new(
            X + camDistanceX + 0x800,
            Y + camDistanceY + 0xC00,
            false
        );

        if((Main.Frame % 300 == 0 || Input.GetPressed(Keys.N)) && staffAnim == StaffAnimations.none)
        {
            PlayStaffAnimation(StaffAnimations.flutter, StaffAnimFlutter);
        }

        if((Random.Shared.NextSingle() < 0.002f || Input.GetPressed(Keys.B)) && staffEyeAnim == StaffAnimations.none)
        {
            PlayStaffAnimation(StaffAnimations.eye_blink, StaffAnimBlink);
        }
    }

    protected override bool CheckColliding(int offsetX, int offsetY, Tag matchTags = default, TagFilter filter = TagFilter.NoFiltering)
    {
        return SolidMeeting(Hitbox.Shift(offsetX, offsetY), matchTags, filter);
    }

    private void CheckDebugKeys()
    {
        if(Input.GetPressed(Keys.F2))
        {
            debugDrawCamDeadzone = !debugDrawCamDeadzone;
        }
        if(Input.GetPressed(Keys.F3))
        {
            debugDrawHitbox = !debugDrawHitbox;
        }
    }

    private void PlayAnimation(Animations animId, Func<IEnumerator> func, bool force = false)
    {
        var name = $"playerAnimation_{animId}";
        if(!Main.LevelCoroutines.IsRunning(name) || force)
        {
            if(force)
                Main.LevelCoroutines.Stop(name);

            Main.LevelCoroutines.Run(name, func());
        }
    }

    private void PlayStaffAnimation(StaffAnimations animId, Func<IEnumerator> func, bool force = false)
    {
        var name = $"playerAnimation_staff_{animId}";
        if(!Main.LevelCoroutines.IsRunning(name) || force)
        {
            if(force)
                Main.LevelCoroutines.Stop(name);

            Main.LevelCoroutines.Run(name, func());
        }
    }

    protected override void Draw()
    {
        FPoint texPos = new(4, 6);

        FPoint staffPos = texPos + new FPoint(
            (7 + (walkFrame switch {
                1 => -1,
                3 => 1,
                _ => 0,
            })) * Facing,
            -4 + lookDir
        );

        BaseRenderer.SpriteBatch.Draw(
            mainTexture(),
            (Position + texPos + drawOffset).ToPoint().ToVector2(),
            new Rectangle(frameX * 16, frameY * 16, 16, 16),
            Color.White,
            0,
            new Vector2(8),
            1,
            SpriteEffects,
            0
        );

        BaseRenderer.SpriteBatch.Draw(
            staffTexture(),
            (Position + staffPos + drawOffset).ToPoint().ToVector2(),
            new Rectangle(staffFrameX * 16, staffFrameY * 16, 16, 16),
            Color.White,
            0,
            new Vector2(8),
            1,
            SpriteEffects,
            0
        );
        BaseRenderer.SpriteBatch.Draw(
            staffEyeTexture(),
            (Position + staffPos + drawOffset).ToPoint().ToVector2(),
            new Rectangle(staffEyeFrameX * 16, staffEyeFrameY * 16, 16, 16),
            Color.White,
            0,
            new Vector2(8),
            1,
            SpriteEffects,
            0
        );

        #if DEBUG

        if(debugDrawHitbox)
        {
            BaseRenderer.SpriteBatch.DrawNineSlice(
                BaseRenderer.OutlineTexture,
                Hitbox.ToRectangle(),
                null,
                new Point(1),
                new Point(1),
                Color.Red
            );
        }

        if(debugDrawCamDeadzone)
        {
            BaseRenderer.SpriteBatch.Draw(
                BaseRenderer.PixelTexture,
                new FRectangle(
                    new(
                        Main.Camera.Position.X - Main.Camera.Deadzone.X,
                        Main.Camera.Position.Y - Main.Camera.Deadzone.Y,
                        false
                    ),
                    new(
                        MathHelper.Max(0x200, Main.Camera.Deadzone.X + Main.Camera.Deadzone.Width),
                        MathHelper.Max(0x200, Main.Camera.Deadzone.Y + Main.Camera.Deadzone.Height),
                        false
                    )
                ).ToRectangle(),
                Color.LightPink * 0.5f
            );

            BaseRenderer.SpriteBatch.DrawLine(
                new FPoint(
                    Main.Camera.Position.X,
                    Main.Camera.Position.Y,
                    false
                ).ToVector2(),
                new FPoint(
                    Main.Camera.TargetPosition.X,
                    Main.Camera.TargetPosition.Y,
                    false
                ).ToVector2(),
                Color.Yellow
            );
        }

        #endif
    }

    protected override void DrawUI()
    {
        #if DEBUG

        BaseRenderer.SpriteBatch.DrawStringSpacesFix(
            Fonts.Regular,
            vel.ToString(),
            new FPoint(1, 12).ToVector2(),
            Color.White,
            6
        );
        BaseRenderer.SpriteBatch.DrawStringSpacesFix(
            Fonts.Regular,
            OnGround.ToString(),
            new FPoint(1, 24).ToVector2(),
            Color.White,
            6
        );

        #endif
    }

    IEnumerator StaffAnimBlink()
    {
        const int frameTime = 3;
        staffEyeAnim = StaffAnimations.eye_blink;

        staffEyeFrameX = 1;
        yield return frameTime;
        staffEyeFrameX = 2;
        yield return frameTime;
        staffEyeFrameX = 1;
        yield return frameTime;
        staffEyeFrameX = 0;
        yield return frameTime;

        staffEyeAnim = StaffAnimations.none;
    }

    IEnumerator StaffAnimFlutter()
    {
        const int frameTime = 6;
        staffAnim = StaffAnimations.flutter;

        staffFrameX = 1;
        yield return frameTime;
        staffFrameX = 2;
        yield return frameTime;
        staffFrameX = 3;
        yield return frameTime;
        staffFrameX = 0;
        yield return frameTime;

        staffAnim = StaffAnimations.none;
    }

    IEnumerator ResetPositionAndFade()
    {
        yield return ScreenFade.FadeOut(ScreenFade.TransitionStyles.Diamond);

        ScreenFade.SetState(ScreenFade.TransitionStates.IdleOut);

        Position = new(0, 50);
        Velocity = FPoint.Zero;
        Main.Camera.Position = new(
            X + camDistanceX + 0x800,
            Y + camDistanceY + 0xC00,
            false
        );

        yield return ScreenFade.FadeIn(ScreenFade.TransitionStyles.DiamondInverse);
    }
}
