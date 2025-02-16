using MageQuest.Graphics;
using Microsoft.Xna.Framework;

namespace MageQuest;

public class InteractableActor : Actor
{
    protected override void Start()
    {
        Collidable = false;
        Size = new(16);
    }

    protected override void Update()
    {
        
    }

    protected override void Draw()
    {
        BaseRenderer.SpriteBatch.Draw(BaseRenderer.PixelTexture, this.Hitbox.ToRectangle(), Color.White);
    }
}
