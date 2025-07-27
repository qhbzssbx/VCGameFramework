using Cysharp.Threading.Tasks;
using Game.Infrastructure.Managers;
using Game.Core.FlowSystem;
using Game.Modules.Log.Domain;

namespace Game.Flows.Sub
{
    /// <summary>
    /// 游戏进行中子流程 - 正常游戏状态下的子流程
    /// </summary>
    public class GamePlaySubFlow : BaseSubFlow
    {
        private readonly ILogService _logService;
        private readonly IInputManager _inputManager;
        private readonly IAudioManager _audioManager;
        private readonly ITimeManager _timeManager;
        
        /// <summary>
        /// 游戏进行中不需要暂停父流程
        /// </summary>
        public override bool ShouldPauseParent => false;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public GamePlaySubFlow(
            ILogService logService,
            IInputManager inputManager,
            IAudioManager audioManager,
            ITimeManager timeManager)
        {
            _logService = logService;
            _inputManager = inputManager;
            _audioManager = audioManager;
            _timeManager = timeManager;
        }
        
        /// <summary>
        /// 进入游戏进行中状态
        /// </summary>
        protected override async UniTask OnEnterInternal(FlowContext context)
        {
            _logService.Info("进入游戏进行中状态");
            
            // 确保游戏处于正常运行状态
            _timeManager.ResumeGame();
            _inputManager.EnableInput();
            
            // 显示游戏HUD界面
            await ShowGameHUD();
            
            // 开始游戏逻辑更新
            StartGameLogicUpdate();
            
            _logService.Info("游戏进行中状态初始化完成");
        }
        
        /// <summary>
        /// 显示游戏HUD界面
        /// </summary>
        private async UniTask ShowGameHUD()
        {
            _logService.Info("显示游戏HUD界面");
            
            // 这里应该显示游戏的主要UI元素
            // 比如血条、小地图、快捷栏等
            
            await UniTask.Delay(300); // 模拟UI显示时间
            _logService.Info("✓ 游戏HUD界面显示完成");
        }
        
        /// <summary>
        /// 开始游戏逻辑更新
        /// </summary>
        private void StartGameLogicUpdate()
        {
            _logService.Info("开始游戏逻辑更新");
            
            // 启动游戏的各种更新逻辑
            // 比如AI更新、物理更新、动画更新等
            
            _logService.Info("✓ 游戏逻辑更新已启动");
        }
        
        /// <summary>
        /// 流程更新
        /// </summary>
        protected override async UniTask OnUpdateInternal()
        {
            // 游戏进行中的更新逻辑
            // 这里可以处理游戏的主循环逻辑
            
            await UniTask.CompletedTask;
        }
        
        /// <summary>
        /// 退出游戏进行中状态
        /// </summary>
        protected override async UniTask OnExitInternal()
        {
            _logService.Info("退出游戏进行中状态");
            
            // 停止游戏逻辑更新
            StopGameLogicUpdate();
            
            // 隐藏游戏HUD界面
            await HideGameHUD();
            
            _logService.Info("游戏进行中状态清理完成");
        }
        
        /// <summary>
        /// 停止游戏逻辑更新
        /// </summary>
        private void StopGameLogicUpdate()
        {
            _logService.Info("停止游戏逻辑更新");
            
            // 停止各种游戏更新逻辑
            
            _logService.Info("✓ 游戏逻辑更新已停止");
        }
        
        /// <summary>
        /// 隐藏游戏HUD界面
        /// </summary>
        private async UniTask HideGameHUD()
        {
            _logService.Info("隐藏游戏HUD界面");
            
            // 隐藏游戏UI元素
            
            await UniTask.Delay(200); // 模拟UI隐藏时间
            _logService.Info("✓ 游戏HUD界面隐藏完成");
        }
    }
}