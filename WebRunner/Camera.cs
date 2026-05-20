using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WebRunner;

public class Camera
{
    
    private int _viewportWidth;
    private int _viewportHeight;
    private Vector2 _position;
    private float _levelWidth;
    private float _levelHeight;

    public Camera(int viewportWidth, int viewportHeight, float levelWidth, float levelHeight)
    {
        _viewportWidth = viewportWidth;
        _viewportHeight = viewportHeight;
        _levelWidth = MathHelper.Max(levelWidth, viewportWidth);  // не меньше экрана
        _levelHeight = MathHelper.Max(levelHeight, viewportHeight);
        _position = Vector2.Zero;
    }

    // Вызывать каждый кадр после обновления позиции игрока
    public void Follow(Vector2 target)
    {
        float targetX = target.X - _viewportWidth / 2f;
        
        float targetY = _viewportHeight / 2f;

        // Не выходим за границы уровня
        targetX = MathHelper.Clamp(targetX, 0, _levelWidth - _viewportWidth);
        targetY = MathHelper.Clamp(targetY, 0, _levelHeight - _viewportHeight);

        _position = new Vector2(targetX, targetY);
    }

    
    public Matrix GetTransformMatrix() => Matrix.CreateTranslation(-_position.X, -_position.Y, 0);
    public Vector2 GetCameraPosition() => _position;

    public void SetLevelBounds(float width, float height)
    {
        _levelWidth = MathHelper.Max(width, _viewportWidth);
        _levelHeight = MathHelper.Max(height, _viewportHeight);
    }
}