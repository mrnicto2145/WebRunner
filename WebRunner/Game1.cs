using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.DirectWrite;

namespace WebRunner;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Player _player;
    private List<Level> _levels;
    private int _levelNum;
    private Camera _camera;
    private float _levelWidth = 2000;   // ширина текущего уровня (вычислите из платформ или задайте)
    private float _levelHeight = 480;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _levelNum = 0;
    }

    protected override void Initialize()
    {
        //player
        _player = new Player(new Vector2(100, 100));
        _levels = LoadLevels();
        _camera = new Camera(GraphicsDevice.Viewport, _levelWidth, _levelHeight);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        var pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        pixelTexture.SetData(new[] { Color.White });
        _player.LoadContent(pixelTexture);
        foreach (var p in _levels)
            p.LoadContent(pixelTexture);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _player.Update(gameTime, _levels, 0);
        _camera.Follow(_player.Position);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(transformMatrix: _camera.GetTransformMatrix());
        _player.Draw(_spriteBatch);
        foreach (var p in _levels[_levelNum].Platforms)
            p.Draw(_spriteBatch);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private List<Level> LoadLevels()
    {
        var p = new Platform[][]
        {
            new Platform[]
            {
                new Platform(new Rectangle(0, 400, 80000, 20)),
                new Platform(new Rectangle(200, 350, 10000, 20)),
                new Platform(new Rectangle(500, 300, 80000, 20))
            },
            new Platform[]
            {
                new Platform(new Rectangle(0, 400, 80000, 20)),
            },
            new Platform[]
            {
                new Platform(new Rectangle(0, 400, 80000, 20)),
                new Platform(new Rectangle(200, 300, 10000, 20)),
                new Platform(new Rectangle(500, 350, 80000, 20))
            },
            new Platform[]
            {
                new Platform(new Rectangle(0, 400, 80000, 20)),
            }
        };
        return new List<Level>() { new Level(p) };
    }
}
