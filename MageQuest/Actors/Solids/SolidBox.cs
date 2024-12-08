using MageQuest.Graphics;
using Microsoft.Xna.Framework;

namespace MageQuest.Actors;

public class SolidBox : Solid
{
    protected override void Draw()
    {
        const int cellSize = 8;

        int cellsY = Hitbox.HeightPixels / cellSize;
        int cellsX = Hitbox.WidthPixels / cellSize;

        Rectangle destRect = new(Hitbox.XPixels, Hitbox.YPixels, cellSize, cellSize);

        int yExtra = (Hitbox.HeightPixels > cellsY * cellSize).ToInt32();
        int xExtra = (Hitbox.WidthPixels > cellsX * cellSize).ToInt32();

        Color colorDark = ColorUtil.CreateFromHex(0x24213d);
        Color colorLight = ColorUtil.CreateFromHex(0x76428a);

        bool alternator = true;
        for(int y = 0; y < cellsY + yExtra; y++)
        {
            destRect.Y = Hitbox.YPixels + y * cellSize;
            destRect.Height = (y == cellsY) ? Hitbox.HeightPixels - (cellsY * cellSize) : cellSize;

            for(int x = 0; x < cellsX + xExtra; x++)
            {
                destRect.X = Hitbox.XPixels + x * cellSize;
                destRect.Width = (x == cellsX) ? Hitbox.WidthPixels - (cellsX * cellSize) : cellSize;

                bool lightCell = x % 2 == alternator.ToInt32();

                BaseRenderer.SpriteBatch.Draw(BaseRenderer.PixelTexture, destRect, lightCell ? colorDark : colorLight);
            }

            alternator = !alternator;
        }

        BaseRenderer.SpriteBatch.DrawNineSlice(
            BaseRenderer.OutlineTexture,
            Hitbox.ToRectangle(),
            null,
            new Point(1),
            new Point(1),
            Color.Red
        );
    }

    public override bool IsColliding(FRectangle rectangle)
    {
        return rectangle.Intersects(Hitbox);
    }
}
