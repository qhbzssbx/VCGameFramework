using Cysharp.Threading.Tasks;
using Game.Infrastructure.Managers;
using Game.Modules.Global.Domain;
using Game.Modules.Log.Domain;
using UnityEngine;
using Game.UI;
using Game.Core.UI;
using Game.HotFix.FlowSystem;
using Game.HotFix.FlowSystem.BaseClass;
using Game.HotFix.FlowSystem.Interface;
using Game.HotFix.FlowSystem.Manager;

namespace Game.Flows.Main
{
    /// <summary>
    /// 热更新检查状态
    /// </summary>
    public enum HotUpdateState
    {
        CheckingVersion,    // 检查版本
        DownloadingUpdate,  // 下载更新
        Installing,         // 安装更新
        Completed,          // 完成
        Failed,            // 失败
        Skipped            // 跳过
    }
    
    /// <summary>
    /// 热更新流程 - 负责检查和下载游戏更新
    /// </summary>
    public class HotUpdateFlow : BaseMainFlow
    {
        private readonly ILogService _logService;

        private readonly INetworkService _networkService;
        private readonly IInputManager _inputManager;
        private readonly IFlowManager _flowManager;
        private readonly IUIManager _uiManager;

        private HotFixPanel hotFixPanel;
        private GeneralPopUp generalPopUp;
        
        private HotUpdateState _currentState = HotUpdateState.CheckingVersion;
        private float _downloadProgress = 0f;
        private string _updateMessage = "";
        private bool _updateRequired = false;
        private bool _userConfirmed = false;
        
        /// <summary>
        /// 热更新流程优先级
        /// </summary>
        public override int Priority => 10;

        /// <summary>
        /// 构造函数
        /// </summary>
        public HotUpdateFlow(
            ILogService logService,

            INetworkService networkService,
            IInputManager inputManager,
            IFlowManager flowManager,
            IUIManager uISystem)
        {
            _logService = logService;

            _networkService = networkService;
            _inputManager = inputManager;
            _flowManager = flowManager;
            _uiManager = uISystem;
        }
        
        /// <summary>
        /// 热更新流程通常只能切换到登录流程
        /// </summary>
        public override bool CanSwitchTo(System.Type targetFlowType)
        {
            // 热更新完成后通常切换到登录流程
            return targetFlowType == typeof(LoginFlow) || base.CanSwitchTo(targetFlowType);
        }
        
        /// <summary>
        /// 进入热更新流程
        /// </summary>
        protected override async UniTask OnEnterInternal(FlowContext context)
        {
            _logService.Info("=== 热更新流程开始 ===");
            
            try
            {
                // 获取来源信息
                var fromFlow = context?.Get<string>("FromFlow") ?? "Unknown";
                _logService.Info($"从 {fromFlow} 流程进入热更新");
                
                // 显示热更新界面
                await ShowHotUpdateUI();
                
                // 设置输入监听
                //SetupInputHandlers();
                
                // 开始热更新检查
                await StartHotUpdateProcess();
                
                _logService.Info("=== 热更新流程完成 ===");
            }
            catch (System.Exception ex)
            {
                _logService.Error($"热更新流程发生错误: {ex.Message} {ex.StackTrace}");
                await HandleHotUpdateError(ex);
                throw;
            }
        }
        
        /// <summary>
        /// 显示热更新UI界面
        /// </summary>
        private async UniTask ShowHotUpdateUI()
        {
            _logService.Info("显示热更新界面");

            // 这里应该显示热更新的UI界面
            // 包括进度条、状态文本、取消按钮等

            hotFixPanel = await _uiManager.ShowAsync<HotFixPanel>();
        }
        
        /// <summary>
        /// 设置输入处理
        /// </summary>
        private void SetupInputHandlers()
        {
            // 在热更新过程中，通常只允许基本的UI输入
            _inputManager.SetUIOnlyMode();
            
            // 监听返回按键，允许用户取消更新
            _inputManager.OnBackPressed += OnBackPressed;
        }
        
        /// <summary>
        /// 开始热更新流程
        /// </summary>
        private async UniTask StartHotUpdateProcess()
        {
            _logService.Info("开始热更新检查流程");
            
            // 检查网络连接
            if (!await CheckNetworkConnection())
            {
                _logService.Warning("网络连接不可用，跳过热更新");
                await SkipHotUpdate();
                return;
            }
            
            // 检查版本更新
            await CheckForUpdates();
            
            if (_updateRequired)
            {
                // 等待用户确认
                await WaitForUserConfirmation();
                
                if (_userConfirmed)
                {
                    // 下载更新
                    await DownloadUpdates();
                    
                    // 安装更新
                    await InstallUpdates();
                }
                else
                {
                    await SkipHotUpdate();
                    return;
                }
            }
            
            // 完成热更新，切换到下一个流程
            await CompleteHotUpdate();
        }
        
        /// <summary>
        /// 检查网络连接
        /// </summary>
        private async UniTask<bool> CheckNetworkConnection()
        {
            _logService.Info("检查网络连接...");
            _updateMessage = "检查网络连接...";
            
            // 这里应该调用实际的网络检查逻辑
            // 可以通过INetworkService来检查网络状态
            
            await UniTask.Delay(1000); // 模拟网络检查时间
            
            // 模拟网络检查结果
            bool isConnected = Application.internetReachability != NetworkReachability.NotReachable;
            
            if (isConnected)
            {
                _logService.Info("✓ 网络连接正常");
                return true;
            }
            else
            {
                _logService.Warning("✗ 网络连接不可用");
                return false;
            }
        }
        
        /// <summary>
        /// 检查更新
        /// </summary>
        private async UniTask CheckForUpdates()
        {
            _logService.Info("检查版本更新...");
            _currentState = HotUpdateState.CheckingVersion;
            _updateMessage = "正在检查更新...";
            
            try
            {
                // 获取当前版本
                string currentVersion = Application.version;
                _logService.Info($"当前版本: {currentVersion}");
                
                // 从服务器获取最新版本信息
                var latestVersionInfo = await FetchLatestVersionInfo();
                
                if (latestVersionInfo != null)
                {
                    _logService.Info($"最新版本: {latestVersionInfo.Version}");
                    
                    // 比较版本
                    if (IsVersionNewer(latestVersionInfo.Version, currentVersion))
                    {
                        _updateRequired = true;
                        _updateMessage = $"New Version: {latestVersionInfo.Version}\\n{latestVersionInfo.Description}";

                        _logService.Info("发现新版本，需要更新");
                    }
                    else
                    {
                        _updateRequired = false;
                        _updateMessage = "当前已是最新版本";
                        _logService.Info("当前已是最新版本");
                    }
                }
                else
                {
                    _logService.Warning("无法获取版本信息");
                    _updateRequired = false;
                }
            }
            catch (System.Exception ex)
            {
                _logService.Error($"检查更新失败: {ex.Message}");
                _updateRequired = false;
            }
        }
        
        /// <summary>
        /// 获取最新版本信息
        /// </summary>
        private async UniTask<VersionInfo> FetchLatestVersionInfo()
        {
            // 这里应该调用实际的版本检查API
            // 模拟从服务器获取版本信息
            
            await UniTask.Delay(2000); // 模拟网络请求时间
            
            // 模拟版本信息
            return new VersionInfo
            {
                Version = "1.0.1",
                Description = "修复了一些已知问题\\n优化了游戏性能",
                DownloadUrl = "https://example.com/update.zip",
                FileSize = 1024 * 1024 * 50 // 50MB
            };
        }
        
        /// <summary>
        /// 比较版本号
        /// </summary>
        private bool IsVersionNewer(string newVersion, string currentVersion)
        {
            // 简单的版本比较逻辑
            // 实际项目中可能需要更复杂的版本比较算法
            
            var newVer = System.Version.Parse(newVersion);
            var currentVer = System.Version.Parse(currentVersion);
            
            return newVer > currentVer;
        }
        
        /// <summary>
        /// 等待用户确认更新
        /// </summary>
        private async UniTask WaitForUserConfirmation()
        {
            _logService.Info("等待用户确认更新...");
            
            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
            
            // 显示确认弹窗
            generalPopUp = await _uiManager.ShowAsync<GeneralPopUp, GeneralPopUpParams>(
                new GeneralPopUpParams(_updateMessage + @"\n\nStart Download？",
                    () => tcs.SetResult(true),
                    () => tcs.SetResult(false))
                );

            
            // 等待用户选择
            _userConfirmed = await tcs.Task;
            
            _logService.Info($"用户选择: {(_userConfirmed ? "确认更新" : "跳过更新")}");
        }
        
        /// <summary>
        /// 下载更新
        /// </summary>
        private async UniTask DownloadUpdates()
        {
            _logService.Info("开始下载更新...");
            _currentState = HotUpdateState.DownloadingUpdate;
            _downloadProgress = 0f;
            
            try
            {
                // 模拟下载进度
                for (int i = 0; i <= 100; i += 2)
                {
                    _downloadProgress = i / 100f;
                    _updateMessage = $"正在下载更新... {i}%";
                    
                    await UniTask.Delay(50); // 模拟下载时间
                    
                    // 检查是否被取消
                    if (_currentState != HotUpdateState.DownloadingUpdate)
                    {
                        _logService.Info("下载被取消");
                        return;
                    }
                }
                
                _logService.Info("✓ 更新下载完成");
            }
            catch (System.Exception ex)
            {
                _logService.Error($"下载更新失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 安装更新
        /// </summary>
        private async UniTask InstallUpdates()
        {
            _logService.Info("开始安装更新...");
            _currentState = HotUpdateState.Installing;
            _updateMessage = "正在安装更新...";
            
            try
            {
                // 模拟安装过程
                await UniTask.Delay(3000);
                
                _logService.Info("✓ 更新安装完成");
                _currentState = HotUpdateState.Completed;
            }
            catch (System.Exception ex)
            {
                _logService.Error($"安装更新失败: {ex.Message}");
                _currentState = HotUpdateState.Failed;
                throw;
            }
        }
        
        /// <summary>
        /// 跳过热更新
        /// </summary>
        private async UniTask SkipHotUpdate()
        {
            _logService.Info("跳过热更新");
            _currentState = HotUpdateState.Skipped;
            _updateMessage = "已跳过更新";
            
            await UniTask.Delay(1000);
        }
        
        /// <summary>
        /// 完成热更新流程
        /// </summary>
        private async UniTask CompleteHotUpdate()
        {
            _logService.Info("热更新流程完成，准备切换到登录流程");
            
            // 创建传递给登录流程的上下文
            var loginContext = FlowContextBuilder.Create()
                .WithData("FromFlow", "HotUpdate")
                .WithData("UpdateState", _currentState.ToString())
                .WithData("UpdateRequired", _updateRequired)
                .WithTypedData(_flowManager)
                .Build();
            
            // 延迟一下让用户看到完成状态
            await UniTask.Delay(1500);
            
            // 切换到登录流程
            await _flowManager.SwitchToFlow<LoginFlow>(loginContext);
        }
        
        /// <summary>
        /// 处理热更新错误
        /// </summary>
        private async UniTask HandleHotUpdateError(System.Exception ex)
        {
            _logService.Error($"热更新发生错误: {ex}");
            _currentState = HotUpdateState.Failed;
            _updateMessage = $"更新失败: {ex.Message}\\n\\n按任意键继续游戏";
            
            // 等待用户按键继续
            await WaitForUserInput();
            
            // 即使更新失败也继续到登录流程
            await CompleteHotUpdate();
        }
        
        /// <summary>
        /// 等待用户输入
        /// </summary>
        private async UniTask WaitForUserInput()
        {
            bool keyPressed = false;
            System.Action onKeyPressed = () => keyPressed = true;
            
            _inputManager.OnConfirmPressed += onKeyPressed;
            _inputManager.OnBackPressed += onKeyPressed;
            
            while (!keyPressed)
            {
                await UniTask.Delay(100);
            }
            
            _inputManager.OnConfirmPressed -= onKeyPressed;
            _inputManager.OnBackPressed -= onKeyPressed;
        }
        
        #region 事件处理
        
        
        /// <summary>
        /// 用户按下返回键
        /// </summary>
        private void OnBackPressed()
        {
            _logService.Info("用户取消操作");
            
            switch (_currentState)
            {
                case HotUpdateState.CheckingVersion:
                    // 跳过更新
                    SkipHotUpdate().Forget();
                    break;
                    
                case HotUpdateState.DownloadingUpdate:
                    // 取消下载
                    _currentState = HotUpdateState.Failed;
                    _logService.Info("用户取消下载");
                    break;
                    
                default:
                    // 其他状态下不允许取消
                    break;
            }
        }
        
        #endregion
        
        /// <summary>
        /// 退出热更新流程
        /// </summary>
        protected override async UniTask OnExitInternal()
        {
            _logService.Info("退出热更新流程");
            
            // 清理事件监听
            _inputManager.OnBackPressed -= OnBackPressed;
            
            // 恢复正常输入模式
            _inputManager.EnableInput();
            
            // 隐藏热更新UI
            await HideHotUpdateUI();
        }
        
        /// <summary>
        /// 隐藏热更新UI
        /// </summary>
        private async UniTask HideHotUpdateUI()
        {
            // 隐藏热更新界面
            await UniTask.Delay(200);
        }
        
        #region 数据类
        
        /// <summary>
        /// 版本信息
        /// </summary>
        private class VersionInfo
        {
            public string Version { get; set; }
            public string Description { get; set; }
            public string DownloadUrl { get; set; }
            public long FileSize { get; set; }
        }
        
        #endregion
    }
}

