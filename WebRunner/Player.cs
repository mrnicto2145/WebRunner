

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
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
    private bool _prevZKey;
    private int _jumpOnTop;
    private bool _wasOnGround;
    private float _currentGravity;

    private int _health;
    private int _maxHealth;
    private int _lives;
    private float _invincibilityTimer;   // неуязвимость после получения урона
    private const float InvincibilityDuration = 1.0f;
    private Hitbox _hitbox;              // хитбокс самого игрока

    // Свойства для доступа
    public int Health => _health;
    public int Lives => _lives;
    public Hitbox Hitbox => _hitbox;
    public bool IsAlive => _health > 0;

    public Vector2 Position => _position;

    public Player(Vector2 startPos)
    {
        _position = startPos;
        _velocity = Vector2.Zero;
        _prevDownKey = false;
        _prevUpKey = false;
        _prevZKey = false;
        _jumpOnTop = 1;
        _currentGravity = Gravity;

        _maxHealth = 5;
        _health = _maxHealth;
        _lives = 3;
        _invincibilityTimer = 0f;
        // Хитбокс игрока – чуть меньше его спрайта для удобства (32x32 -> 28x28)
        _hitbox = new Hitbox(new Rectangle((int)startPos.X + 2, (int)startPos.Y + 2, 28, 28), 0);
    }

    public void LoadContent(Texture2D texture)
    {
        _texture = texture; // используем одну белую точку, но можно любой квадрат 32x32
    }

    public void Update(GameTime gameTime, LevelManager manager, bool debug)
    {
        _wasOnGround = _isOnGround;
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        KeyboardState kb = Keyboard.GetState();
        // --- Горизонтальное движение ---
        float move = MoveSpeed;
        if (debug)
        {
            move = 0f;
            if (kb.IsKeyDown(Keys.Left) || kb.IsKeyDown(Keys.A))
                move = -MoveSpeed;
            if (kb.IsKeyDown(Keys.Right) || kb.IsKeyDown(Keys.D))
                move = MoveSpeed;
        }
        _velocity.X = move;

        if (_invincibilityTimer > 0)
        {
            _invincibilityTimer -= dt;
            if (Math.Abs(_invincibilityTimer) < 1e-4)
            {
                _invincibilityTimer = 0;
            }
        }

        if (kb.IsKeyDown(Keys.Space) && _isOnGround)
        {
            _velocity.Y = JumpForce;
            _isOnGround = false;
            _jumpOnTop = 1;
            _currentGravity = Gravity;
        }
        /*
        bool currentDown = kb.IsKeyDown(Keys.Down);
        if (currentDown && !_prevDownKey) manager.SwitchFloor(5);
        _prevDownKey = currentDown;

        bool currentUp = kb.IsKeyDown(Keys.Up);
        if (currentUp && !_prevUpKey) manager.SwitchFloor(1);
        _prevUpKey = currentUp;
        */

        bool currentZ = kb.IsKeyDown(Keys.Z);
        if (currentZ && !_prevZKey)
        {
            _jumpOnTop = -_jumpOnTop;
            _currentGravity = _jumpOnTop * Gravity * 5;
        }
        _prevZKey = currentZ;

        // --- Гравитация ---
        if (!_isOnGround)
            _velocity.Y += _currentGravity * dt;

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
                    if (_velocity.Y > 0)
                    {
                        newPos.Y = platformRect.Top - playerRect.Height;
                        _velocity.Y = 0;
                        _isOnGround = _currentGravity > 0;
                    }
                    else if (_velocity.Y < 0)
                    {
                        newPos.Y = platformRect.Bottom;
                        _velocity.Y = 0;
                        _isOnGround = _currentGravity < 0;
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
        // Если только что оторвались от платформы (прыжок или соскальзывание)

        _position = newPos;

        _hitbox.Bounds = new Rectangle((int)_position.X + 2, (int)_position.Y + 2, 28, 28);
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

    public void Draw(SpriteBatch spriteBatch, SpriteFont font = null, bool debug = false)
    {
        spriteBatch.Draw(_texture, GetPlayerRect(_position), Color.Red);
        // Для теста рисуем красным. Потом можно заменить на текстуру с прозрачностью.
        if ((font != null) && debug)
        {
            spriteBatch.DrawString(font, $"HP: {_health}/{_maxHealth}  Lives: {_lives} Timer {_invincibilityTimer}",
                new Vector2(_position.X - 20, _position.Y - 30), Color.White);
        }
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive) return;
        if (_invincibilityTimer > 0) return;   // неуязвимость

        _health -= amount;
        if (_health <= 0)
        {
            _health = 0;
            Die();
        }
        else
        {
            _invincibilityTimer = InvincibilityDuration;
        }
    }

    public void Heal(int amount)
    {
        if (!IsAlive) return;
        _health = MathHelper.Min(_health + amount, _maxHealth);
    }

    private void Die()
    {
        _lives--;
        if (_lives > 0)
        {
            // Возрождение с полным здоровьем в начале уровня
            Respawn();
        }
        else
        {
            // Игра окончена – можно вызвать событие или выйти
            // Например, сбросить игру или показать экран "Game Over"
            OnGameOver();
        }
    }

    private void Respawn()
    {
        _health = _maxHealth;
        _velocity = Vector2.Zero;
        _position = new Vector2(100, 100);   // стартовая позиция
        _invincibilityTimer = 0f;        // можно дать небольшую неуязвимость после респавна
        _jumpOnTop = 1;
        _currentGravity = Gravity;
    }

    private void OnGameOver()
    {
        // Реализуйте нужное поведение: перезапуск уровня, выход в меню и т.д.
        // Для простоты пока просто сбросим жизни и респавн:
        _lives = 3;
        Respawn();
    }
}
