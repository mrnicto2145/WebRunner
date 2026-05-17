using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace WebRunner;

public class Level
{
    private Platform[][] _platforms;
    private int _floor;
    public Platform[] Platforms => _platforms[_floor];

    public Level(Platform[][] platforms)
    {
        _platforms = platforms;
    }

    public void LoadContent(Texture2D texture)
    {
        for (var i = 0; i<4; i++)
            foreach (var platform in _platforms[i])
            {
                platform.LoadContent(texture);
            }
    }

    public void switchFloor(int c)
    {
        if (_floor + c < 0)
            _floor += 4;
        _floor = (_floor + c) % 4;
    }
}