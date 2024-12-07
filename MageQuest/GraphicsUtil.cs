using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MageQuest;

public static class GraphicsUtil
{
    public static Rectangle GetFrameInStrip(Texture2D texture, int currentFrame, int totalFrames)
    {
        int width = texture.Width / totalFrames;
        return new(currentFrame * width, 0, width, texture.Height);
    }

    public static Rectangle GetFrameInStrip(Texture2D texture, int currentFrame, int totalFramesX, int totalFramesY)
    {
        int width = texture.Width / totalFramesX;
        int height = texture.Height / totalFramesY;
        return new(
            currentFrame % totalFramesX * width,
            currentFrame / totalFramesX * height,
            width,
            height
        );
    }
}
