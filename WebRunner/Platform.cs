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

    public void Draw(SpriteBatch spriteBatch, bool isTop, bool isBackground)
    {
        var currentBounds = Bounds;            
        var color = isBackground ? Color.Gray : Color.Green;
        var texture = _texture;
        if (isTop)
        {
            color = Color.DarkGreen;
            texture = _topTexture;
            currentBounds = new Rectangle(Bounds.X, 475 - Bounds.Y - Bounds.Height, Bounds.Width, Bounds.Height);
        }
        if (isBackground)
        {
            currentBounds = new Rectangle(Bounds.X, 55, Bounds.Width, 365);
        }
        spriteBatch.Draw(texture, currentBounds, color);
    }
}
