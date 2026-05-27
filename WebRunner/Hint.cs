using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WebRunner;
public class Hint
{
    private string _text;
    private Vector2 _Location;
    private Color _color;
    private SpriteFont _font;

    public Hint(string text, int xlocation, int yLocation)
    {
        _text = text;
        _Location = new Vector2(xlocation, yLocation);
    }

    public void LoadContent(Color color)
    {
        _color = color;
    }

    public void Draw(SpriteBatch spriteBatch, SpriteFont font)
    {
        spriteBatch.DrawString(font,_text,_Location, _color);
    }
}