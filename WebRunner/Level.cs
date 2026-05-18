using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace WebRunner;

public class Level
{
    private Platform[][] _platforms;
    public int Length => _platforms.Length;

    public Level(Platform[][] platforms)
    {
        _platforms = platforms;
    }

    public void LoadContent(Texture2D texture, Texture2D topTexture)
    {
        for (var i = 0; i<4; i++)
            foreach (var platform in _platforms[i])
                platform.LoadContent(texture, topTexture);
    }

    public Platform[] GetPlatforms(int i) => _platforms[i];
}