using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WebRunner;

/// <summary>
/// Хитбокс — область, которая может наносить урон или получать его.
/// </summary>
public class Hitbox
{
    public Rectangle Bounds { get; set; }
    public int Damage { get; set; }          // сколько урона наносит при столкновении
    public bool IsActive { get; set; } = true;
    public float Cooldown { get; set; } = 0f; // задержка между срабатываниями (сек)

    private float _cooldownTimer = 0f;
    public static Texture2D _debugTexture;
    private Texture2D _texture;
    private bool _drawable;
    public bool drawable => _drawable;
    private float _switcherCooldown;
    private bool _switcher;
    private float _switchSpeed;


    public Hitbox(Rectangle bounds, bool drawable, bool switcher = false, int damage = 1, float cooldown = 0f, float switchSpeed = 1f)
    {
        Bounds = bounds;
        Damage = damage;
        Cooldown = cooldown;
        _drawable = drawable;
        _switcher = switcher;
        _switcherCooldown = 0f;
        _switchSpeed = switchSpeed;
    }

    /// <summary>
    /// Обновление таймера кд. Вызывать каждый кадр.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_cooldownTimer > 0)
        {
            _cooldownTimer -= dt;
            if (Math.Abs(_cooldownTimer) < 1e-4)
            {
                _cooldownTimer = 0;
            }
        }
        else if (_switcher)
        {
            _switcherCooldown += _switchSpeed * dt;
            if (_switcherCooldown >= Cooldown)
            {
                _cooldownTimer = 3 * Cooldown;
                _switcherCooldown = 0;
            }
        }
    }

    /// <summary>
    /// Попытка нанести урон цели. Возвращает true, если урон был нанесён.
    /// </summary>
    public bool TryDamage(Player player)
    {
        if (!IsActive) return false;
        if (_cooldownTimer > 0) return false;
        if (!Bounds.Intersects(player.Hitbox.Bounds)) return false;

        
        player.TakeDamage(Damage);        
        _cooldownTimer = Cooldown;
        return true;
    }

    public void LoadContent(Texture2D texture)
    {
        _texture = texture;
    }

    public void Draw(SpriteBatch spriteBatch, SpriteFont font = null, bool debug = false)
    {
        if (_drawable)
        {
            spriteBatch.Draw(_texture, Bounds,_cooldownTimer > 0 ? Color.Blue : Color.Azure);
        }
        if (!(font == null) && debug)
        {
            spriteBatch.Draw(_debugTexture, Bounds, Color.Red);
            spriteBatch.DrawString(font, $"Timer {_cooldownTimer}",
                new Vector2(Bounds.X - 20, Bounds.Y - 30), Color.White);
        }
    }
}