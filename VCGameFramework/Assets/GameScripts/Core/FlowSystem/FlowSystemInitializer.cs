using System;
using Cysharp.Threading.Tasks;
using Game.Core.FlowSystem.Managers;
using MessagePipe;
using UnityEngine;
using VContainer.Unity;

namespace Game.Core.FlowSystem
{
    /// <summary>
    /// 流程系统初始化器，负责启动和配置整个流程系统
    /// </summary>
    public class FlowSystemInitializer : IStartable, IDisposable
    {
        private readonly IFlowManager _flowManager;
        private readonly ISubFlowManager _subFlowManager;
        private readonly IFlowEventPublisher _eventPublisher;
        private readonly ITimeManager _timeManager;
        private readonly IAudioManager _audioManager;
        private readonly IInputManager _inputManager;
        private readonly ISubscriber<FlowEvent> _flowEventSubscriber;
        
        private IDisposable _flowEventDisposable;
        private bool _initialized = false;
        
        /// <summary>
        /// 构造函数，通过依赖注入获取所需组件
        /// </summary>
        public FlowSystemInitializer(
            IFlowManager flowManager,
            ISubFlowManager subFlowManager,
            IFlowEventPublisher eventPublisher,
            ITimeManager timeManager,
            IAudioManager audioManager,
            IInputManager inputManager,
            ISubscriber<FlowEvent> flowEventSubscriber)
        {
            _flowManager = flowManager;
            _subFlowManager = subFlowManager;
            _eventPublisher = eventPublisher;
            _timeManager = timeManager;
            _audioManager = audioManager;
            _inputManager = inputManager;
            _flowEventSubscriber = flowEventSubscriber;
        }
        
        /// <summary>
        /// 启动流程系统
        /// </summary>
        public void Start()
        {
            Debug.Log("Starting FlowSystemInitializer...");
            
            try
            {
                // 初始化流程系统
                InitializeFlowSystem();
                
                // 设置事件监听
                SetupEventListeners();
                
                // 配置管理器
                ConfigureManagers();
                
                // 启动流程系统
                StartFlowSystemAsync().Forget();
                
                _initialized = true;
                Debug.Log("FlowSystemInitializer started successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to start FlowSystemInitializer: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 初始化流程系统核心组件
        /// </summary>
        private void InitializeFlowSystem()
        {
            Debug.Log("Initializing flow system core components...");
            
            // 设置子流程管理器的父流程引用（将在具体流程中设置）
            // 这里只做基础配置
            
            Debug.Log("Flow system core components initialized");
        }
        
        /// <summary>
        /// 设置事件监听器
        /// </summary>
        private void SetupEventListeners()
        {
            Debug.Log("Setting up flow event listeners...");
            
            // 订阅流程事件进行日志记录和监控
            _flowEventDisposable = _flowEventSubscriber.Subscribe(OnFlowEvent);
            
            // 设置管理器事件监听
            SetupManagerEventListeners();
            
            Debug.Log("Flow event listeners setup completed");
        }
        
        /// <summary>
        /// 设置管理器事件监听器
        /// </summary>
        private void SetupManagerEventListeners()
        {
            // 时间管理器事件
            _timeManager.OnGamePausedChanged += OnGamePausedChanged;
            _timeManager.OnTimeScaleChanged += OnTimeScaleChanged;
            
            // 音频管理器事件
            _audioManager.OnMasterVolumeChanged += OnMasterVolumeChanged;
            _audioManager.OnSFXMuteChanged += OnSFXMuteChanged;
            _audioManager.OnMusicMuteChanged += OnMusicMuteChanged;
            
            // 输入管理器事件
            _inputManager.OnInputStateChanged += OnInputStateChanged;
            _inputManager.OnPausePressed += OnPausePressed;
            _inputManager.OnBackPressed += OnBackPressed;
            _inputManager.OnConfirmPressed += OnConfirmPressed;
            _inputManager.OnMenuPressed += OnMenuPressed;
            
            Debug.Log("Manager event listeners setup completed");
        }
        
        /// <summary>
        /// 配置管理器
        /// </summary>
        private void ConfigureManagers()
        {
            Debug.Log("Configuring managers...");
            
            // 配置时间管理器
            _timeManager.DefaultTimeScale = 1.0f;
            
            // 配置音频管理器（如果需要的话）
            // 音频管理器的默认配置在模块中已设置
            
            // 配置输入管理器
            _inputManager.EnableInput();
            
            Debug.Log("Managers configuration completed");
        }
        
        /// <summary>
        /// 异步启动流程系统
        /// </summary>
        private async UniTaskVoid StartFlowSystemAsync()
        {
            Debug.Log("Starting flow system asynchronously...");
            
            try
            {
                // 等待一帧确保所有组件都已初始化
                await UniTask.Yield();
                
                // 这里可以启动默认流程，比如启动流程
                // 但具体的流程启动应该由应用程序的入口点控制
                // 所以这里只做系统准备工作
                
                Debug.Log("Flow system is ready to use");
                
                // 发布系统就绪事件（如果需要的话）
                PublishSystemReadyEvent();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during async flow system startup: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 发布系统就绪事件
        /// </summary>
        private void PublishSystemReadyEvent()
        {
            Debug.Log("Flow system is ready and operational");
            
            // 这里可以发布一个自定义的系统就绪事件
            // 让其他系统知道流程系统已经可以使用了
        }
        
        #region 事件处理器
        
        /// <summary>
        /// 处理流程事件
        /// </summary>
        private void OnFlowEvent(FlowEvent flowEvent)
        {
            // 记录流程事件（可以根据需要调整日志级别）
            switch (flowEvent.EventType)
            {
                case FlowEventType.FlowEntered:
                    Debug.Log($"Flow entered: {flowEvent.FlowName}");
                    break;
                    
                case FlowEventType.FlowExited:
                    Debug.Log($"Flow exited: {flowEvent.FlowName}");
                    break;
                    
                case FlowEventType.MainFlowSwitched:
                    var fromFlow = flowEvent.Context?.Get<string>("FromFlowName") ?? "None";
                    var toFlow = flowEvent.Context?.Get<string>("ToFlowName") ?? "Unknown";
                    Debug.Log($"Main flow switched: {fromFlow} -> {toFlow}");
                    break;
                    
                case FlowEventType.SubFlowPushed:
                    Debug.Log($"Sub flow pushed: {flowEvent.FlowName}");
                    break;
                    
                case FlowEventType.SubFlowPopped:
                    Debug.Log($"Sub flow popped: {flowEvent.FlowName}");
                    break;
                    
                case FlowEventType.FlowError:
                    Debug.LogError($"Flow error in {flowEvent.FlowName}: {flowEvent.Error?.Message}");
                    break;
                    
                case FlowEventType.FlowPaused:
                    Debug.Log($"Flow paused: {flowEvent.FlowName}");
                    break;
                    
                case FlowEventType.FlowResumed:
                    Debug.Log($"Flow resumed: {flowEvent.FlowName}");
                    break;
            }
        }
        
        /// <summary>
        /// 游戏暂停状态改变
        /// </summary>
        private void OnGamePausedChanged(bool isPaused)
        {
            Debug.Log($"Game pause state changed: {isPaused}");
        }
        
        /// <summary>
        /// 时间缩放改变
        /// </summary>
        private void OnTimeScaleChanged(float timeScale)
        {
            Debug.Log($"Time scale changed: {timeScale}");
        }
        
        /// <summary>
        /// 主音量改变
        /// </summary>
        private void OnMasterVolumeChanged(float volume)
        {
            Debug.Log($"Master volume changed: {volume}");
        }
        
        /// <summary>
        /// 音效静音状态改变
        /// </summary>
        private void OnSFXMuteChanged(bool isMuted)
        {
            Debug.Log($"SFX mute state changed: {isMuted}");
        }
        
        /// <summary>
        /// 音乐静音状态改变
        /// </summary>
        private void OnMusicMuteChanged(bool isMuted)
        {
            Debug.Log($"Music mute state changed: {isMuted}");
        }
        
        /// <summary>
        /// 输入状态改变
        /// </summary>
        private void OnInputStateChanged(InputState inputState)
        {
            Debug.Log($"Input state changed: {inputState}");
        }
        
        /// <summary>
        /// 暂停按键按下
        /// </summary>
        private void OnPausePressed()
        {
            Debug.Log("Pause key pressed - this can be handled by specific flows");
            // 具体的暂停逻辑应该由当前活跃的流程处理
        }
        
        /// <summary>
        /// 返回按键按下
        /// </summary>
        private void OnBackPressed()
        {
            Debug.Log("Back key pressed - this can be handled by specific flows");
            // 具体的返回逻辑应该由当前活跃的流程处理
        }
        
        /// <summary>
        /// 确认按键按下
        /// </summary>
        private void OnConfirmPressed()
        {
            Debug.Log("Confirm key pressed - this can be handled by specific flows");
            // 具体的确认逻辑应该由当前活跃的流程处理
        }
        
        /// <summary>
        /// 菜单按键按下
        /// </summary>
        private void OnMenuPressed()
        {
            Debug.Log("Menu key pressed - this can be handled by specific flows");
            // 具体的菜单逻辑应该由当前活跃的流程处理
        }
        
        #endregion
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_initialized) return;
            
            Debug.Log("Disposing FlowSystemInitializer...");
            
            try
            {
                // 取消事件订阅
                _flowEventDisposable?.Dispose();
                
                // 取消管理器事件监听
                if (_timeManager != null)
                {
                    _timeManager.OnGamePausedChanged -= OnGamePausedChanged;
                    _timeManager.OnTimeScaleChanged -= OnTimeScaleChanged;
                }
                
                if (_audioManager != null)
                {
                    _audioManager.OnMasterVolumeChanged -= OnMasterVolumeChanged;
                    _audioManager.OnSFXMuteChanged -= OnSFXMuteChanged;
                    _audioManager.OnMusicMuteChanged -= OnMusicMuteChanged;
                }
                
                if (_inputManager != null)
                {
                    _inputManager.OnInputStateChanged -= OnInputStateChanged;
                    _inputManager.OnPausePressed -= OnPausePressed;
                    _inputManager.OnBackPressed -= OnBackPressed;
                    _inputManager.OnConfirmPressed -= OnConfirmPressed;
                    _inputManager.OnMenuPressed -= OnMenuPressed;
                }
                
                Debug.Log("FlowSystemInitializer disposed successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error disposing FlowSystemInitializer: {ex.Message}");
            }
        }
    }
}