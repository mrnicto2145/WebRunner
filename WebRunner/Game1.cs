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
    public static bool _debug;
    private enum GameState { MainMenu, Playing, Paused }
    private GameState _gameState = GameState.MainMenu;
    private MenuManager _menuManager;
    private bool _prevState;
    public Texture2D _defaultTexture;

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
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _menuManager = new MenuManager(GraphicsDevice, _spriteBatch);
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
        
        var pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        var topTexture = new Texture2D(GraphicsDevice, 1, 1);
        _defaultTexture = new Texture2D(GraphicsDevice, 1, 1);
        _defaultTexture.SetData(new[] { Color.White });
        pixelTexture.SetData(new[] { Color.White });
        topTexture.SetData(new[] { Color.White });
        _font = Content.Load<SpriteFont>("HomeVideo");
        _player.LoadContent(pixelTexture);
        _levelManager.LoadContent(pixelTexture, topTexture, pixelTexture);
        _menuManager.LoadContent(_font);
    }

    protected override void Update(GameTime gameTime)
    {
        var prevState = GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape);
        if (prevState && !_prevState)
        {
            if (_gameState == GameState.Playing)
                _gameState = GameState.Paused;
            else if (_gameState == GameState.Paused)
                _gameState = GameState.Playing;
            else if (_gameState == GameState.MainMenu)
                Exit();
        }
        _prevState = prevState;

        switch (_gameState)
        {
            case GameState.MainMenu:
                var action = _menuManager.Update(false, gameTime);
                HandleMenuAction(action);
                break;
            case GameState.Playing:
                // обычная игровая логика
                _levelManager.Update(_player.Position, gameTime);
                foreach (var trap in _levelManager.GetCurrentTraps())
                    trap.TryDamage(_player);
                _player.Update(gameTime, _levelManager, _debug);
                _camera.Follow(_player.Position);
                break;
            case GameState.Paused:
                var pauseAction = _menuManager.Update(true, gameTime);
                HandleMenuAction(pauseAction);
                break;
        }

        base.Update(gameTime);
    }

    private void HandleMenuAction(MenuAction action)
    {
        switch (action)
        {
            case MenuAction.StartGame:
                _gameState = GameState.Playing;
                // сбросить игру (заново инициализировать игровые объекты)
                ResetGame();
                break;
            case MenuAction.ExitGame:
                Exit();
                break;
            case MenuAction.Resume:
                _gameState = GameState.Playing;
                break;
            case MenuAction.GoToMainMenu:
                _gameState = GameState.MainMenu;
                // сбросить игру (очистить состояние)
                ResetGame();
                break;
        }
    }

    public void OnGameOver()
    {
        ResetGame();
        _gameState = GameState.MainMenu;
    }

    private void ResetGame()
    {
        // Сброс игровых данных: пересоздать Player, LevelManager, сбросить уровень и т.д.
        _levelManager.ResetLevel();
        _player.Respawn();
        // Если нужно — сбросить камеру
        _camera = new Camera(gameWidth, gameHeight, _levelWidth, _levelHeight);
    }

    private void OnClientSizeChanged(object sender, EventArgs e)
    {
        UpdateDestinationRectangle();
        //_menuManager?.OnResolutionChanged(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
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
        if (_gameState == GameState.MainMenu || _gameState == GameState.Paused)
        {
            _menuManager.Draw(_gameState == GameState.Paused, (int)_camera.X, (int)_camera.Y);
        }
        else{
            _levelManager.DrawLevel(_spriteBatch, _font, _debug);
            _player.Draw(_spriteBatch, _font, _debug);
            if (_font != null)
            {
                var pos = _camera.GetCameraPosition();
                if (!_debug)
                {
                    _spriteBatch.DrawString(_font, $"HP: {_player.Health}\nLives: {_player.Lives}", new Vector2(pos.X + 10, pos.Y + 10), Color.White);

                    //_spriteBatch.DrawString(_font, $"X: {GraphicsDevice.Viewport.X} Y: {GraphicsDevice.Viewport.Y}", new Vector2(pos.X + 10, pos.Y + 30), Color.White);
                }
                else
                {
                    _spriteBatch.DrawString(_font, $"Xpos: {_player.Position.X} \nYpos: {_player.Position.Y}", new Vector2(pos.X + 10, pos.Y + 30), Color.White);
                }
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
                new Platform(new Rectangle(500, 300, 800, 20)),
                new Platform(new Rectangle(1800, 300, 40, 150))
            },
            new Platform[]
            {

            },
            new Platform[]
            {
            },
            new Platform[]
            {

            }
        };
        var t = new Hitbox[][]
        {
            new Hitbox[]
            {
                new Hitbox(new Rectangle(300,350,100,20), true, true, 1, 1),
                new Hitbox(new Rectangle(1000, 440, 500, 10), true, true, 1, 1.5f),
                new Hitbox(new Rectangle(1000, 35, 500, 10), true, true, 1, 1.6f),
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
