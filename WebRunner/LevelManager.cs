using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
namespace WebRunner;



/// <summary>
/// Управляет чанками уровня. Уровень состоит из 4 этажей (массивов платформ).
/// Чанк — это часть уровня определённой ширины, содержащая платформы всех этажей на этом отрезке.
/// </summary>
public class LevelManager
{
    private class Chunk
    {
        public float X { get; private set; }          // левая граница чанка
        public float Width { get; private set; }      // ширина чанка
        public List<Platform>[] FloorPlatforms { get; private set; } // платформы для каждого этажа (0..3)
        public List<Hitbox>[] FloorTraps { get; private set; }

        public Chunk(float x, float width)
        {
            X = x;
            Width = width;
            FloorPlatforms = new List<Platform>[4];
            for (int i = 0; i < 4; i++)
                FloorPlatforms[i] = new List<Platform>();
            FloorTraps = new List<Hitbox>[4];
            for (int i = 0; i < 4; i++)
                FloorTraps[i] = new List<Hitbox>();
        }

        public void Update(GameTime gameTime)
        {
            for (var i = 0; i < 4; i++)
                foreach (var trap in FloorTraps[i])
                    trap.Update(gameTime);
        }

        public float Right => X + Width;
    }

    private List<Chunk> _allChunks;          // все чанки уровня (сгенерированные или загруженные)
    private List<Chunk> _activeChunks;       // чанки, которые сейчас в памяти
    private float _chunkWidth;                // ширина одного чанка (например, 1000 пикселей)
    private int _currentFloor;                // текущий этаж (0..3)
    private float _viewDistance;              // расстояние вперёд/назад от игрока, в котором держим чанки
    private Level _currentLevel;
    private Platform _floorPlatform;
    private Platform _topPlatform;
    /// <summary>
    /// Создаёт менеджер и разбивает переданные платформы на чанки.
    /// </summary>
    /// <param name="platformsByFloor">Массив из 4 списков платформ (по этажам).</param>
    /// <param name="chunkWidth">Ширина одного чанка в пикселях.</param>
    /// <param name="viewDistance">Дистанция от игрока, на которой загружаются чанки (влево и вправо).</param>
    public LevelManager(Level level, float chunkWidth, float viewDistance)
    {
        if (level.Length != 4)
            throw new System.ArgumentException("Должно быть ровно 4 массива платформ");

        _chunkWidth = chunkWidth;
        _viewDistance = viewDistance;
        _currentFloor = 0;
        _currentLevel = level;
        _activeChunks = new List<Chunk>();
        _floorPlatform = new Platform(new Rectangle(0, 800, 2000, 10));
        _topPlatform = new Platform(new Rectangle(0, 800, 2000, 10));
        DivideLevelByChunks();
    }

    private void DivideLevelByChunks()
    {
       
        // 1. Определяем границы уровня по всем платформам
        float minX = float.MaxValue, maxX = float.MinValue;
        for (int f = 0; f < 4; f++)
        {
            foreach (var p in _currentLevel.GetPlatforms(f))
            {
                minX = MathHelper.Min(minX, p.Bounds.Left);
                maxX = MathHelper.Max(maxX, p.Bounds.Right);
            }
        }
        if (minX == float.MaxValue) minX = 0; // если платформ нет
        if (maxX == float.MinValue) maxX = minX + 1000;

        // 2. Создаём чанки, покрывающие весь диапазон [minX, maxX]
        _allChunks = new List<Chunk>();
        float start = minX;
        while (start < maxX)
        {
            var chunk = new Chunk(start, _chunkWidth);
            _allChunks.Add(chunk);
            start += _chunkWidth;
        }

        // 3. Распределяем платформы по чанкам
        foreach (var chunk in _allChunks)
        {
            for (int floor = 0; floor < 4; floor++)
            {
                var prepplatforms = _currentLevel.GetPlatforms(floor);
                foreach (var platform in prepplatforms)
                {
                    if (platform.Bounds.Right >= chunk.X && platform.Bounds.Left <= chunk.Right)
                    {
                        chunk.FloorPlatforms[floor].Add(platform);
                        var px = platform.Bounds.X;
                        var py = platform.Bounds.Y;
                        var hitbox = new Hitbox(new Rectangle(px - 2, py + 1, 5, platform.Bounds.Height - 2), false);
                        chunk.FloorTraps[floor].Add(hitbox);
                    }
                }
                var preptraps = _currentLevel.GetTraps(floor);
                foreach (var trap in preptraps)
                {
                    if (trap.Bounds.Right >= chunk.X && trap.Bounds.Left <= chunk.Right)
                    {
                        chunk.FloorTraps[floor].Add(trap);
                    }
                }
            }
        }
    }

    public void ResetLevel(Level currentLevel = null)
    {
        _activeChunks = new List<Chunk>();
        _floorPlatform.ChangeBounds(new Rectangle(0, 800, 2000, 10));
        _topPlatform.ChangeBounds(new Rectangle(0, 800, 2000, 10));
        _currentFloor = 0;
        if (currentLevel != null){
            _currentLevel = currentLevel;
            DivideLevelByChunks();
        }
    }

    /// <summary>
    /// Переключает текущий этаж.
    /// </summary>
    public void SwitchFloor(int delta)
    {
        _currentFloor = (_currentFloor + delta) % 4;
        if (_currentFloor < 0) _currentFloor += 4;
    }

    /// <summary>
    /// Обновляет активные чанки по позиции игрока.
    /// </summary>
    public void Update(Vector2 playerPosition, GameTime gameTime)
    {
        float leftBound = playerPosition.X - _viewDistance * 2;
        float rightBound = playerPosition.X + _viewDistance * 2;

        // Находим чанки, которые пересекаются с зоной видимости
        var neededChunks = _allChunks.Where(c => c.Right >= leftBound && c.X <= rightBound).ToList();
        _floorPlatform.ChangeBounds(new Rectangle((int)leftBound, 450, (int)_viewDistance * 4, 10));
        _topPlatform.ChangeBounds(new Rectangle((int)leftBound, 25, (int)_viewDistance * 4, 10));
        // Удаляем чанки, которые больше не нужны
        _activeChunks.RemoveAll(c => !neededChunks.Contains(c));

        // Добавляем новые чанки (если их ещё нет в активных)
        foreach (var chunk in neededChunks)
        {
            chunk.Update(gameTime);
            if (!_activeChunks.Contains(chunk))
                _activeChunks.Add(chunk);                
        }
    }

    /// <summary>
    /// Возвращает список платформ текущего этажа из активных чанков.
    /// Эти платформы будут использоваться для коллизий и отрисовки.
    /// </summary>
    public List<Platform> GetCurrentPlatforms()
    {
        var result = new List<Platform>();
        foreach (var chunk in _activeChunks)
        {
            result.AddRange(chunk.FloorPlatforms[_currentFloor]);
        }
        result.Add(_floorPlatform);
        result.Add(_topPlatform);
        return result;
    }

    public List<Hitbox> GetCurrentTraps()
    {
        var result = new List<Hitbox>();
        foreach (var chunk in _activeChunks)
            result.AddRange(chunk.FloorTraps[_currentFloor]);
        return result;
    }

    // Опционально: метод для загрузки текстур во все платформы (если нужно)
    public void LoadContent(Texture2D texture, Texture2D topTexture, Texture2D trapTexture)
    {
        foreach (var chunk in _allChunks)
            for (int f = 0; f < 4; f++){
                foreach (var p in chunk.FloorPlatforms[f])
                    p.LoadContent(texture, topTexture);
                foreach (var p in chunk.FloorTraps[f])
                    p.LoadContent(trapTexture);
            }
        _floorPlatform.LoadContent(texture, topTexture);
        _topPlatform.LoadContent(texture, topTexture);
        Hitbox._debugTexture = texture;
    }

    public void DrawLevel(SpriteBatch spriteBatch, SpriteFont font = null, bool debug = false)
    {
        /*
        var platforms = _currentLevel.GetPlatforms((_currentFloor + 3) % 4);
        foreach (var p in platforms)
            p.Draw(spriteBatch, false, true);*/
        /*
        var platforms = _currentLevel.GetPlatforms((_currentFloor + 2) % 4);
        foreach (var p in platforms)
            p.Draw(spriteBatch, true, false);*/
        var platforms = _currentLevel.GetPlatforms(_currentFloor % 4);
        foreach (var p in platforms)
            p.Draw(spriteBatch, false, false);
        _floorPlatform.Draw(spriteBatch, false, false);
        _topPlatform.Draw(spriteBatch, false, false);
        var traps = GetCurrentTraps();
        foreach (var t in traps)
            t.Draw(spriteBatch, font, debug);
    }
}
