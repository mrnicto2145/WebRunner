using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WebRunner;

public class Platform
{
    private Texture2D _texture;
    private Texture2D _topTexture;
    public Rectangle Bounds { get; private set; }

    public Platform(Rectangle bounds)
    {
        Bounds = bounds;
    }

    public void ChangeBounds(Rectangle newBounds)
    {
        Bounds = newBounds;
    }

    public void LoadContent(Texture2D texture, Texture2D topTexture)
    {
        _texture = texture;
        _topTexture = topTexture;
    }

    public void Draw(SpriteBatch spriteBatch, int xOffcet, int yOffcet, bool isTop, bool isBackground)
    {
        var currentBounds = new Rectangle(Bounds.X + xOffcet, Bounds.Y + yOffcet, Bounds.Width, Bounds.Height);
        var color = isBackground ? Color.Gray : Color.Green;
        var texture = _texture;
        if (isTop)
        {
            color = Color.DarkGreen;
            texture = _topTexture;
        }
        spriteBatch.Draw(texture, Bounds, color);
    }
}
