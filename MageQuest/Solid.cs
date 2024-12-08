using Microsoft.Xna.Framework;

namespace MageQuest;

public class Solid : Actor
{
    public virtual bool IsColliding(FRectangle rectangle)
    {
        return false;
    }
}
