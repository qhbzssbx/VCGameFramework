using Cysharp.Threading.Tasks;
using Game.Infrastructure.Managers;
using Game.Core.FlowSystem;
using Game.Modules.Log.Domain;
using UnityEngine;
using Game.UI;
using Game.Core.UI;

namespace Game.Flows.Main
{
    /// <summary>
    /// 启动流程 - 游戏的第一个流程，负责初始化最小必备系统
    /// </summary>
    public class LaunchFlow : BaseMainFlow
    {
        private readonly ITimeManager _timeManager;
        private readonly IAudioManager _audioManager;
        private readonly IInputManager _inputManager;
        private readonly ILogService _logService;

        private readonly IFlowManager _flowManager;

        private readonly IUIManager _uiManager;

        private LaunchPanel launchPanel;
        
        /// <summary>
        /// 启动流程优先级最高
        /// </summary>
        public override int Priority => 0;

        /// <summary>
        /// 构造函数
        /// </summary>
        public LaunchFlow(
            ITimeManager timeManager,
            IAudioManager audioManager,
            IInputManager inputManager,
            ILogService logService,

            IFlowManager flowManager,
            IUIManager uISystem)
        {
            _timeManager = timeManager;
            _audioManager = audioManager;
            _inputManager = inputManager;
            _logService = logService;

            _flowManager = flowManager;
            _uiManager = uISystem;
        }
        
        /// <summary>
        /// 启动流程可以切换到任何其他流程
        /// </summary>
        public override bool CanSwitchTo(System.Type targetFlowType)
        {
            // 启动流程结束后可以切换到任何流程
            return base.CanSwitchTo(targetFlowType);
        }
        
        /// <summary>
        /// 进入启动流程
        /// </summary>
        protected override async UniTask OnEnterInternal(FlowContext context)
        {
            _logService.Info("=== 游戏启动流程开始 ===");
            
            try
            {
                // 显示启动画面
                await ShowLaunchScreen();
                
                // 初始化核心系统
                await InitializeCoreSystem();
                
                // 初始化管理器
                await InitializeManagers();
                
                // 加载基础资源
                await LoadEssentialResources();
                
                // 执行启动动画
                await PlayLaunchAnimation();
                
                // 自动切换到热更新流程
                await SwitchToNextFlow();
                
                _logService.Info("=== 游戏启动流程完成 ===");
            }
            catch (System.Exception ex)
            {
                _logService.Error($"启动流程发生错误: {ex.Message}");
                await HandleLaunchError(ex);
                throw;
            }
        }
        
        /// <summary>
        /// 显示启动画面
        /// </summary>
        private async UniTask ShowLaunchScreen()
        {
            _logService.Info("显示启动画面...");

            // 这里可以显示游戏Logo、公司Logo等
            // 可以通过UI系统显示启动界面

            // 模拟启动画面显示时间
            await UniTask.Delay(500);

            
            
            _logService.Info("启动画面显示完成");
        }
        
        /// <summary>
        /// 初始化核心系统
        /// </summary>
        private async UniTask InitializeCoreSystem()
        {
            _logService.Info("初始化核心系统...");
            
            // 初始化日志系统（通常已经初始化了）
            _logService.Info("✓ 日志系统已就绪");
            
            // 初始化资源系统
            // TODO: 当资源系统实现后，在这里添加资源系统的初始化逻辑
            _logService.Info("✓ 资源系统初始化完成");
            
            // 设置应用程序配置
            SetupApplicationSettings();
            
            // 模拟系统初始化时间
            await UniTask.Delay(500);
            
            _logService.Info("核心系统初始化完成");
        }
        
        /// <summary>
        /// 设置应用程序配置
        /// </summary>
        private void SetupApplicationSettings()
        {
            // 设置目标帧率
            Application.targetFrameRate = 60;
            
            // 设置屏幕常亮（移动设备）
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            
            // 设置质量等级（可以从配置文件读取）
            QualitySettings.SetQualityLevel(QualitySettings.GetQualityLevel());
            
            _logService.Info("应用程序配置完成");
        }
        
        /// <summary>
        /// 初始化管理器
        /// </summary>
        private async UniTask InitializeManagers()
        {
            _logService.Info("初始化管理器...");
            
            // 初始化时间管理器
            _timeManager.ResetTimeScale();
            _logService.Info("✓ 时间管理器初始化完成");
            
            // 初始化音频管理器
            _audioManager.MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
            _audioManager.SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
            _audioManager.MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.6f);
            _logService.Info("✓ 音频管理器初始化完成");
            
            // 初始化输入管理器
            _inputManager.EnableInput();
            _logService.Info("✓ 输入管理器初始化完成");
            
            // 模拟管理器初始化时间
            await UniTask.Delay(300);
            
            _logService.Info("所有管理器初始化完成");
        }
        
        /// <summary>
        /// 加载基础资源
        /// </summary>
        private async UniTask LoadEssentialResources()
        {
            _logService.Info("加载基础资源...");
            
            try
            {
                // 加载基础UI资源
                await LoadBasicUIResources();
                
                // 加载基础音频资源
                await LoadBasicAudioResources();
                
                // 加载系统配置
                await LoadSystemConfigs();
                
                _logService.Info("基础资源加载完成");
            }
            catch (System.Exception ex)
            {
                _logService.Error($"基础资源加载失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 加载基础UI资源
        /// </summary>
        private async UniTask LoadBasicUIResources()
        {
            // 这里可以预加载一些基础的UI预制体
            // 比如加载界面、提示框等
            launchPanel = await _uiManager.ShowAsync<LaunchPanel>();
            
            _logService.Info("✓ 基础UI资源加载完成");
            await UniTask.Delay(200); // 模拟加载时间
        }
        
        /// <summary>
        /// 加载基础音频资源
        /// </summary>
        private async UniTask LoadBasicAudioResources()
        {
            // 这里可以预加载一些基础的音效
            // 比如按钮点击音效、错误提示音等
            
            _logService.Info("✓ 基础音频资源加载完成");
            await UniTask.Delay(200); // 模拟加载时间
        }
        
        /// <summary>
        /// 加载系统配置
        /// </summary>
        private async UniTask LoadSystemConfigs()
        {
            // 加载游戏配置文件
            // 比如画质设置、按键绑定等
            
            _logService.Info("✓ 系统配置加载完成");
            await UniTask.Delay(100); // 模拟加载时间
        }
        
        /// <summary>
        /// 播放启动动画
        /// </summary>
        private async UniTask PlayLaunchAnimation()
        {
            _logService.Info("播放启动动画...");

            // 播放启动动画，等待动画播放完毕
            await launchPanel.PlayAnim();
            
            _logService.Info("启动动画播放完成");
        }
        
        /// <summary>
        /// 切换到下一个流程
        /// </summary>
        private async UniTask SwitchToNextFlow()
        {
            _logService.Info("准备切换到热更新流程...");
            
            // 创建传递给下一个流程的上下文数据
            var nextContext = FlowContextBuilder.Create()
                .WithData("FromFlow", "Launch")
                .WithData("LaunchTime", System.DateTime.Now)
                .WithTypedData(_flowManager)
                .Build();
            
            // 切换到热更新流程
            await _flowManager.SwitchToFlow<HotUpdateFlow>(nextContext);
        }
        
        /// <summary>
        /// 处理启动错误
        /// </summary>
        private async UniTask HandleLaunchError(System.Exception ex)
        {
            _logService.Error($"启动流程出现严重错误: {ex}");
            
            // 这里可以显示错误对话框
            // 允许用户选择重试或退出游戏
            
            // 模拟错误处理时间
            await UniTask.Delay(1000);
            
            // 可以根据错误类型决定是否重试或退出
            // 对于启动流程的致命错误，通常需要退出游戏
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
        /// <summary>
        /// 退出启动流程
        /// </summary>
        protected override async UniTask OnExitInternal()
        {
            _logService.Info("退出启动流程");

            // 清理启动流程相关的资源
            // 比如隐藏启动界面等
            
            await _uiManager.HideAsync<LaunchPanel>();
        }
    }
}

