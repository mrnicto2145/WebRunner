using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WebRunner;

public class Camera
{
    private Viewport _viewport;
    private Vector2 _position;
    private float _levelWidth;
    private float _levelHeight;

    public Camera(Viewport viewport, float levelWidth, float levelHeight)
    {
        _viewport = viewport;
        _levelWidth = MathHelper.Max(levelWidth, viewport.Width);  // не меньше экрана
        _levelHeight = MathHelper.Max(levelHeight, viewport.Height);
        _position = Vector2.Zero;
    }

    // Вызывать каждый кадр после обновления позиции игрока
    public void Follow(Vector2 target)
    {
        float targetX = target.X - _viewport.Width / 2f;
        
        float targetY = _viewport.Height / 2f;

        // Не выходим за границы уровня
        targetX = MathHelper.Clamp(targetX, 0, _levelWidth - _viewport.Width);
        targetY = MathHelper.Clamp(targetY, 0, _levelHeight - _viewport.Height);

        _position = new Vector2(targetX, targetY);
    }

    
    public Matrix GetTransformMatrix() => Matrix.CreateTranslation(-_position.X, -_position.Y, 0);

    public void SetLevelBounds(float width, float height)
    {
        _levelWidth = MathHelper.Max(width, _viewport.Width);
        _levelHeight = MathHelper.Max(height, _viewport.Height);
    }
}