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
        _player = new Player(new Vector2(-40, 100));
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
        _levelManager.LoadContent(pixelTexture, topTexture, pixelTexture, Color.Green);
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
        _menuManager?.OnResolutionChanged(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
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
                //обучение прыжкам
                new Platform(new Rectangle(400, 400, 200, 20)),
                new Platform(new Rectangle(700, 360, 400, 20)),
                new Platform(new Rectangle(1200, 400, 200, 20)),
                new Platform(new Rectangle(1500, 360, 400, 20)),

                //обучение "подвисанию"
                new Platform(new Rectangle(2400, 200, 20, 250)),
                new Platform(new Rectangle(2700, 35, 20, 250)),
                new Platform(new Rectangle(2900, 200, 20, 250)),
                new Platform(new Rectangle(3100, 35, 20, 250)),

                //комбинирование механик
                new Platform(new Rectangle(3400, 400, 600, 20)),
                new Platform(new Rectangle(3700, 150, 700, 20)),
                new Platform(new Rectangle(4300, 400, 400, 20)),
                new Platform(new Rectangle(4700, 150, 600, 20)),

                new Platform(new Rectangle(5800, 200, 20, 250)),
                new Platform(new Rectangle(6100, 35, 20, 250)),
                new Platform(new Rectangle(6400, 150, 20, 300)),
                new Platform(new Rectangle(6600, 35, 20, 300)),
                new Platform(new Rectangle(6800, 150, 20, 300)),


                //game over
                new Platform(new Rectangle(8000, 35, 20, 415))
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
                //обучение прыжкам
                new Hitbox(new Rectangle(400, 440, 1500, 10), true, false, 1, 0f),
                new Hitbox(new Rectangle(400, 35, 1500, 10), true, false, 1, 0f),

                //обучение подвисанию
                //комбинирование механик
                new Hitbox(new Rectangle(3400, 440, 1900, 10), true, false, 1, 0f),
                new Hitbox(new Rectangle(3400, 35, 1900, 10), true, false, 1, 0f),

                new Hitbox(new Rectangle(5820, 440, 580, 10), true, false, 1, 0f),
                new Hitbox(new Rectangle(6420, 440, 380, 10), true, false, 1, 0f),

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
        var h = new Hint[]
        {
            new Hint("Press Space to jump", 400, 200),
            new Hint("Press Z to jump on top\n(Be careful, you can't cancel it)", 2100, 200),
            new Hint("Game over", 7700, 200)
        };
        return new List<Level>() { new Level(p, t, h) };
    }
}
