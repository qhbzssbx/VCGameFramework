using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.FlowSystem.Managers
{
    /// <summary>
    /// 输入管理器实现
    /// </summary>
    public class InputManager : MonoBehaviour, IInputManager
    {
        [Header("Key Bindings")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        [SerializeField] private KeyCode backKey = KeyCode.Escape;
        [SerializeField] private KeyCode confirmKey = KeyCode.Return;
        [SerializeField] private KeyCode menuKey = KeyCode.Tab;
        
        [Header("Alternative Key Bindings")]
        [SerializeField] private KeyCode[] alternativePauseKeys = { KeyCode.P };
        [SerializeField] private KeyCode[] alternativeBackKeys = { KeyCode.Backspace };
        [SerializeField] private KeyCode[] alternativeConfirmKeys = { KeyCode.Space, KeyCode.KeypadEnter };
        [SerializeField] private KeyCode[] alternativeMenuKeys = { KeyCode.M };
        
        private InputState _currentInputState = InputState.Normal;
        private readonly HashSet<string> _inputBlockLayers = new();
        
        /// <summary>
        /// 当前输入状态
        /// </summary>
        public InputState CurrentInputState => _currentInputState;
        
        /// <summary>
        /// 输入是否被禁用
        /// </summary>
        public bool IsInputDisabled => _currentInputState == InputState.Disabled || IsInputBlocked();
        
        /// <summary>
        /// 是否允许游戏输入
        /// </summary>
        public bool IsGameInputEnabled => !IsInputDisabled && 
            (_currentInputState == InputState.Normal || _currentInputState == InputState.GameOnly);
        
        /// <summary>
        /// 是否允许UI输入
        /// </summary>
        public bool IsUIInputEnabled => !IsInputDisabled && 
            (_currentInputState == InputState.Normal || _currentInputState == InputState.UIOnly);
        
        /// <summary>
        /// 输入状态改变事件
        /// </summary>
        public event Action<InputState> OnInputStateChanged;
        
        /// <summary>
        /// 暂停按键按下事件
        /// </summary>
        public event Action OnPausePressed;
        
        /// <summary>
        /// 返回/取消按键按下事件
        /// </summary>
        public event Action OnBackPressed;
        
        /// <summary>
        /// 确认按键按下事件
        /// </summary>
        public event Action OnConfirmPressed;
        
        /// <summary>
        /// 菜单按键按下事件
        /// </summary>
        public event Action OnMenuPressed;
        
        /// <summary>
        /// 初始化输入管理器
        /// </summary>
        private void Awake()
        {
            Debug.Log("InputManager initialized");
        }
        
        /// <summary>
        /// 更新输入检查
        /// </summary>
        private void Update()
        {
            if (IsInputDisabled) return;
            
            CheckSpecialKeys();
        }
        
        /// <summary>
        /// 检查特殊按键
        /// </summary>
        private void CheckSpecialKeys()
        {
            // 检查暂停键
            if (CheckKeyDown(pauseKey, alternativePauseKeys))
            {
                OnPausePressed?.Invoke();
                Debug.Log("Pause key pressed");
            }
            
            // 检查返回键
            if (CheckKeyDown(backKey, alternativeBackKeys))
            {
                OnBackPressed?.Invoke();
                Debug.Log("Back key pressed");
            }
            
            // 检查确认键
            if (CheckKeyDown(confirmKey, alternativeConfirmKeys))
            {
                OnConfirmPressed?.Invoke();
                Debug.Log("Confirm key pressed");
            }
            
            // 检查菜单键
            if (CheckKeyDown(menuKey, alternativeMenuKeys))
            {
                OnMenuPressed?.Invoke();
                Debug.Log("Menu key pressed");
            }
        }
        
        /// <summary>
        /// 检查主键或备选键是否被按下
        /// </summary>
        private bool CheckKeyDown(KeyCode mainKey, KeyCode[] alternativeKeys)
        {
            if (Input.GetKeyDown(mainKey))
                return true;
                
            foreach (var key in alternativeKeys)
            {
                if (Input.GetKeyDown(key))
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 设置输入状态
        /// </summary>
        /// <param name="inputState">新的输入状态</param>
        public void SetInputState(InputState inputState)
        {
            if (_currentInputState == inputState) return;
            
            var oldState = _currentInputState;
            _currentInputState = inputState;
            
            Debug.Log($"Input state changed: {oldState} -> {inputState}");
            OnInputStateChanged?.Invoke(inputState);
        }
        
        /// <summary>
        /// 禁用所有输入
        /// </summary>
        public void DisableInput()
        {
            SetInputState(InputState.Disabled);
        }
        
        /// <summary>
        /// 启用所有输入
        /// </summary>
        public void EnableInput()
        {
            SetInputState(InputState.Normal);
        }
        
        /// <summary>
        /// 设置仅UI输入模式
        /// </summary>
        public void SetUIOnlyMode()
        {
            SetInputState(InputState.UIOnly);
        }
        
        /// <summary>
        /// 设置仅游戏输入模式
        /// </summary>
        public void SetGameOnlyMode()
        {
            SetInputState(InputState.GameOnly);
        }
        
        /// <summary>
        /// 检查指定按键是否被按下
        /// </summary>
        /// <param name="keyCode">按键代码</param>
        /// <returns>如果被按下返回true</returns>
        public bool GetKeyDown(KeyCode keyCode)
        {
            return IsGameInputEnabled && Input.GetKeyDown(keyCode);
        }
        
        /// <summary>
        /// 检查指定按键是否被持续按住
        /// </summary>
        /// <param name="keyCode">按键代码</param>
        /// <returns>如果被按住返回true</returns>
        public bool GetKey(KeyCode keyCode)
        {
            return IsGameInputEnabled && Input.GetKey(keyCode);
        }
        
        /// <summary>
        /// 检查指定按键是否被释放
        /// </summary>
        /// <param name="keyCode">按键代码</param>
        /// <returns>如果被释放返回true</returns>
        public bool GetKeyUp(KeyCode keyCode)
        {
            return IsGameInputEnabled && Input.GetKeyUp(keyCode);
        }
        
        /// <summary>
        /// 检查鼠标按键是否被按下
        /// </summary>
        /// <param name="button">鼠标按键（0=左键，1=右键，2=中键）</param>
        /// <returns>如果被按下返回true</returns>
        public bool GetMouseButtonDown(int button)
        {
            return IsGameInputEnabled && Input.GetMouseButtonDown(button);
        }
        
        /// <summary>
        /// 检查鼠标按键是否被持续按住
        /// </summary>
        /// <param name="button">鼠标按键（0=左键，1=右键，2=中键）</param>
        /// <returns>如果被按住返回true</returns>
        public bool GetMouseButton(int button)
        {
            return IsGameInputEnabled && Input.GetMouseButton(button);
        }
        
        /// <summary>
        /// 检查鼠标按键是否被释放
        /// </summary>
        /// <param name="button">鼠标按键（0=左键，1=右键，2=中键）</param>   
        /// <returns>如果被释放返回true</returns>
        public bool GetMouseButtonUp(int button)
        {
            return IsGameInputEnabled && Input.GetMouseButtonUp(button);
        }
        
        /// <summary>
        /// 获取鼠标位置
        /// </summary>
        /// <returns>鼠标屏幕坐标</returns>
        public Vector3 GetMousePosition()
        {
            return Input.mousePosition;
        }
        
        /// <summary>
        /// 获取鼠标滚轮输入
        /// </summary>
        /// <returns>滚轮滚动值</returns>
        public float GetMouseScrollDelta()
        {
            return IsGameInputEnabled ? Input.mouseScrollDelta.y : 0f;
        }
        
        /// <summary>
        /// 获取水平轴输入（A/D键或左右箭头）
        /// </summary>
        /// <returns>水平输入值（-1到1）</returns>
        public float GetHorizontalAxis()
        {
            if (!IsGameInputEnabled) return 0f;
            
            float horizontal = 0f;
            
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                horizontal -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                horizontal += 1f;
                
            // 也支持Input.GetAxis，但只在游戏输入启用时
            if (horizontal == 0f)
            {
                horizontal = Input.GetAxis("Horizontal");
            }
            
            return horizontal;
        }
        
        /// <summary>
        /// 获取垂直轴输入（W/S键或上下箭头）
        /// </summary>
        /// <returns>垂直输入值（-1到1）</returns>
        public float GetVerticalAxis()
        {
            if (!IsGameInputEnabled) return 0f;
            
            float vertical = 0f;
            
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                vertical -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                vertical += 1f;
                
            // 也支持Input.GetAxis，但只在游戏输入启用时
            if (vertical == 0f)
            {
                vertical = Input.GetAxis("Vertical");
            }
            
            return vertical;
        }
        
        /// <summary>
        /// 获取移动向量
        /// </summary>
        /// <returns>归一化的移动向量</returns>
        public Vector2 GetMovementVector()
        {
            if (!IsGameInputEnabled) return Vector2.zero;
            
            var movement = new Vector2(GetHorizontalAxis(), GetVerticalAxis());
            
            // 归一化以防止对角线移动过快
            if (movement.magnitude > 1f)
            {
                movement = movement.normalized;
            }
            
            return movement;
        }
        
        /// <summary>
        /// 添加输入屏蔽层（用于模态对话框等）
        /// </summary>
        /// <param name="layerName">屏蔽层名称</param>
        public void PushInputBlockLayer(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                Debug.LogWarning("Cannot add input block layer with null or empty name");
                return;
            }
            
            _inputBlockLayers.Add(layerName);
            Debug.Log($"Added input block layer: {layerName} (Total layers: {_inputBlockLayers.Count})");
        }
        
        /// <summary>
        /// 移除输入屏蔽层
        /// </summary>
        /// <param name="layerName">屏蔽层名称</param>
        public void RemoveInputBlockLayer(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                Debug.LogWarning("Cannot remove input block layer with null or empty name");
                return;
            }
            
            if (_inputBlockLayers.Remove(layerName))
            {
                Debug.Log($"Removed input block layer: {layerName} (Remaining layers: {_inputBlockLayers.Count})");
            }
            else
            {
                Debug.LogWarning($"Input block layer not found: {layerName}");
            }
        }
        
        /// <summary>
        /// 清除所有输入屏蔽层
        /// </summary>
        public void ClearInputBlockLayers()
        {
            var layerCount = _inputBlockLayers.Count;
            _inputBlockLayers.Clear();
            Debug.Log($"Cleared all input block layers ({layerCount} layers removed)");
        }
        
        /// <summary>
        /// 检查输入是否被屏蔽
        /// </summary>
        /// <returns>如果被屏蔽返回true</returns>
        public bool IsInputBlocked()
        {
            return _inputBlockLayers.Count > 0;
        }
        
        /// <summary>
        /// 在编辑器中显示调试信息
        /// </summary>
        private void OnGUI()
        {
            if (!Debug.isDebugBuild) return;
            
            var rect = new Rect(10, 10, 300, 120);
            GUI.Box(rect, "Input Manager Debug");
            
            GUI.Label(new Rect(15, 30, 290, 20), $"Input State: {_currentInputState}");
            GUI.Label(new Rect(15, 50, 290, 20), $"Game Input Enabled: {IsGameInputEnabled}");
            GUI.Label(new Rect(15, 70, 290, 20), $"UI Input Enabled: {IsUIInputEnabled}");
            GUI.Label(new Rect(15, 90, 290, 20), $"Input Blocked: {IsInputBlocked()}");
            GUI.Label(new Rect(15, 110, 290, 20), $"Block Layers: {_inputBlockLayers.Count}");
        }
    }
}