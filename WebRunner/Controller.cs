using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace WebRunner
{
    /// <summary>
    /// Централизованный обработчик ввода с клавиатуры.
    /// Позволяет регистрировать действия на одноразовые нажатия, удержания и комбинации клавиш.
    /// </summary>
    public class Controller
    {
        // Словарь для обработчиков, вызываемых при одиночном нажатии (только в момент нажатия)
        private readonly Dictionary<Keys, Action> _onPressedHandlers = new Dictionary<Keys, Action>();
        
        // Словарь для обработчиков, вызываемых каждый кадр при удержании клавиши
        private readonly Dictionary<Keys, Action> _onHeldHandlers = new Dictionary<Keys, Action>();
        
        // Словарь для комбинаций клавиш (ключ – массив клавиш, значение – действие)
        private readonly Dictionary<Keys[], Action> _combinationHandlers = new Dictionary<Keys[], Action>(new KeysArrayComparer());
        
        // Предыдущее состояние клавиатуры для детектирования момента нажатия
        private KeyboardState _previousKeyboardState;
        
        // Состояние удержания комбинаций (чтобы срабатывали только один раз)
        private readonly Dictionary<Keys[], bool> _combinationTriggered = new Dictionary<Keys[], bool>(new KeysArrayComparer());
        
        /// <summary>
        /// Регистрирует действие, которое выполнится ОДИН РАЗ в момент нажатия клавиши.
        /// </summary>
        /// <param name="key">Клавиша.</param>
        /// <param name="action">Делегат, вызываемый при нажатии.</param>
        public void RegisterKeyDown(Keys key, Action action)
        {
            if (!_onPressedHandlers.ContainsKey(key))
                _onPressedHandlers.Add(key, action);
            else
                _onPressedHandlers[key] += action;
        }
        
        /// <summary>
        /// Регистрирует действие, которое будет выполняться КАЖДЫЙ КАДР, пока клавиша удерживается.
        /// </summary>
        /// <param name="key">Клавиша.</param>
        /// <param name="action">Делегат, вызываемый в каждом кадре при удержании.</param>
        public void RegisterKeyHeld(Keys key, Action action)
        {
            if (!_onHeldHandlers.ContainsKey(key))
                _onHeldHandlers.Add(key, action);
            else
                _onHeldHandlers[key] += action;
        }
        
        /// <summary>
        /// Регистрирует действие на комбинацию клавиш (все клавиши должны быть нажаты одновременно).
        /// Срабатывает один раз в момент, когда комбинация становится активной.
        /// </summary>
        /// <param name="keys">Массив клавиш, которые должны быть нажаты вместе.</param>
        /// <param name="action">Делегат, вызываемый при активации комбинации.</param>
        public void RegisterCombination(Keys[] keys, Action action)
        {
            if (!_combinationHandlers.ContainsKey(keys))
                _combinationHandlers.Add(keys, action);
            else
                _combinationHandlers[keys] += action;
            
            if (!_combinationTriggered.ContainsKey(keys))
                _combinationTriggered.Add(keys, false);
        }
        
        /// <summary>
        /// Обновляет состояние контроллера. Должен вызываться один раз за кадр.
        /// </summary>
        public void Update()
        {
            KeyboardState currentState = Keyboard.GetState();
            
            // 1. Обработка одиночных нажатий (по переходу из отпущенного в нажатое)
            foreach (var pair in _onPressedHandlers)
            {
                Keys key = pair.Key;
                if (currentState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key))
                {
                    pair.Value?.Invoke();
                }
            }
            
            // 2. Обработка удержаний (вызывается каждый кадр, пока клавиша нажата)
            foreach (var pair in _onHeldHandlers)
            {
                if (currentState.IsKeyDown(pair.Key))
                {
                    pair.Value?.Invoke();
                }
            }
            
            // 3. Обработка комбинаций
            foreach (var comb in _combinationHandlers)
            {
                Keys[] keys = comb.Key;
                bool allPressed = true;
                foreach (Keys k in keys)
                {
                    if (!currentState.IsKeyDown(k))
                    {
                        allPressed = false;
                        break;
                    }
                }
                
                bool wasTriggered = _combinationTriggered[keys];
                if (allPressed && !wasTriggered)
                {
                    comb.Value?.Invoke();
                    _combinationTriggered[keys] = true;
                }
                else if (!allPressed && wasTriggered)
                {
                    _combinationTriggered[keys] = false; // сброс флага, когда комбинация отпущена
                }
            }
            
            _previousKeyboardState = currentState;
        }
        
        /// <summary>
        /// Вспомогательный класс для сравнения массивов клавиш (по содержимому, а не по ссылке).
        /// </summary>
        private class KeysArrayComparer : IEqualityComparer<Keys[]>
        {
            public bool Equals(Keys[] x, Keys[] y)
            {
                if (x == null || y == null) return false;
                if (x.Length != y.Length) return false;
                for (int i = 0; i < x.Length; i++)
                    if (x[i] != y[i]) return false;
                return true;
            }
            
            public int GetHashCode(Keys[] obj)
            {
                if (obj == null) return 0;
                int hash = 17;
                foreach (var k in obj)
                    hash = hash * 31 + k.GetHashCode();
                return hash;
            }
        }
    }
    
    /* 
    ============================================================================
    ПРИМЕР ИСПОЛЬЗОВАНИЯ В СУЩЕСТВУЮЩЕМ ПРОЕКТЕ
    ============================================================================
    
    1. В классе Game1 добавьте поле:
        private Controller _controller;
    
    2. В Initialize() после создания остальных объектов:
        _controller = new Controller();
        RegisterGameplayControls();
    
    3. В Update() перед обработкой состояния игры вызовите:
        _controller.Update();
    
    4. Регистрация всех обнаруженных клавиш (пример):
    
    private void RegisterGameplayControls()
    {
        // ----- Глобальные клавиши (обрабатываются в Game1) -----
        _controller.RegisterKeyDown(Keys.Escape, () =>
        {
            // Логика переключения между паузой и игрой / выход из меню
            if (_gameState == GameState.Playing)
                _gameState = GameState.Paused;
            else if (_gameState == GameState.Paused)
                _gameState = GameState.Playing;
            else if (_gameState == GameState.MainMenu)
                Exit();
        });
        
        // Комбинация D+E+B для включения отладки (обработка внутри Player, но можно поднять сюда)
        _controller.RegisterCombination(new[] { Keys.D, Keys.E, Keys.B }, () =>
        {
            Game1._debug = !Game1._debug;
        });
        
        // ----- Управление игроком (обычно вынести в отдельный метод, вызываемый из Player.Update, но для наглядности) -----
        // Прыжок (одноразово)
        _controller.RegisterKeyDown(Keys.Space, () =>
        {
            if (_player.IsOnGround) // потребуется добавить свойство IsOnGround в Player
                _player.Jump();     // добавить метод Jump()
        });
        
        // Смена гравитации (клавиша Z)
        _controller.RegisterKeyDown(Keys.Z, () =>
        {
            _player.ToggleGravity();
        });
        
        // Движение влево/вправо (только в debug-режиме) – удержание
        _controller.RegisterKeyHeld(Keys.Left, () =>
        {
            if (Game1._debug)
                _player.MoveLeft();
        });
        _controller.RegisterKeyHeld(Keys.Right, () =>
        {
            if (Game1._debug)
                _player.MoveRight();
        });
        
        // Смена этажей (закомментированные клавиши в оригинале) – одноразово
        _controller.RegisterKeyDown(Keys.Down, () =>
        {
            _levelManager.SwitchFloor(5); // или 1? по коду было 5
        });
        _controller.RegisterKeyDown(Keys.Up, () =>
        {
            _levelManager.SwitchFloor(1);
        });
    }
    
    ============================================================================
    ПРИМЕЧАНИЯ ПО ИЗМЕНЕНИЮ СУЩЕСТВУЮЩИХ КЛАССОВ
    ============================================================================
    
    Чтобы полностью перевести игру на использование Controller, необходимо:
    
    - В Player.cs убрать прямые вызовы Keyboard.GetState() и заменить их вызовами методов,
      которые будут вызываться через делегаты контроллера.
    - В Game1.cs убрать обработку Escape и debug-комбинации из Update.
    - Добавить в Player свойства IsOnGround (публичное), методы Jump(), ToggleGravity(),
      MoveLeft(), MoveRight().
    
    Альтернативный (более простой) путь – оставить игровую логику как есть, но использовать
    Controller только для централизованного сбора нажатий и проброса их в существующие методы.
    Для этого можно в RegisterGameplayControls() вызывать методы существующих объектов напрямую,
    как показано выше, но тогда нужно добавить недостающие методы/свойства.
    
    */
}