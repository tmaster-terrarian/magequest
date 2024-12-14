using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MageQuest;

public class Solid : Actor
{
    private bool _isCarriable = true;

    public override void MoveX(int move, Action onCollide)
    {
        Move(move, 0);
    }

    public override void MoveY(int move, Action onCollide)
    {
        Move(0, move);
    }

    public virtual void Move(int moveX, int moveY)
    {
        if(moveY != 0 || moveX != 0)
        {
            // Loop through every Actor in the Level, add it to
            // a list if actor.IsRiding(this) is true
            var allActors = GetAll<Actor>();

            var riding =
                from actor in allActors
                where actor.IsRiding(this)
                select actor;

            // Make this Solid non-collidable for Actors,
            // so that Actors moved by it do not get stuck on it
            Collidable = false;
            _isCarriable = false;

            // y
            if(moveY != 0)
            {
                Y += moveY;
                if(moveY > 0)
                {
                    foreach(var actor in allActors)
                    {
                        if(actor is Solid solid && !solid._isCarriable)
                            continue;

                        if(this.IsCollidingWith(actor.Hitbox))
                        {
                            // Push down
                            actor.MoveY(this.Hitbox.Bottom - actor.Hitbox.Top, actor.Squished);
                        }
                        else if(riding.Contains(actor))
                        {
                            // Carry down
                            actor.MoveY(moveY, null);
                        }
                    }
                }
                else
                {
                    foreach(var actor in allActors)
                    {
                        if(actor is Solid solid && !solid._isCarriable)
                            continue;

                        if(this.IsCollidingWith(actor.Hitbox))
                        {
                            // Push up
                            actor.MoveY(this.Hitbox.Top - actor.Hitbox.Bottom, actor.Squished);
                        }
                        else if(riding.Contains(actor))
                        {
                            // Carry up
                            actor.MoveY(moveY, null);
                        }
                    }
                }
            }

            // x
            if(moveX != 0)
            {
                X += moveX;
                if(moveX > 0)
                {
                    foreach(var actor in allActors)
                    {
                        if(actor is Solid solid && !solid._isCarriable)
                            continue;

                        if(this.IsCollidingWith(actor.Hitbox))
                        {
                            // Push right
                            actor.MoveX(this.Hitbox.Right - actor.Hitbox.Left, actor.Squished);
                        }
                        else if(riding.Contains(actor))
                        {
                            // Carry right
                            actor.MoveX(moveX, null);
                        }
                    }
                }
                else
                {
                    foreach(var actor in allActors)
                    {
                        if(actor is Solid solid && !solid._isCarriable)
                            continue;

                        if(this.IsCollidingWith(actor.Hitbox))
                        {
                            // Push left
                            actor.MoveX(this.Hitbox.Left - actor.Hitbox.Right, actor.Squished);
                        }
                        else if(riding.Contains(actor))
                        {
                            // Carry left
                            actor.MoveX(moveX, null);
                        }
                    }
                }
            }

            // Re-enable collisions for this Solid
            Collidable = true;
            _isCarriable = true;
        }
    }
}
