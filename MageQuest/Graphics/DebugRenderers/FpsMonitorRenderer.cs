using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MageQuest.Graphics.DebugRenderers;

public class FpsMonitorRenderer(int targetFramerate) : DebugRenderer
{
    private readonly List<TimeSpan> _frameTimes = [];

    private double _avg = 1000;

    public override void DrawUI(SpriteBatch spriteBatch, GameTime gameTime)
    {
        _frameTimes.Add(gameTime.ElapsedGameTime);
        while(_frameTimes.Count >= targetFramerate)
        {
            _avg = _frameTimes.Average(t => t.TotalSeconds);
            _frameTimes.Clear();
        }

        spriteBatch.DrawStringSpacesFix(
            Fonts.Regular,
            $"FPS: {(1 / _avg):F2}",
            new Vector2(BaseRenderer.ScreenSize.Y - 12, 2),
            Color.White, 6
        );
    }
}
