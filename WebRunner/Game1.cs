using System;
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
    private RenderTarget2D renderTarget;
    private Player _player;
    private List<Level> _levels;
    private int _levelNum;
    private Camera _camera;
    private Rectangle _destinationRectangle;
    private LevelManager _levelManager;
    private float _levelWidth = 200000;   // ширина текущего уровня (вычислите из платформ или задайте)
    private float _levelHeight = 480;
    private const int gameWidth = 800;
    private const int gameHeight = 480;
    private SpriteFont _font;
    private bool _debug;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _levelNum = 0;
        _debug = false;

        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnClientSizeChanged;
    }

    protected override void Initialize()
    {
        renderTarget = new RenderTarget2D(GraphicsDevice, gameWidth, gameHeight);
        //player
        _player = new Player(new Vector2(100, 100));
        _levels = LoadLevels();
        _camera = new Camera(gameWidth, gameHeight, _levelWidth, _levelHeight);
        _levelManager = new LevelManager(_levels[_levelNum], 800f, 1200f);
        UpdateDestinationRectangle();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        var pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        var topTexture = new Texture2D(GraphicsDevice, 1, 1);
        pixelTexture.SetData(new[] { Color.White });
        topTexture.SetData(new[] { Color.White });
        _font = Content.Load<SpriteFont>("HomeVideo");
        _player.LoadContent(pixelTexture);
        _levelManager.LoadContent(pixelTexture, topTexture);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _levelManager.Update(_player.Position, gameTime);

        foreach (var trap in _levelManager.GetCurrentTraps())
        {
            trap.TryDamage(_player);
        }

        _player.Update(gameTime, _levelManager, _debug);
        _camera.Follow(_player.Position);

        base.Update(gameTime);
    }

    private void OnClientSizeChanged(object sender, EventArgs e)
    {
        UpdateDestinationRectangle();
    }

    private void UpdateDestinationRectangle()
    {
        var viewport = GraphicsDevice.Viewport;
        float targetAspect = (float)gameWidth / gameHeight;
        float windowAspect = (float)viewport.Width / viewport.Height;
        
        int width, height;
        if (windowAspect > targetAspect)
        {
            // Окно шире — ограничение по высоте
            height = viewport.Height;
            width = (int)(height * targetAspect);
        }
        else
        {
            // Окно уже — ограничение по ширине
            width = viewport.Width;
            height = (int)(width / targetAspect);
        }
        
        int x = (viewport.Width - width) / 2;
        int y = (viewport.Height - height) / 2;
        _destinationRectangle = new Rectangle(x, y, width, height);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(renderTarget);
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(transformMatrix: _camera.GetTransformMatrix());
        _levelManager.DrawLevel(_spriteBatch, _font, _debug);
        _player.Draw(_spriteBatch, _font, _debug);
        if (_font != null)
        {
            var pos = _camera.GetCameraPosition();
            if (!_debug){
                _spriteBatch.DrawString(_font, $"HP: {_player.Health}", new Vector2(pos.X + 10, pos.Y + 10), Color.White);
                _spriteBatch.DrawString(_font, $"X: {GraphicsDevice.Viewport.Width} Y: {GraphicsDevice.Viewport.Height}", new Vector2(pos.X + 10, pos.Y + 30), Color.White);
            }
            else
            {
                _spriteBatch.DrawString(_font, $"Xpos: {_player.Position.X} \nYpos: {_player.Position.Y}" , new Vector2(pos.X + 10, pos.Y + 30), Color.White);
            }
        }

        _spriteBatch.End();


        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp); 
        _spriteBatch.Draw(renderTarget, _destinationRectangle, Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private List<Level> LoadLevels()
    {
        var p = new Platform[][]
        {
            new Platform[]
            {
                new Platform(new Rectangle(200, 351, 100, 20)),
                new Platform(new Rectangle(500, 300, 800, 20))
            },
            new Platform[]
            {

            },
            new Platform[]
            {
                new Platform(new Rectangle(200, 350, 100, 20)),
                new Platform(new Rectangle(500, 300, 800, 20))
            },
            new Platform[]
            {

            }
        };
        var t= new Hitbox[][]
        {
            new Hitbox[]
            {
                new Hitbox(new Rectangle(300,350,100,20), 1, 1),
            },
            new Hitbox[]
            {
                
            },
            new Hitbox[]
            {
                
            },
            new Hitbox[]
            {
                
            }
        };
        return new List<Level>() { new Level(p, t) };
    }
}
