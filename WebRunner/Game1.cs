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
    private LevelManager _levelManager;
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
        _levelManager = new LevelManager(_levels[_levelNum], 800f, 1200f);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        var pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        var topTexture = new Texture2D(GraphicsDevice, 1, 1);
        pixelTexture.SetData(new[] { Color.White });
        topTexture.SetData(new[] { Color.White });
        _player.LoadContent(pixelTexture);
        _levelManager.LoadContent(pixelTexture, topTexture);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        _levelManager.Update(_player.Position);
        _player.Update(gameTime, _levelManager, true);
        _camera.Follow(_player.Position);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(transformMatrix: _camera.GetTransformMatrix());
        _levelManager.DrawLevel(_spriteBatch);
        _player.Draw(_spriteBatch);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private List<Level> LoadLevels()
    {
        var p = new Platform[][]
        {
            new Platform[]
            {
                new Platform(new Rectangle(200, 350, 100, 20)),
                new Platform(new Rectangle(500, 300, 800, 20))
            },
            new Platform[]
            {
                new Platform(new Rectangle(200, 400, 100, 20)),
            },
            new Platform[]
            {
                new Platform(new Rectangle(200, 300, 100, 20)),
                new Platform(new Rectangle(500, 350, 800, 20))
            },
            new Platform[]
            {
                new Platform(new Rectangle(200, 100, 100, 20)),
            }
        };
        return new List<Level>() { new Level(p) };
    }
}
