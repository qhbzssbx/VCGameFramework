using Cysharp.Threading.Tasks;
using Game.Core.FlowSystem;
using Game.Infrastructure.Managers;
using UnityEngine;
using VContainer;

namespace Game.Examples
{
    /// <summary>
    /// 流程系统使用示例
    /// 展示如何在游戏中使用流程系统进行状态管理
    /// </summary>
    public class FlowSystemUsageExample : MonoBehaviour
    {
        [Header("示例配置")]
        [SerializeField] private bool enableAutoDemo = false;
        [SerializeField] private float demoDelay = 2f;
        
        // 通过依赖注入获取流程系统组件
        private IFlowManager _flowManager;
        private ISubFlowManager _subFlowManager;
        private ITimeManager _timeManager;
        private IAudioManager _audioManager;
        private IInputManager _inputManager;
        
        [Inject]
        public void Construct(
            IFlowManager flowManager,
            ISubFlowManager subFlowManager,
            ITimeManager timeManager,
            IAudioManager audioManager,
            IInputManager inputManager)
        {
            _flowManager = flowManager;
            _subFlowManager = subFlowManager;
            _timeManager = timeManager;
            _audioManager = audioManager;
            _inputManager = inputManager;
            
            Debug.Log("FlowSystemUsageExample dependencies injected");
        }
        
        private void Start()
        {
            if (enableAutoDemo)
            {
                StartAutoDemo().Forget();
            }
        }
        
        /// <summary>
        /// 自动演示流程系统功能
        /// </summary>
        private async UniTaskVoid StartAutoDemo()
        {
            Debug.Log("Starting FlowSystem demo...");
            
            await UniTask.Delay((int)(demoDelay * 1000));
            
            // 演示时间管理器
            await DemoTimeManager();
            
            await UniTask.Delay((int)(demoDelay * 1000));
            
            // 演示音频管理器
            await DemoAudioManager();
            
            await UniTask.Delay((int)(demoDelay * 1000));
            
            // 演示输入管理器
            await DemoInputManager();
            
            Debug.Log("FlowSystem demo completed");
        }
        
        /// <summary>
        /// 演示时间管理器功能
        /// </summary>
        private async UniTask DemoTimeManager()
        {
            Debug.Log("=== Time Manager Demo ===");
            
            // 正常时间
            Debug.Log($"Current time scale: {_timeManager.GameTimeScale}");
            await UniTask.Delay(1000);
            
            // 慢动作
            Debug.Log("Setting slow motion (0.5x)");
            _timeManager.SetTimeScale(0.5f);
            await UniTask.Delay(2000, ignoreTimeScale: true);
            
            // 快进
            Debug.Log("Setting fast forward (2x)");
            _timeManager.SetTimeScale(2f);
            await UniTask.Delay(1000, ignoreTimeScale: true);
            
            // 暂停游戏
            Debug.Log("Pausing game");
            _timeManager.PauseGame();
            await UniTask.Delay(1000, ignoreTimeScale: true);
            
            // 恢复游戏
            Debug.Log("Resuming game");
            _timeManager.ResumeGame();
            
            // 重置时间缩放
            _timeManager.ResetTimeScale();
            Debug.Log("Time scale reset to normal");
        }
        
        /// <summary>
        /// 演示音频管理器功能
        /// </summary>
        private async UniTask DemoAudioManager()
        {
            Debug.Log("=== Audio Manager Demo ===");
            
            // 调整音量
            Debug.Log("Setting master volume to 0.5");
            _audioManager.MasterVolume = 0.5f;
            await UniTask.Delay(500);
            
            // 静音音效
            Debug.Log("Muting SFX");
            _audioManager.SetSFXMuted(true);
            await UniTask.Delay(1000);
            
            Debug.Log("Unmuting SFX");
            _audioManager.SetSFXMuted(false);
            await UniTask.Delay(500);
            
            // 恢复正常音量
            _audioManager.MasterVolume = 1.0f;
            Debug.Log("Audio settings restored");
        }
        
        /// <summary>
        /// 演示输入管理器功能
        /// </summary>
        private async UniTask DemoInputManager()
        {
            Debug.Log("=== Input Manager Demo ===");
            
            // 禁用输入
            Debug.Log("Disabling input");
            _inputManager.DisableInput();
            await UniTask.Delay(1000);
            
            // 仅UI输入
            Debug.Log("Setting UI only mode");
            _inputManager.SetUIOnlyMode();
            await UniTask.Delay(1000);
            
            // 仅游戏输入
            Debug.Log("Setting game only mode");
            _inputManager.SetGameOnlyMode();
            await UniTask.Delay(1000);
            
            // 添加输入屏蔽层
            Debug.Log("Adding input block layer");
            _inputManager.PushInputBlockLayer("DemoBlocker");
            await UniTask.Delay(1000);
            
            // 移除输入屏蔽层
            Debug.Log("Removing input block layer");
            _inputManager.RemoveInputBlockLayer("DemoBlocker");
            
            // 恢复正常输入
            _inputManager.EnableInput();
            Debug.Log("Input restored to normal");
        }
        
        #region 公共方法供外部调用
        
        /// <summary>
        /// 手动演示主流程切换
        /// </summary>
        [ContextMenu("Demo Main Flow Switching")]
        public async void DemoMainFlowSwitching()
        {
            Debug.Log("=== Main Flow Switching Demo ===");
            
            // 注意：这里需要实际的流程类才能工作
            // 这只是展示API的使用方式
            
            try
            {
                // 切换到启动流程
                // await _flowManager.SwitchToFlow<LaunchFlow>();
                
                // 切换到登录流程
                // var loginContext = FlowContextBuilder.Create()
                //     .WithData("FromFlow", "Launch")
                //     .WithTypedData(new LoginData { AutoLogin = false })
                //     .Build();
                // await _flowManager.SwitchToFlow<LoginFlow>(loginContext);
                
                Debug.Log("Main flow switching demo completed (requires actual flow implementations)");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Main flow demo failed (expected without actual flows): {ex.Message}");
            }
        }
        
        /// <summary>
        /// 手动演示子流程管理
        /// </summary>
        [ContextMenu("Demo Sub Flow Management")]
        public async void DemoSubFlowManagement()
        {
            Debug.Log("=== Sub Flow Management Demo ===");
            
            try
            {
                // 压入暂停菜单子流程
                // await _subFlowManager.PushSubFlow<PauseMenuSubFlow>();
                
                // 压入设置子流程
                // var settingsContext = FlowContextBuilder.Create()
                //     .WithData("Section", "Audio")
                //     .Build();
                // await _subFlowManager.PushSubFlow<SettingsSubFlow>(settingsContext);
                
                // 弹出到暂停菜单
                // await _subFlowManager.PopSubFlow();
                
                // 弹出到根
                // await _subFlowManager.PopToRoot();
                
                Debug.Log("Sub flow management demo completed (requires actual sub flow implementations)");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Sub flow demo failed (expected without actual flows): {ex.Message}");
            }
        }
        
        /// <summary>
        /// 手动测试管理器功能
        /// </summary>
        [ContextMenu("Test Managers")]
        public void TestManagers()
        {
            Debug.Log("=== Manager Test ===");
            
            Debug.Log($"Time Manager - Is Paused: {_timeManager.IsGamePaused}, Time Scale: {_timeManager.GameTimeScale}");
            Debug.Log($"Audio Manager - Master Volume: {_audioManager.MasterVolume}, SFX Muted: {_audioManager.IsSFXMuted}");
            Debug.Log($"Input Manager - State: {_inputManager.CurrentInputState}, Game Input: {_inputManager.IsGameInputEnabled}");
            
            Debug.Log("Manager test completed");
        }
        
        #endregion
        
        #region Unity编辑器GUI
        
        private void OnGUI()
        {
            if (!Application.isPlaying) return;
            
            var rect = new Rect(10, 150, 300, 200);
            GUI.Box(rect, "Flow System Example Controls");
            
            if (GUI.Button(new Rect(15, 175, 140, 30), "Demo Time Manager"))
            {
                DemoTimeManager().Forget();
            }
            
            if (GUI.Button(new Rect(160, 175, 140, 30), "Demo Audio Manager"))
            {
                DemoAudioManager().Forget();
            }
            
            if (GUI.Button(new Rect(15, 210, 140, 30), "Demo Input Manager"))
            {
                DemoInputManager().Forget();
            }
            
            if (GUI.Button(new Rect(160, 210, 140, 30), "Test Managers"))
            {
                TestManagers();
            }
            
            if (GUI.Button(new Rect(15, 245, 285, 30), "Demo Main Flow (Requires Implementation)"))
            {
                DemoMainFlowSwitching();
            }
            
            if (GUI.Button(new Rect(15, 280, 285, 30), "Demo Sub Flow (Requires Implementation)"))
            {
                DemoSubFlowManagement();
            }
        }
        
        #endregion
    }
}