using System;
using UnityEngine;

namespace Game.Core.FlowSystem.Managers
{
    /// <summary>
    /// 输入状态枚举
    /// </summary>
    public enum InputState
    {
        /// <summary>
        /// 正常输入状态
        /// </summary>
        Normal,
        
        /// <summary>
        /// 输入被禁用
        /// </summary>
        Disabled,
        
        /// <summary>
        /// 仅UI输入
        /// </summary>
        UIOnly,
        
        /// <summary>
        /// 仅游戏输入
        /// </summary>
        GameOnly
    }
    
    /// <summary>
    /// 输入管理器接口，用于管理游戏输入状态和事件
    /// </summary>
    public interface IInputManager
    {
        /// <summary>
        /// 当前输入状态
        /// </summary>
        InputState CurrentInputState { get; }
        
        /// <summary>
        /// 输入是否被禁用
        /// </summary>
        bool IsInputDisabled { get; }
        
        /// <summary>
        /// 是否允许游戏输入
        /// </summary>
        bool IsGameInputEnabled { get; }
        
        /// <summary>
        /// 是否允许UI输入
        /// </summary>
        bool IsUIInputEnabled { get; }
        
        /// <summary>
        /// 输入状态改变事件
        /// </summary>
        event Action<InputState> OnInputStateChanged;
        
        /// <summary>
        /// 暂停按键按下事件
        /// </summary>
        event Action OnPausePressed;
        
        /// <summary>
        /// 返回/取消按键按下事件
        /// </summary>
        event Action OnBackPressed;
        
        /// <summary>
        /// 确认按键按下事件
        /// </summary>
        event Action OnConfirmPressed;
        
        /// <summary>
        /// 菜单按键按下事件
        /// </summary>
        event Action OnMenuPressed;
        
        /// <summary>
        /// 设置输入状态
        /// </summary>
        /// <param name="inputState">新的输入状态</param>
        void SetInputState(InputState inputState);
        
        /// <summary>
        /// 禁用所有输入
        /// </summary>
        void DisableInput();
        
        /// <summary>
        /// 启用所有输入
        /// </summary>
        void EnableInput();
        
        /// <summary>
        /// 设置仅UI输入模式
        /// </summary>
        void SetUIOnlyMode();
        
        /// <summary>
        /// 设置仅游戏输入模式
        /// </summary>
        void SetGameOnlyMode();
        
        /// <summary>
        /// 检查指定按键是否被按下
        /// </summary>
        /// <param name="keyCode">按键代码</param>
        /// <returns>如果被按下返回true</returns>
        bool GetKeyDown(KeyCode keyCode);
        
        /// <summary>
        /// 检查指定按键是否被持续按住
        /// </summary>
        /// <param name="keyCode">按键代码</param>
        /// <returns>如果被按住返回true</returns>
        bool GetKey(KeyCode keyCode);
        
        /// <summary>
        /// 检查指定按键是否被释放
        /// </summary>
        /// <param name="keyCode">按键代码</param>
        /// <returns>如果被释放返回true</returns>
        bool GetKeyUp(KeyCode keyCode);
        
        /// <summary>
        /// 检查鼠标按键是否被按下
        /// </summary>
        /// <param name="button">鼠标按键（0=左键，1=右键，2=中键）</param>
        /// <returns>如果被按下返回true</returns>
        bool GetMouseButtonDown(int button);
        
        /// <summary>
        /// 检查鼠标按键是否被持续按住
        /// </summary>
        /// <param name="button">鼠标按键（0=左键，1=右键，2=中键）</param>
        /// <returns>如果被按住返回true</returns>
        bool GetMouseButton(int button);
        
        /// <summary>
        /// 检查鼠标按键是否被释放
        /// </summary>
        /// <param name="button">鼠标按键（0=左键，1=右键，2=中键）</param>
        /// <returns>如果被释放返回true</returns>
        bool GetMouseButtonUp(int button);
        
        /// <summary>
        /// 获取鼠标位置
        /// </summary>
        /// <returns>鼠标屏幕坐标</returns>
        Vector3 GetMousePosition();
        
        /// <summary>
        /// 获取鼠标滚轮输入
        /// </summary>
        /// <returns>滚轮滚动值</returns>
        float GetMouseScrollDelta();
        
        /// <summary>
        /// 获取水平轴输入（A/D键或左右箭头）
        /// </summary>
        /// <returns>水平输入值（-1到1）</returns>
        float GetHorizontalAxis();
        
        /// <summary>
        /// 获取垂直轴输入（W/S键或上下箭头）
        /// </summary>
        /// <returns>垂直输入值（-1到1）</returns>
        float GetVerticalAxis();
        
        /// <summary>
        /// 获取移动向量
        /// </summary>
        /// <returns>归一化的移动向量</returns>
        Vector2 GetMovementVector();
        
        /// <summary>
        /// 添加输入屏蔽层（用于模态对话框等）
        /// </summary>
        /// <param name="layerName">屏蔽层名称</param>
        void PushInputBlockLayer(string layerName);
        
        /// <summary>
        /// 移除输入屏蔽层
        /// </summary>
        /// <param name="layerName">屏蔽层名称</param>
        void RemoveInputBlockLayer(string layerName);
        
        /// <summary>
        /// 清除所有输入屏蔽层
        /// </summary>
        void ClearInputBlockLayers();
        
        /// <summary>
        /// 检查输入是否被屏蔽
        /// </summary>
        /// <returns>如果被屏蔽返回true</returns>
        bool IsInputBlocked();
    }
}