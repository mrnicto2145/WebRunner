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
    public static Texture2D _texture;

    public Hitbox(Rectangle bounds, int damage = 1, float cooldown = 0.5f)
    {
        Bounds = bounds;
        Damage = damage;
        Cooldown = cooldown;
    }

    /// <summary>
    /// Обновление таймера кд. Вызывать каждый кадр.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        if (_cooldownTimer > 0)
        {
            _cooldownTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (Math.Abs(_cooldownTimer) < 1e-4)
            {
                _cooldownTimer = 0;
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

    public void Draw(SpriteBatch spriteBatch, SpriteFont font = null, bool debug = false)
    {
        spriteBatch.Draw(_texture, Bounds, Color.Red);
        if (!(font == null) && debug)
        {
            spriteBatch.DrawString(font, $"Timer {_cooldownTimer}",
                new Vector2(Bounds.X - 20, Bounds.Y - 30), Color.White);
        }
    }

}