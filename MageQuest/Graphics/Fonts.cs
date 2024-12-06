using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MageQuest.Graphics;

public static class Fonts
{
    private static SpriteFont _regular;
    private static SpriteFont _bold;
    private static SpriteFont _italic;

    public static SpriteFont Regular => _regular;
    public static SpriteFont Bold => _bold;
    public static SpriteFont Italic => _italic;

    public static void LoadContent()
    {
        _regular = ContentLoader.Load<SpriteFont>("fonts/default");
        _bold = ContentLoader.Load<SpriteFont>("fonts/defaultBold");
        _italic = ContentLoader.Load<SpriteFont>("fonts/defaultItalic");
    }
}
