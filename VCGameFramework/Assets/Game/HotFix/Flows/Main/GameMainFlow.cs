using Cysharp.Threading.Tasks;
using Game.Infrastructure.Managers;
using Game.Flows.Sub;
using Game.Modules.Global.Domain;
using Game.Modules.Log.Domain;
using Game.HotFix.FlowSystem;
using Game.HotFix.FlowSystem.BaseClass;
using Game.HotFix.FlowSystem.Interface;
using Game.HotFix.FlowSystem.Manager;
using UnityEngine;

namespace Game.Flows.Main
{
    /// <summary>
    /// 游戏主流程状态
    /// </summary>
    public enum GameMainState
    {
        Initializing,       // 初始化中
        LoadingGameData,    // 加载游戏数据
        EnteringGame,      // 进入游戏
        InGame,            // 游戏中
        Paused,            // 暂停中
        NetworkError,      // 网络错误
        Exiting           // 退出中
    }
    
    /// <summary>
    /// 游戏主流程 - 游戏的核心流程，管理游戏的主要游玩状态
    /// </summary>
    public class GameMainFlow : BaseMainFlow
    {
        private readonly ILogService _logService;

        private readonly INetworkService _networkService;
        private readonly IAccountService _accountService;
        private readonly IInputManager _inputManager;
        private readonly IAudioManager _audioManager;
        private readonly ITimeManager _timeManager;
        private readonly ISubFlowManager _subFlowManager;
        private readonly IFlowManager _flowManager;
        
        private GameMainState _currentState = GameMainState.Initializing;
        private LoginFlow.PlayerLoginInfo _playerInfo;
        private bool _isGameInitialized = false;
        private bool _isPaused = false;
        
        /// <summary>
        /// 游戏主流程优先级
        /// </summary>
        public override int Priority => 100;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public GameMainFlow(
            ILogService logService,

            INetworkService networkService,
            IAccountService accountService,
            IInputManager inputManager,
            IAudioManager audioManager,
            ITimeManager timeManager,
            ISubFlowManager subFlowManager,
            IFlowManager flowManager)
        {
            _logService = logService;

            _networkService = networkService;
            _accountService = accountService;
            _inputManager = inputManager;
            _audioManager = audioManager;
            _timeManager = timeManager;
            _subFlowManager = subFlowManager;
            _flowManager = flowManager;
        }
        
        /// <summary>
        /// 游戏主流程通常不允许切换到其他主流程，除非是退出游戏
        /// </summary>
        public override bool CanSwitchTo(System.Type targetFlowType)
        {
            // 只有在特殊情况下才允许切换主流程
            // 比如网络断线返回登录，或者重新启动游戏
            return targetFlowType == typeof(LoginFlow) || 
                   targetFlowType == typeof(LaunchFlow) || 
                   base.CanSwitchTo(targetFlowType);
        }
        
        /// <summary>
        /// 进入游戏主流程
        /// </summary>
        protected override async UniTask OnEnterInternal(FlowContext context)
        {
            _logService.Info("=== 游戏主流程开始 ===");
            
            try
            {
                // 获取玩家信息
                _playerInfo = context?.GetTyped<LoginFlow.PlayerLoginInfo>();
                var fromFlow = context?.Get<string>("FromFlow") ?? "Unknown";
                
                _logService.Info($"从 {fromFlow} 流程进入游戏，玩家: {_playerInfo?.Username}");
                
                // 设置子流程管理器的父流程
                _subFlowManager.ParentMainFlow = this;
                
                // 初始化游戏系统
                await InitializeGameSystems();
                
                // 加载游戏数据
                await LoadGameData();
                
                // 设置输入处理
                SetupInputHandlers();
                
                // 播放游戏背景音乐
                await PlayGameMusic();
                
                // 进入游戏
                await EnterGameWorld();
                
                _logService.Info("=== 游戏主流程初始化完成 ===");
            }
            catch (System.Exception ex)
            {
                _logService.Error($"游戏主流程初始化失败: {ex.Message}");
                await HandleGameMainError(ex);
                throw;
            }
        }
        
        /// <summary>
        /// 初始化游戏系统
        /// </summary>
        private async UniTask InitializeGameSystems()
        {
            _logService.Info("初始化游戏系统...");
            _currentState = GameMainState.Initializing;
            
            try
            {
                // 初始化游戏世界
                await InitializeGameWorld();
                
                // 初始化玩家系统
                await InitializePlayerSystems();
                
                // 初始化UI系统
                await InitializeUISystem();
                
                // 初始化游戏逻辑系统
                await InitializeGameLogicSystems();
                
                _isGameInitialized = true;
                _logService.Info("游戏系统初始化完成");
            }
            catch (System.Exception ex)
            {
                _logService.Error($"游戏系统初始化失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 初始化游戏世界
        /// </summary>
        private async UniTask InitializeGameWorld()
        {
            _logService.Info("初始化游戏世界...");
            
            // 这里可以初始化游戏场景、地图、天气系统等
            // 例如：
            // - 加载默认场景
            // - 初始化物理系统
            // - 设置光照和渲染
            
            await UniTask.Delay(800); // 模拟初始化时间
            _logService.Info("✓ 游戏世界初始化完成");
        }
        
        /// <summary>
        /// 初始化玩家系统
        /// </summary>
        private async UniTask InitializePlayerSystems()
        {
            _logService.Info("初始化玩家系统...");
            
            // 初始化玩家数据、角色、背包等
            if (_playerInfo != null)
            {
                _logService.Info($"为玩家 {_playerInfo.Username} 初始化系统");
                // 这里可以从服务器同步玩家数据
            }
            
            await UniTask.Delay(600); // 模拟初始化时间
            _logService.Info("✓ 玩家系统初始化完成");
        }
        
        /// <summary>
        /// 初始化UI系统
        /// </summary>
        private async UniTask InitializeUISystem()
        {
            _logService.Info("初始化UI系统...");
            
            // 预加载游戏内的UI预制体
            // 初始化HUD、小地图、聊天系统等
            
            await UniTask.Delay(400); // 模拟初始化时间
            _logService.Info("✓ UI系统初始化完成");
        }
        
        /// <summary>
        /// 初始化游戏逻辑系统
        /// </summary>
        private async UniTask InitializeGameLogicSystems()
        {
            _logService.Info("初始化游戏逻辑系统...");
            
            // 初始化任务系统、成就系统、商店系统等
            
            await UniTask.Delay(500); // 模拟初始化时间
            _logService.Info("✓ 游戏逻辑系统初始化完成");
        }
        
        /// <summary>
        /// 加载游戏数据
        /// </summary>
        private async UniTask LoadGameData()
        {
            _logService.Info("加载游戏数据...");
            _currentState = GameMainState.LoadingGameData;
            
            try
            {
                // 加载玩家数据
                await LoadPlayerData();
                
                // 加载游戏配置
                await LoadGameConfigs();
                
                // 加载必要的资源
                await LoadEssentialGameResources();
                
                _logService.Info("游戏数据加载完成");
            }
            catch (System.Exception ex)
            {
                _logService.Error($"游戏数据加载失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 加载玩家数据
        /// </summary>
        private async UniTask LoadPlayerData()
        {
            _logService.Info("加载玩家数据...");
            
            if (_playerInfo != null)
            {
                // 从服务器或本地加载玩家数据
                // 包括角色信息、进度、设置等
                _logService.Info($"加载玩家 {_playerInfo.Username} 的数据");
            }
            
            await UniTask.Delay(1000); // 模拟数据加载
            _logService.Info("✓ 玩家数据加载完成");
        }
        
        /// <summary>
        /// 加载游戏配置
        /// </summary>
        private async UniTask LoadGameConfigs()
        {
            _logService.Info("加载游戏配置...");
            
            // 加载游戏平衡配置、关卡配置等
            
            await UniTask.Delay(500); // 模拟配置加载
            _logService.Info("✓ 游戏配置加载完成");
        }
        
        /// <summary>
        /// 加载必要的游戏资源
        /// </summary>
        private async UniTask LoadEssentialGameResources()
        {
            _logService.Info("加载必要游戏资源...");
            
            // 预加载常用的游戏资源
            // 比如角色模型、UI图标、音效等
            
            await UniTask.Delay(1200); // 模拟资源加载
            _logService.Info("✓ 必要游戏资源加载完成");
        }
        
        /// <summary>
        /// 设置输入处理
        /// </summary>
        private void SetupInputHandlers()
        {
            _logService.Info("设置游戏输入处理...");
            
            // 游戏主流程中启用所有输入
            _inputManager.EnableInput();
            
            // 监听特殊按键
            _inputManager.OnPausePressed += OnPausePressed;
            _inputManager.OnMenuPressed += OnMenuPressed;
            _inputManager.OnBackPressed += OnBackPressed;
            
            _logService.Info("✓ 输入处理设置完成");
        }
        
        /// <summary>
        /// 播放游戏背景音乐
        /// </summary>
        private async UniTask PlayGameMusic()
        {
            _logService.Info("播放游戏背景音乐...");
            
            // 播放游戏内背景音乐
            // var gameMusic = await _resourceService.LoadAssetAsync<AudioClip>("GameBGM");
            // _audioManager.SwitchMusic(gameMusic, fadeOutDuration: 1f, fadeInDuration: 2f);
            
            await UniTask.Delay(500); // 模拟音乐切换时间
            _logService.Info("✓ 游戏背景音乐播放完成");
        }
        
        /// <summary>
        /// 进入游戏世界
        /// </summary>
        private async UniTask EnterGameWorld()
        {
            _logService.Info("进入游戏世界...");
            _currentState = GameMainState.EnteringGame;
            
            try
            {
                // 显示进入游戏的过渡效果
                await ShowGameEntryTransition();
                
                // 启动游戏进行中的子流程
                await _subFlowManager.PushSubFlow<GamePlaySubFlow>();
                
                _currentState = GameMainState.InGame;
                _logService.Info("✓ 成功进入游戏世界");
            }
            catch (System.Exception ex)
            {
                _logService.Error($"进入游戏世界失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 显示游戏进入过渡效果
        /// </summary>
        private async UniTask ShowGameEntryTransition()
        {
            // 播放进入游戏的过渡动画或效果
            await UniTask.Delay(1000);
        }
        
        /// <summary>
        /// 处理游戏主流程错误
        /// </summary>
        private async UniTask HandleGameMainError(System.Exception ex)
        {
            _logService.Error($"游戏主流程发生严重错误: {ex}");
            
            // 显示错误信息
            await ShowGameErrorDialog(ex.Message);
            
            // 根据错误类型决定处理方式
            if (IsNetworkError(ex))
            {
                await HandleNetworkError();
            }
            else
            {
                // 其他错误，返回登录界面
                await ReturnToLogin("游戏发生错误，请重新登录");
            }
        }
        
        /// <summary>
        /// 显示游戏错误对话框
        /// </summary>
        private async UniTask ShowGameErrorDialog(string message)
        {
            // 显示错误对话框
            await UniTask.Delay(2000);
        }
        
        /// <summary>
        /// 检查是否是网络错误
        /// </summary>
        private bool IsNetworkError(System.Exception ex)
        {
            // 简单的网络错误检查
            return ex.Message.Contains("network") || ex.Message.Contains("connection");
        }
        
        /// <summary>
        /// 处理网络错误
        /// </summary>
        private async UniTask HandleNetworkError()
        {
            _logService.Warning("检测到网络错误，尝试重连...");
            _currentState = GameMainState.NetworkError;
            
            // 尝试重新连接
            for (int i = 0; i < 3; i++)
            {
                await UniTask.Delay(2000);
                
                try
                {
                    // 尝试重连网络
                    // await _networkService.ReconnectAsync();
                    
                    _logService.Info("网络重连成功");
                    _currentState = GameMainState.InGame;
                    return;
                }
                catch
                {
                    _logService.Warning($"重连失败，第 {i + 1} 次尝试");
                }
            }
            
            // 重连失败，返回登录界面
            await ReturnToLogin("网络连接失败，请检查网络后重新登录");
        }
        
        /// <summary>
        /// 返回登录界面
        /// </summary>
        private async UniTask ReturnToLogin(string reason)
        {
            _logService.Info($"返回登录界面: {reason}");
            
            // 清理游戏状态
            await CleanupGameState();
            
            // 创建返回登录的上下文
            var loginContext = FlowContextBuilder.Create()
                .WithData("FromFlow", "GameMain")
                .WithData("ReturnReason", reason)
                .WithTypedData(new LoginFlow.LoginData { ForceManualLogin = true })
                .Build();
            
            // 切换到登录流程
            await _flowManager.SwitchToFlow<LoginFlow>(loginContext);
        }
        
        /// <summary>
        /// 清理游戏状态
        /// </summary>
        private async UniTask CleanupGameState()
        {
            _logService.Info("清理游戏状态...");
            
            try
            {
                // 清理所有子流程
                await _subFlowManager.Clear();
                
                // 保存玩家数据
                if (_playerInfo != null)
                {
                    await SavePlayerData();
                }
                
                // 清理资源
                await CleanupGameResources();
                
                _logService.Info("游戏状态清理完成");
            }
            catch (System.Exception ex)
            {
                _logService.Error($"清理游戏状态时发生错误: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 保存玩家数据
        /// </summary>
        private async UniTask SavePlayerData()
        {
            _logService.Info($"保存玩家 {_playerInfo.Username} 的数据...");
            
            // 保存玩家数据到服务器或本地
            await UniTask.Delay(1000); // 模拟保存时间
            
            _logService.Info("✓ 玩家数据保存完成");
        }
        
        /// <summary>
        /// 清理游戏资源
        /// </summary>
        private async UniTask CleanupGameResources()
        {
            _logService.Info("清理游戏资源...");
            
            // 释放不再需要的游戏资源
            await UniTask.Delay(500); // 模拟清理时间
            
            _logService.Info("✓ 游戏资源清理完成");
        }
        
        #region 游戏流程控制方法
        
        /// <summary>
        /// 暂停游戏
        /// </summary>
        public async UniTask PauseGame()
        {
            if (_isPaused) return;
            
            _logService.Info("暂停游戏");
            _isPaused = true;
            _currentState = GameMainState.Paused;
            
            // 推入暂停菜单子流程
            await _subFlowManager.PushSubFlow<PauseMenuSubFlow>();
        }
        
        /// <summary>
        /// 恢复游戏
        /// </summary>
        public async UniTask ResumeGame()
        {
            if (!_isPaused) return;
            
            _logService.Info("恢复游戏");
            _isPaused = false;
            _currentState = GameMainState.InGame;
            
            // 弹出暂停菜单（如果存在）
            if (_subFlowManager.IsCurrentSubFlow<PauseMenuSubFlow>())
            {
                await _subFlowManager.PopSubFlow();
            }
        }
        
        /// <summary>
        /// 打开设置菜单
        /// </summary>
        public async UniTask OpenSettings()
        {
            _logService.Info("打开设置菜单");
            await _subFlowManager.PushSubFlow<SettingsSubFlow>();
        }
        
        /// <summary>
        /// 打开背包
        /// </summary>
        public async UniTask OpenInventory()
        {
            _logService.Info("打开背包");
            await _subFlowManager.PushSubFlow<InventorySubFlow>();
        }
        
        #endregion
        
        #region 事件处理
        
        /// <summary>
        /// 暂停按键按下
        /// </summary>
        private async void OnPausePressed()
        {
            _logService.Info("检测到暂停按键");
            
            if (_currentState == GameMainState.InGame)
            {
                if (!_isPaused)
                {
                    await PauseGame();
                }
                else
                {
                    await ResumeGame();
                }
            }
        }
        
        /// <summary>
        /// 菜单按键按下
        /// </summary>
        private async void OnMenuPressed()
        {
            _logService.Info("检测到菜单按键");
            
            if (_currentState == GameMainState.InGame && !_isPaused)
            {
                await OpenSettings();
            }
        }
        
        /// <summary>
        /// 返回按键按下
        /// </summary>
        private async void OnBackPressed()
        {
            _logService.Info("检测到返回按键");
            
            // 如果有子流程，先弹出子流程
            if (_subFlowManager.StackDepth > 1) // 大于1是因为GamePlaySubFlow始终在栈底
            {
                await _subFlowManager.PopSubFlow();
            }
            else if (_currentState == GameMainState.InGame)
            {
                // 否则暂停游戏或显示退出确认
                await PauseGame();
            }
        }
        
        #endregion
        
        /// <summary>
        /// 退出游戏主流程
        /// </summary>
        protected override async UniTask OnExitInternal()
        {
            _logService.Info("退出游戏主流程");
            
            try
            {
                // 清理事件监听
                _inputManager.OnPausePressed -= OnPausePressed;
                _inputManager.OnMenuPressed -= OnMenuPressed;
                _inputManager.OnBackPressed -= OnBackPressed;
                
                // 清理游戏状态
                await CleanupGameState();
                
                // 停止游戏音乐
                _audioManager.StopMusic();
                
                _logService.Info("游戏主流程退出完成");
            }
            catch (System.Exception ex)
            {
                _logService.Error($"退出游戏主流程时发生错误: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 流程更新循环
        /// </summary>
        protected override async UniTask OnUpdateInternal()
        {
            // 游戏主流程的更新逻辑
            if (_currentState == GameMainState.InGame && _isGameInitialized)
            {
                // 这里可以添加游戏主循环的更新逻辑
                // 比如检查网络状态、更新游戏系统等
                
                await CheckNetworkStatus();
            }
            
            await base.OnUpdateInternal();
        }
        
        /// <summary>
        /// 检查网络状态
        /// </summary>
        private async UniTask CheckNetworkStatus()
        {
            // 定期检查网络连接状态
            // 如果网络断开，可以显示提示或尝试重连
            
            await UniTask.CompletedTask; // 占位符
        }
    }
}
