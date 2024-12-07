using Microsoft.Xna.Framework;

namespace MageQuest.Actors;

public class Solid : Actor
{
    public int SolidID { get; set; }

    public virtual bool IsColliding(Rectangle rectangle)
    {
        return false;
    }
}
