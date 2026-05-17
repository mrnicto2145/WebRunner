using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WebRunner;

public class Platform
{
    private Texture2D _texture;
    public Rectangle Bounds { get; private set; }

    public Platform(Rectangle bounds)
    {
        Bounds = bounds;
    }

    public void LoadContent(Texture2D texture)
    {
        _texture = texture;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_texture, Bounds, Color.Green);
    }
}
