using Cysharp.Threading.Tasks;
using Game.Core.FlowSystem.Managers;
using Game.Modules.Log.Domain;

namespace Game.Core.FlowSystem.SubFlows
{
    /// <summary>
    /// 暂停菜单子流程 - 游戏暂停时显示的菜单
    /// </summary>
    public class PauseMenuSubFlow : BaseSubFlow
    {
        private readonly ILogService _logService;
        private readonly ITimeManager _timeManager;
        private readonly IAudioManager _audioManager;
        private readonly IInputManager _inputManager;
        private readonly ISubFlowManager _subFlowManager;
        
        private bool _isMenuVisible = false;
        
        /// <summary>
        /// 暂停菜单需要暂停父流程
        /// </summary>
        public override bool ShouldPauseParent => true;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public PauseMenuSubFlow(
            ILogService logService,
            ITimeManager timeManager,
            IAudioManager audioManager,
            IInputManager inputManager,
            ISubFlowManager subFlowManager)
        {
            _logService = logService;
            _timeManager = timeManager;
            _audioManager = audioManager;
            _inputManager = inputManager;
            _subFlowManager = subFlowManager;
        }
        
        /// <summary>
        /// 进入暂停菜单
        /// </summary>
        protected override async UniTask OnEnterInternal(FlowContext context)
        {
            _logService.Info("进入暂停菜单");
            
            // 暂停游戏
            _timeManager.PauseGame();
            _audioManager.PauseAllSFX();
            
            // 设置UI输入模式
            _inputManager.SetUIOnlyMode();
            
            // 显示暂停菜单UI
            await ShowPauseMenuUI();
            
            // 设置输入处理
            SetupPauseMenuInput();
            
            // 播放菜单音效
            PlayMenuSFX();
            
            _logService.Info("暂停菜单显示完成");
        }
        
        /// <summary>
        /// 显示暂停菜单UI
        /// </summary>
        private async UniTask ShowPauseMenuUI()
        {
            _logService.Info("显示暂停菜单UI");
            
            // 这里应该显示暂停菜单的UI界面
            // 包括继续游戏、设置、退出游戏等按钮
            
            await UniTask.Delay(400); // 模拟UI显示动画
            _isMenuVisible = true;
            
            _logService.Info("✓ 暂停菜单UI显示完成");
        }
        
        /// <summary>
        /// 设置暂停菜单输入处理
        /// </summary>
        private void SetupPauseMenuInput()
        {
            // 监听暂停菜单的特殊按键
            _inputManager.OnConfirmPressed += OnResumePressed;
            _inputManager.OnBackPressed += OnResumePressed;
            _inputManager.OnMenuPressed += OnSettingsPressed;
        }
        
        /// <summary>
        /// 播放菜单音效
        /// </summary>
        private void PlayMenuSFX()
        {
            // 播放暂停菜单打开音效
            // var pauseMenuSound = ResourceService.LoadAsset<AudioClip>("PauseMenuOpen");
            // _audioManager.PlaySFX(pauseMenuSound);
        }
        
        /// <summary>
        /// 恢复游戏
        /// </summary>
        public async UniTask ResumeGame()
        {
            _logService.Info("从暂停菜单恢复游戏");
            
            // 播放恢复音效
            PlayResumeGameSFX();
            
            // 弹出当前子流程（这会触发OnExitInternal）
            await _subFlowManager.PopSubFlow();
        }
        
        /// <summary>
        /// 打开设置菜单
        /// </summary>
        public async UniTask OpenSettings()
        {
            _logService.Info("从暂停菜单打开设置");
            
            // 推入设置子流程
            var settingsContext = FlowContextBuilder.Create()
                .WithData("FromFlow", "PauseMenu")
                .Build();
                
            await _subFlowManager.PushSubFlow<SettingsSubFlow>(settingsContext);
        }
        
        /// <summary>
        /// 退出到主菜单
        /// </summary>
        public async UniTask ExitToMainMenu()
        {
            _logService.Info("退出到主菜单");
            
            // 显示确认对话框
            bool confirmed = await ShowExitConfirmDialog();
            
            if (confirmed)
            {
                // 清理所有子流程
                await _subFlowManager.PopToRoot();
                
                // 这里可以切换到主菜单流程
                // 或者返回登录流程
                _logService.Info("确认退出到主菜单");
            }
        }
        
        /// <summary>
        /// 显示退出确认对话框
        /// </summary>
        private async UniTask<bool> ShowExitConfirmDialog()
        {
            _logService.Info("显示退出确认对话框");
            
            // 这里应该显示确认对话框
            // 返回用户的选择结果
            
            await UniTask.Delay(2000); // 模拟用户思考时间
            
            // 模拟用户选择（实际应该由UI事件决定）
            bool userConfirmed = false; // 这里应该从UI获取
            
            _logService.Info($"用户选择: {(userConfirmed ? "确认退出" : "取消退出")}");
            return userConfirmed;
        }
        
        /// <summary>
        /// 播放恢复游戏音效
        /// </summary>
        private void PlayResumeGameSFX()
        {
            // 播放恢复游戏音效
            // var resumeSound = ResourceService.LoadAsset<AudioClip>("ResumeGame");
            // _audioManager.PlaySFX(resumeSound);
        }
        
        #region 事件处理
        
        /// <summary>
        /// 恢复按键按下
        /// </summary>
        private async void OnResumePressed()
        {
            if (_isMenuVisible)
            {
                await ResumeGame();
            }
        }
        
        /// <summary>
        /// 设置按键按下
        /// </summary>
        private async void OnSettingsPressed()
        {
            if (_isMenuVisible)
            {
                await OpenSettings();
            }
        }
        
        #endregion
        
        /// <summary>
        /// 退出暂停菜单
        /// </summary>
        protected override async UniTask OnExitInternal()
        {
            _logService.Info("退出暂停菜单");
            
            // 清理输入事件监听
            _inputManager.OnConfirmPressed -= OnResumePressed;
            _inputManager.OnBackPressed -= OnResumePressed;
            _inputManager.OnMenuPressed -= OnSettingsPressed;
            
            // 隐藏暂停菜单UI
            await HidePauseMenuUI();
            
            // 恢复游戏状态
            _timeManager.ResumeGame();
            _audioManager.ResumeAllSFX();
            
            // 恢复正常输入模式
            _inputManager.EnableInput();
            
            _logService.Info("暂停菜单退出完成");
        }
        
        /// <summary>
        /// 隐藏暂停菜单UI
        /// </summary>
        private async UniTask HidePauseMenuUI()
        {
            _logService.Info("隐藏暂停菜单UI");
            
            _isMenuVisible = false;
            
            // 播放菜单关闭动画
            await UniTask.Delay(300); // 模拟UI隐藏动画
            
            _logService.Info("✓ 暂停菜单UI隐藏完成");
        }
    }
}