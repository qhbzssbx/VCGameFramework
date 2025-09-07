using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Infrastructure.Managers
{
    /// <summary>
    /// 时间管理器实现
    /// </summary>
    public class TimeManager : ITimeManager
    {
        private readonly Stack<float> _timeScaleStack = new();
        private float _defaultTimeScale = 1.0f;
        private bool _isGamePaused = false;
        private float _savedTimeScale = 1.0f;
        
        /// <summary>
        /// 游戏是否处于暂停状态
        /// </summary>
        public bool IsGamePaused => _isGamePaused;
        
        /// <summary>
        /// 当前游戏时间缩放
        /// </summary>
        public float GameTimeScale => Time.timeScale;
        
        /// <summary>
        /// 默认时间缩放值
        /// </summary>
        public float DefaultTimeScale 
        { 
            get => _defaultTimeScale;
            set
            {
                if (value < 0)
                {
                    Debug.LogWarning("Default time scale cannot be negative, clamping to 0");
                    value = 0;
                }
                
                _defaultTimeScale = value;
                Debug.Log($"Default time scale set to: {_defaultTimeScale}");
            }
        }
        
        /// <summary>
        /// 时间缩放栈深度
        /// </summary>
        public int TimeScaleStackDepth => _timeScaleStack.Count;
        
        /// <summary>
        /// 游戏暂停状态改变事件
        /// </summary>
        public event Action<bool> OnGamePausedChanged;
        
        /// <summary>
        /// 时间缩放改变事件
        /// </summary>
        public event Action<float> OnTimeScaleChanged;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public TimeManager()
        {
            // 确保初始时间缩放为默认值
            Time.timeScale = _defaultTimeScale;
            Debug.Log($"TimeManager initialized with default time scale: {_defaultTimeScale}");
        }
        
        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            if (_isGamePaused)
            {
                Debug.LogWarning("Game is already paused");
                return;
            }
            
            // 保存当前时间缩放
            _savedTimeScale = Time.timeScale;
            
            // 设置时间缩放为0（暂停）
            Time.timeScale = 0f;
            _isGamePaused = true;
            
            Debug.Log($"Game paused (saved time scale: {_savedTimeScale})");
            
            // 触发事件
            OnGamePausedChanged?.Invoke(true);
            OnTimeScaleChanged?.Invoke(0f);
        }
        
        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void ResumeGame()
        {
            if (!_isGamePaused)
            {
                Debug.LogWarning("Game is not paused");
                return;
            }
            
            // 恢复之前保存的时间缩放
            Time.timeScale = _savedTimeScale;
            _isGamePaused = false;
            
            Debug.Log($"Game resumed (restored time scale: {_savedTimeScale})");
            
            // 触发事件
            OnGamePausedChanged?.Invoke(false);
            OnTimeScaleChanged?.Invoke(_savedTimeScale);
        }
        
        /// <summary>
        /// 切换游戏暂停状态
        /// </summary>
        public void TogglePause()
        {
            if (_isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
        
        /// <summary>
        /// 设置游戏时间缩放
        /// </summary>
        /// <param name="timeScale">时间缩放值</param>
        public void SetTimeScale(float timeScale)
        {
            if (timeScale < 0)
            {
                Debug.LogWarning("Time scale cannot be negative, clamping to 0");
                timeScale = 0;
            }
            
            if (_isGamePaused)
            {
                Debug.LogWarning("Cannot set time scale while game is paused");
                return;
            }
            
            var oldTimeScale = Time.timeScale;
            Time.timeScale = timeScale;
            
            Debug.Log($"Time scale changed from {oldTimeScale} to {timeScale}");
            
            // 触发事件
            OnTimeScaleChanged?.Invoke(timeScale);
        }
        
        /// <summary>
        /// 推入时间缩放到栈中（支持嵌套时间控制）
        /// </summary>
        /// <param name="timeScale">时间缩放值</param>
        public void PushTimeScale(float timeScale)
        {
            if (timeScale < 0)
            {
                Debug.LogWarning("Time scale cannot be negative, clamping to 0");
                timeScale = 0;
            }
            
            if (_isGamePaused)
            {
                Debug.LogWarning("Cannot push time scale while game is paused");
                return;
            }
            
            // 推入当前时间缩放到栈
            _timeScaleStack.Push(Time.timeScale);
            
            // 设置新的时间缩放
            var oldTimeScale = Time.timeScale;
            Time.timeScale = timeScale;
            
            Debug.Log($"Pushed time scale: {oldTimeScale} -> {timeScale} (Stack depth: {_timeScaleStack.Count})");
            
            // 触发事件
            OnTimeScaleChanged?.Invoke(timeScale);
        }
        
        /// <summary>
        /// 从栈中弹出时间缩放
        /// </summary>
        public void PopTimeScale()
        {
            if (_timeScaleStack.Count == 0)
            {
                Debug.LogWarning("Time scale stack is empty, cannot pop");
                return;
            }
            
            if (_isGamePaused)
            {
                Debug.LogWarning("Cannot pop time scale while game is paused");
                return;
            }
            
            // 从栈中弹出之前的时间缩放
            var previousTimeScale = _timeScaleStack.Pop();
            var currentTimeScale = Time.timeScale;
            
            Time.timeScale = previousTimeScale;
            
            Debug.Log($"Popped time scale: {currentTimeScale} -> {previousTimeScale} (Stack depth: {_timeScaleStack.Count})");
            
            // 触发事件
            OnTimeScaleChanged?.Invoke(previousTimeScale);
        }
        
        /// <summary>
        /// 重置时间缩放到默认值
        /// </summary>
        public void ResetTimeScale()
        {
            if (_isGamePaused)
            {
                Debug.LogWarning("Cannot reset time scale while game is paused");
                return;
            }
            
            var oldTimeScale = Time.timeScale;
            Time.timeScale = _defaultTimeScale;
            
            Debug.Log($"Time scale reset from {oldTimeScale} to {_defaultTimeScale}");
            
            // 触发事件
            OnTimeScaleChanged?.Invoke(_defaultTimeScale);
        }
        
        /// <summary>
        /// 清空时间缩放栈并重置到默认值
        /// </summary>
        public void ClearTimeScaleStack()
        {
            var stackDepth = _timeScaleStack.Count;
            _timeScaleStack.Clear();
            
            if (!_isGamePaused)
            {
                ResetTimeScale();
            }
            
            Debug.Log($"Time scale stack cleared (was {stackDepth} deep)");
        }
    }
}