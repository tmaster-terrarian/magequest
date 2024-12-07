using Microsoft.Xna.Framework.Graphics;

namespace MageQuest.Actors;

public class Player : Actor
{
    public FPoint Position { get; set; }
    public FPoint Velocity { get; set; }

    public const int MaxFallingSpeed = Constants.MaxFallingSpeed;
    public const int Gravity = 0x50;
    public const int JumpingGravity = 0x20;
    public const int JumpingSpeed = 0x500;

    public const int MaxWalkingSpeed = 0x32C;
    public const int GroundAccel = 0x55;
    public const int AirAccel = 0x20;
    public const int Fric = 0x33;

    private Texture2D _mainTexture;

    protected override void Start()
    {
        _mainTexture = ContentLoader.Load<Texture2D>("graphics/gameplay/entities/player/player");
    }

    protected override void Update()
    {
        
    }
}
