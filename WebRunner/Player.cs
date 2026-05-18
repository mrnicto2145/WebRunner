

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace WebRunner;

public class Player
{
    private Texture2D _texture;
    private Vector2 _position;
    private Vector2 _velocity;
    private bool _isOnGround;
    private const float Gravity = 1200f;    // пикселей/сек²
    private const float JumpForce = -400f;  // отскок вверх
    private const float MoveSpeed = 300f;   // скорость по X
    private bool _prevDownKey;
    private bool _prevUpKey;

    public Vector2 Position => _position;

    public Player(Vector2 startPos)
    {
        _position = startPos;
        _velocity = Vector2.Zero;
        _prevDownKey = false;
        _prevUpKey = false;
    }

    public void LoadContent(Texture2D texture)
    {
        _texture = texture; // используем одну белую точку, но можно любой квадрат 32x32
    }

    public void Update(GameTime gameTime, LevelManager manager, bool debug)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        KeyboardState kb = Keyboard.GetState();
        // --- Горизонтальное движение ---
        float move = MoveSpeed;
        if (debug){
            move = 0f;
            if (kb.IsKeyDown(Keys.Left) || kb.IsKeyDown(Keys.A))
                move = -MoveSpeed;
            if (kb.IsKeyDown(Keys.Right) || kb.IsKeyDown(Keys.D))
                move = MoveSpeed;
        }
        _velocity.X = move;

        if (kb.IsKeyDown(Keys.Space) && _isOnGround)
        {
            _velocity.Y = JumpForce;
            _isOnGround = false;
        }
        
        bool currentDown = kb.IsKeyDown(Keys.Down);
        if (currentDown && !_prevDownKey) manager.SwitchFloor(5);
        _prevDownKey = currentDown;

        bool currentUp = kb.IsKeyDown(Keys.Up);
        if (currentUp && !_prevUpKey) manager.SwitchFloor(1);
        _prevUpKey = currentUp;

        // --- Гравитация ---
        if (!_isOnGround)
            _velocity.Y += Gravity * dt;

        // --- Временное перемещение (для коллизий) ---
        Vector2 newPos = _position + _velocity * dt;
        Rectangle playerRect = GetPlayerRect(newPos);

        // --- Проверка коллизий с платформами ---
        var platforms = manager.GetCurrentPlatforms();
        _isOnGround = false;
        foreach (var platform in platforms)
        {
            Rectangle platformRect = platform.Bounds;
            if (playerRect.Intersects(platformRect))
            {
                // Откатываем столкновение по Y (сверху/снизу) и X (слева/справа)
                // Простейший способ: разделить на X и Y (пошагово)
                // Сначала пробуем только по Y
                Rectangle tryYRect = GetPlayerRect(new Vector2(_position.X, newPos.Y));
                if (tryYRect.Intersects(platformRect))
                {
                    // Столкновение по вертикали
                    if (_velocity.Y > 0) // падаем сверху
                    {
                        newPos.Y = platformRect.Top - playerRect.Height;
                        _velocity.Y = 0;
                        _isOnGround = true;
                    }
                    else if (_velocity.Y < 0) // ударились головой
                    {
                        newPos.Y = platformRect.Bottom;
                        _velocity.Y = 0;
                    }
                }
                // Потом по X (после корректировки Y)
                playerRect = GetPlayerRect(newPos);
                if (playerRect.Intersects(platformRect))
                {
                    if (_velocity.X > 0)
                        newPos.X = platformRect.Left - playerRect.Width;
                    else if (_velocity.X < 0)
                        newPos.X = platformRect.Right;
                }
            }
        }

        _position = newPos;
        /*
        // Опционально: ограничение за края экрана
        if (_position.X < 0) _position.X = 0;
        if (_position.X + 32 > 800) _position.X = 800 - 32; // если ширина экрана 800*/
    }

    private Rectangle GetPlayerRect(Vector2 pos)
    {
        // Просто квадрат 32x32 (можно поменять размер)
        return new Rectangle((int)pos.X, (int)pos.Y, 32, 32);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_texture, GetPlayerRect(_position), Color.Red);
        // Для теста рисуем красным. Потом можно заменить на текстуру с прозрачностью.
    }
}
