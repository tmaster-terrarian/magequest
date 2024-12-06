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

    public static void LoadContent(ContentManager content)
    {
        _regular = content.Load<SpriteFont>("fonts/default");
        _bold = content.Load<SpriteFont>("fonts/defaultBold");
        _italic = content.Load<SpriteFont>("fonts/defaultItalic");

        _regular.DefaultCharacter = '\x10';
        _bold.DefaultCharacter = '\x10';
        _italic.DefaultCharacter = '\x10';
    }
}
