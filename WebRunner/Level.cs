using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace WebRunner;

public class Level
{
    private Platform[][] _platforms;
    private Hitbox[][] _traps;
    private Hint[] _hints;
    public int Length => _platforms.Length;

    public Level(Platform[][] platforms, Hitbox[][] hitboxes = null, Hint[] hints = null)
    {
        _platforms = platforms;
        _traps = hitboxes;
        _hints = hints;
    }

    public void LoadContent(Texture2D platformTexture, Texture2D topTexture, Texture2D trapTexture, SpriteFont font, Color color)
    {
        for (var i = 0; i<4; i++){
            foreach (var platform in _platforms[i])
                platform.LoadContent(platformTexture, topTexture);
            foreach (var trap in _traps[i])
                trap.LoadContent(trapTexture);
        }
        foreach (var hint in _hints)
        {
            hint.LoadContent(color);
        }
    }

    public Platform[] GetPlatforms(int i) => _platforms[i];
    public Hitbox[] GetTraps(int i) => _traps[i];

    public Hint[] GetHints() => _hints;
}