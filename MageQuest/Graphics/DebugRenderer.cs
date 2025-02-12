using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MageQuest.Graphics;

public abstract class DebugRenderer
{
    public virtual void Init() { }

    public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime) { }

    public virtual void DrawUI(SpriteBatch spriteBatch, GameTime gameTime) { }

    public virtual void PostDraw() { }
}
