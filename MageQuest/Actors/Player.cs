using MageQuest.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

    public int Facing { get; private set; } = 1;

    SpriteEffects SpriteEffects => Facing == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

    Texture2D _mainTexture;
    Point drawOffset;
    int frameX;
    int frameY;
    int lookDir;

    int walkFrameCounter;
    int walkFrame;
    int moveDir;

    int camDistanceX;

    Tag collisionMask = new();

    protected override void PreStart()
    {
        _mainTexture = ContentLoader.Load<Texture2D>("graphics/gameplay/entities/player/player");

        Hitbox = new(0, 0, 8, 14);

        collisionMask = Tag.Add(collisionMask, (uint)ActorTags.PlayerCollidable);

        Main.Logger.LogInfo(collisionMask);
    }

    protected override void Update()
    {
        Main.Camera.TargetPosition = new(
            camDistanceX + 4,
            6,
            false
        );

        moveDir = PlayerData.Config.Keybinds.Right.IsDown.ToInt32() - PlayerData.Config.Keybinds.Left.IsDown.ToInt32();
        lookDir = PlayerData.Config.Keybinds.Down.IsDown.ToInt32() - PlayerData.Config.Keybinds.Up.IsDown.ToInt32();

        OnGround = CheckColliding(0, 1, collisionMask, TagFilter.One);

        drawOffset = Point.Zero;

        if(moveDir == 1)
        {
            Facing = 1;

            vel.X += OnGround ? GroundAccel : AirAccel;

            camDistanceX = MathHelper.Min(camDistanceX + 0x180, 0x6000);
        }
        else if(moveDir == -1)
        {
            Facing = -1;

            vel.X -= OnGround ? GroundAccel : AirAccel;

            camDistanceX = MathHelper.Max(camDistanceX - 0x180, -0x6000);
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
            camDistanceX = MathUtil.Approach(camDistanceX, 0, 0x80);

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
            drawOffset.Y = walkFrame switch {
                1 or 3 => -1,
                _ => 0,
            };

            if(PlayerData.Config.Keybinds.Jump.Pressed)
            {
                vel.Y = -JumpingSpeed;
            }
        }
        else
        {
            frameX = _lookDir;

            vel.Y += PlayerData.Config.Keybinds.Jump.IsDown && vel.Y <= 0
                ? JumpingGravity
                : Gravity;
        }

        vel.Y = MathHelper.Min(vel.Y, MaxFallingSpeed);
        vel.X = MathHelper.Clamp(vel.X, -MaxWalkingSpeed, MaxWalkingSpeed);

        Main.Camera.WorldOrigin = -Hitbox.Location;

        MoveY(vel.Y, () => vel.Y = 0);
        MoveX(vel.X, () => vel.X = 0);
    }

    protected override bool CheckColliding(int offsetX, int offsetY, Tag matchTags = default, TagFilter filter = TagFilter.NoFiltering)
    {
        return SolidMeeting(Hitbox.Shift(offsetX, offsetY), matchTags, filter);
    }

    protected override void Draw()
    {
        BaseRenderer.SpriteBatch.Draw(
            _mainTexture,
            (Hitbox.Location.ToPoint() + new Point(4, 6) + drawOffset).ToVector2(),
            new Rectangle(frameX * 16, frameY * 16, 16, 16),
            Color.White,
            0,
            new Vector2(8),
            1,
            SpriteEffects,
            0
        );

        // BaseRenderer.SpriteBatch.DrawNineSlice(
        //     BaseRenderer.OutlineTexture,
        //     Hitbox.ToRectangle(),
        //     null,
        //     new Point(1),
        //     new Point(1),
        //     Color.Red
        // );

        // BaseRenderer.SpriteBatch.Draw(
        //     BaseRenderer.PixelTexture,
        //     Hitbox.Location.ToPoint().ToVector2(),
        //     Color.Yellow
        // );
    }

    protected override void DrawUI()
    {
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
    }
}
