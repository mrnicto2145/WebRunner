using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WebRunner;

/// <summary>
/// Действие, которое должен выполнить игровой цикл после обработки меню.
/// </summary>
public enum MenuAction
{
    None,           // ничего не делать
    StartGame,      // начать игру
    ExitGame,       // выйти из приложения
    Resume,         // продолжить игру (выход из паузы)
    GoToMainMenu    // вернуться в главное меню (сброс игры)
}

public class MenuManager
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;
    private SpriteFont _font;

    // Прямоугольники кнопок главного меню
    private Rectangle _playButtonRect;
    private Rectangle _exitButtonRect;

    // Прямоугольники кнопок меню паузы
    private Rectangle _resumeButtonRect;
    private Rectangle _menuButtonRect;

    // Размеры окна для центрирования
    private int _screenWidth;
    private int _screenHeight;
    private float mouseTranformX;
    private float mouseTranformY;

    // Текст кнопок
    private const string PlayText = "Play";
    private const string ExitText = "Exit";
    private const string ResumeText = "Continue";
    private const string MainMenuText = "Main Menu";
    private const string GameTitle = "WebRunner";

    public MenuManager(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;
        _screenWidth = graphicsDevice.Viewport.Width;
        _screenHeight = graphicsDevice.Viewport.Height;
        mouseTranformX = 1;
        mouseTranformY = 1;
    }

    /// <summary>
    /// Загружает шрифт и вычисляет размеры кнопок.
    /// </summary>
    public void LoadContent(SpriteFont font)
    {
        _font = font;

        // Получаем размеры текста кнопок
        var playSize = _font.MeasureString(PlayText);
        var exitSize = _font.MeasureString(ExitText);
        var resumeSize = _font.MeasureString(ResumeText);
        var menuSize = _font.MeasureString(MainMenuText);

        // Общая ширина и отступы (можно увеличить для удобства нажатия)
        var buttonWidth = (int)MathHelper.Max(playSize.X, exitSize.X);
        buttonWidth = (int)MathHelper.Max(buttonWidth, resumeSize.X);
        buttonWidth = (int)MathHelper.Max(buttonWidth, menuSize.X);
        buttonWidth += 40; // дополнительные отступы по бокам
        var buttonHeight = (int)playSize.Y + 20;

        // Расположение кнопок главного меню
        var centerX = _screenWidth / 2 - buttonWidth / 2;
        var startY = _screenHeight / 2;
        _playButtonRect = new Rectangle(centerX, startY, buttonWidth, buttonHeight);
        _exitButtonRect = new Rectangle(centerX, startY + buttonHeight + 20, buttonWidth, buttonHeight);

        // Расположение кнопок меню паузы (чуть выше центра)
        var pauseCenterY = _screenHeight / 2 - 50;
        _resumeButtonRect = new Rectangle(centerX, pauseCenterY, buttonWidth, buttonHeight);
        _menuButtonRect = new Rectangle(centerX, pauseCenterY + buttonHeight + 20, buttonWidth, buttonHeight);
    }

    // Лучшая реализация с хранением предыдущего состояния
    private MouseState _previousMouseState;
    /// <summary>
    /// Обновляет логику меню, обрабатывает клики мыши и возвращает требуемое действие.
    /// </summary>
    /// <param name="isPaused">true — меню паузы, false — главное меню</param>
    public MenuAction Update(bool isPaused, GameTime gameTime)
    {
        var currentMouse = Mouse.GetState();
        var leftClicked = (currentMouse.LeftButton == ButtonState.Pressed &&
                            _previousMouseState.LeftButton == ButtonState.Released);

        var result = MenuAction.None;

        if (leftClicked)
        {
            var mousePos = currentMouse.Position;
            mousePos = new Point((int)(mousePos.X * mouseTranformX), (int)(mousePos.Y * mouseTranformY));
            
            if (!isPaused) // Главное меню
            {
                if (_playButtonRect.Contains(mousePos))
                {
                    result = MenuAction.StartGame;
                }
                else if (_exitButtonRect.Contains(mousePos))
                {
                    result = MenuAction.ExitGame;
                }
            }
            else // Меню паузы
            {
                if (_resumeButtonRect.Contains(mousePos))
                {
                    result = MenuAction.Resume;
                }
                else if (_menuButtonRect.Contains(mousePos))
                {
                    result = MenuAction.GoToMainMenu;
                }
            }
        }

        _previousMouseState = currentMouse;
        return result;
    }

    private Rectangle ShiftRectangle(Rectangle t, int xShift, int yShift)
    {
        return new Rectangle(t.Location + new Point(xShift, yShift), t.Size);
    }

    /// <summary>
    /// Отрисовывает меню поверх всего.
    /// </summary>
    public void Draw(bool isPaused, int xShift = 0, int yShift = 0)
    {
        // Рисуем полупрозрачный фон, чтобы выделить меню
        var blankTexture = new Texture2D(_graphicsDevice, 1, 1);
        blankTexture.SetData(new[] { Color.Black });
        _spriteBatch.Draw(blankTexture, new Rectangle(xShift, yShift, _screenWidth, _screenHeight), Color.Black * 0.7f);

        // Отрисовка заголовка в главном меню
        if (!isPaused)
        {
            Vector2 titleSize = _font.MeasureString(GameTitle);
            Vector2 titlePos = new Vector2(_screenWidth / 2 - titleSize.X / 2 + xShift, _screenHeight / 4 + yShift);
            _spriteBatch.DrawString(_font, GameTitle, titlePos, Color.White);
        }

        // Рисуем кнопки в зависимости от режима
        if (!isPaused)
        {
            DrawButton(ShiftRectangle(_playButtonRect, xShift, yShift), PlayText);
            DrawButton(ShiftRectangle(_exitButtonRect, xShift, yShift), ExitText);
        }
        else
        {
            DrawButton(ShiftRectangle(_resumeButtonRect, xShift, yShift), ResumeText);
            DrawButton(ShiftRectangle(_menuButtonRect, xShift, yShift), MainMenuText);
        }
    }

    private void DrawButton(Rectangle rect, string text)
    {
        // Фон кнопки
        _spriteBatch.Draw(CreateWhiteTexture(), rect, Color.DarkGray);
        // Рамка
        _spriteBatch.Draw(CreateWhiteTexture(), new Rectangle(rect.X, rect.Y, rect.Width, 2), Color.White); // верх
        _spriteBatch.Draw(CreateWhiteTexture(), new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), Color.White); // низ
        _spriteBatch.Draw(CreateWhiteTexture(), new Rectangle(rect.X, rect.Y, 2, rect.Height), Color.White); // лево
        _spriteBatch.Draw(CreateWhiteTexture(), new Rectangle(rect.Right - 2, rect.Y, 2, rect.Height), Color.White); // право

        Vector2 textSize = _font.MeasureString(text);
        Vector2 textPos = new Vector2(
            rect.X + (rect.Width - textSize.X) / 2,
            rect.Y + (rect.Height - textSize.Y) / 2
        );
        _spriteBatch.DrawString(_font, text, textPos, Color.White);
    }

    // Вспомогательная текстура 1x1 белого цвета (создаётся один раз)
    private Texture2D _whiteTexture;
    private Texture2D CreateWhiteTexture()
    {
        if (_whiteTexture == null)
        {
            _whiteTexture = new Texture2D(_graphicsDevice, 1, 1);
            _whiteTexture.SetData(new[] { Color.White });
        }
        return _whiteTexture;
    }

    /// <summary>
    /// Вызывать при изменении размера окна, чтобы пересчитать расположение кнопок.
    /// </summary>
    public void OnResolutionChanged(int width, int height)
    {
        mouseTranformX = 800f / width;
        mouseTranformY = 480f / height;
        if (_font != null)
            LoadContent(_font); // пересчитать прямоугольники
    }
}