

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
    private float _currentGravity;
    private int _nearYtop;

    private string _damageLock;
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
        _damageLock = "";
        _prevDownKey = false;
        _prevUpKey = false;
        _prevZKey = false;
        _currentGravity = Gravity;
        _maxHealth = 5;
        _health = _maxHealth;
        _lives = 3;
        _invincibilityTimer = 0f;
        // Хитбокс игрока – чуть меньше его спрайта для удобства (32x32 -> 28x28)
        _hitbox = new Hitbox(new Rectangle((int)startPos.X, (int)startPos.Y, 32, 32), false, false, 0);
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
        if (!debug){
            _velocity.X += MoveSpeed * dt;
        }
        else
        {
            _velocity.X = 0;
            if (kb.IsKeyDown(Keys.Left))
                _velocity.X += -MoveSpeed;
            if (kb.IsKeyDown(Keys.Right))
                _velocity.X += MoveSpeed;                
        }
        
        if (_velocity.X > MoveSpeed) _velocity.X = MoveSpeed;
        if (_velocity.X < -MoveSpeed) _velocity.X = -MoveSpeed;

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
            _currentGravity = Gravity; 
        }

        var currDebug = kb.IsKeyDown(Keys.D) && kb.IsKeyDown(Keys.E) && kb.IsKeyDown(Keys.B);
        if (currDebug && !_prevDownKey)
        {
            Game1._debug = !Game1._debug;
        }
        _prevDownKey = currDebug;
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
            _currentGravity = _currentGravity < 0 ? Gravity : - Gravity * 5;
        
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
        _nearYtop = _currentGravity < 0 ? int.MinValue : 0; 
        foreach (var platform in platforms)
        {
            Rectangle platformRect = platform.Bounds;
            if (_position.X >= platformRect.X && playerRect.X <= platformRect.X + platformRect.Width)
            {
                if (_currentGravity < 0)
                {
                    if (_position.Y >= platformRect.Y + platformRect.Height)
                        _nearYtop = Math.Max(_nearYtop, platformRect.Y + platformRect.Height);
                }
                else
                    _nearYtop = -1;
            }
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

        _hitbox.Bounds = new Rectangle((int)_position.X, (int)_position.Y, 32, 32);
        /*
        // Опционально: ограничение за края экрана
        if (_position.X < 0) _position.X = 0;
        if (_position.X + 32 > 800) _position.X = 800 - 32; // если ширина экрана 800*/
    }
    public void Push(Vector2 velocity)
    {
        _velocity += velocity;
    }

    private Rectangle GetPlayerRect(Vector2 pos)
    {
        return new Rectangle((int)pos.X, (int)pos.Y, 32, 32);
    }

    public void Draw(SpriteBatch spriteBatch, SpriteFont font = null, bool debug = false)
    {
        var playerRect = GetPlayerRect(_position);
        spriteBatch.Draw(_texture, playerRect,_invincibilityTimer > 0 ? Color.Red : Color.Blue);
        if (_nearYtop != -1)
        {
            spriteBatch.Draw(_texture, new Rectangle(playerRect.Center.X, _nearYtop, 1, playerRect.Y - _nearYtop), Color.White);
        }
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
            Push(new Vector2(-1000, -400));
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

    public void Respawn(float invincibilityTimer = 1f)
    {
        _health = _maxHealth;
        _velocity = Vector2.Zero;
        _position = new Vector2(100, 100);   // стартовая позиция
        _invincibilityTimer = invincibilityTimer;        // можно дать небольшую неуязвимость после респавна
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
