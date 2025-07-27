using System;

namespace Game.Infrastructure.Managers
{
    /// <summary>
    /// 时间管理器接口，用于控制游戏时间流速和暂停状态
    /// </summary>
    public interface ITimeManager
    {
        /// <summary>
        /// 游戏是否处于暂停状态
        /// </summary>
        bool IsGamePaused { get; }
        
        /// <summary>
        /// 当前游戏时间缩放
        /// </summary>
        float GameTimeScale { get; }
        
        /// <summary>
        /// 默认时间缩放值
        /// </summary>
        float DefaultTimeScale { get; set; }
        
        /// <summary>
        /// 时间缩放栈深度
        /// </summary>
        int TimeScaleStackDepth { get; }
        
        /// <summary>
        /// 游戏暂停状态改变事件
        /// </summary>
        event Action<bool> OnGamePausedChanged;
        
        /// <summary>
        /// 时间缩放改变事件
        /// </summary>
        event Action<float> OnTimeScaleChanged;

        /// <summary>
        /// 暂停游戏
        /// </summary>
        void PauseGame();
        
        /// <summary>
        /// 恢复游戏
        /// </summary>
        void ResumeGame();
        
        /// <summary>
        /// 切换游戏暂停状态
        /// </summary>
        void TogglePause();
        
        /// <summary>
        /// 设置游戏时间缩放
        /// </summary>
        /// <param name="timeScale">时间缩放值</param>
        void SetTimeScale(float timeScale);
        
        /// <summary>
        /// 推入时间缩放到栈中（支持嵌套时间控制）
        /// </summary>
        /// <param name="timeScale">时间缩放值</param>
        void PushTimeScale(float timeScale);
        
        /// <summary>
        /// 从栈中弹出时间缩放
        /// </summary>
        void PopTimeScale();
        
        /// <summary>
        /// 重置时间缩放到默认值
        /// </summary>
        void ResetTimeScale();
        
        /// <summary>
        /// 清空时间缩放栈并重置到默认值
        /// </summary>
        void ClearTimeScaleStack();
    }
}