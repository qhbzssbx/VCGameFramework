using System.Threading;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Game.Core.FlowSystem;
using Game.Flows.Main;
using Game.Modules.Log.Domain;
using Cysharp.Threading.Tasks;

namespace Game.Infrastructure.Bootstrap
{
    /// <summary>
    /// 游戏启动管理器 - 负责初始化和启动整个游戏流程系统
    /// </summary>
    public class GameBootstrap : IAsyncStartable, System.IDisposable, ITickable
    {
        [Inject] private IFlowManager _flowManager;
        [Inject] private ILogService _logService;

        private bool _isStarted = false;
        private bool _disposed = false;

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            
            try
            {
                _logService?.Info("GameBootstrap 开始清理资源...");
                
                // 清理流程系统
                _flowManager?.Dispose();
                
                _disposed = true;
                _logService?.Info("GameBootstrap 资源清理完成");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"GameBootstrap 清理资源时发生错误: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Unity生命周期销毁时的清理
        /// </summary>
        private void OnDestroy()
        {
            Dispose();
        }

        /// <summary>
        /// 异步启动游戏系统
        /// </summary>
        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            if (_isStarted) 
            {
                _logService.Warning("GameBootstrap 已经启动，忽略重复启动请求");
                return;
            }
            
            if (_disposed)
            {
                _logService.Error("GameBootstrap 已被销毁，无法启动");
                throw new System.ObjectDisposedException(nameof(GameBootstrap));
            }

            _isStarted = true;
            _logService.Info("=== 游戏启动流程开始 ===");
            
            try
            {
                // 检查取消令牌
                cancellation.ThrowIfCancellationRequested();
                
                // 验证依赖注入
                await ValidateDependencies();
                
                // 启动第一个流程 - 启动流程
                _logService.Info("启动LaunchFlow...");
                var launchContext = FlowContextBuilder.Create()
                    .WithData("FromBootstrap", true)
                    .WithData("StartTime", System.DateTime.Now)
                    .Build();
                    
                await _flowManager.SwitchToFlow<LaunchFlow>(launchContext);

                _logService.Info("=== 游戏启动流程完成 ===");
            }
            catch (System.OperationCanceledException)
            {
                _logService.Warning("游戏启动被用户取消");
                throw;
            }
            catch (System.Exception ex)
            {
                _logService.Error($"游戏启动失败: {ex}");
                await HandleStartupError(ex);
                throw;
            }
        }
        
        /// <summary>
        /// 验证关键依赖是否正确注入
        /// </summary>
        private async UniTask ValidateDependencies()
        {
            _logService.Info("验证关键依赖注入...");
            
            if (_flowManager == null)
            {
                throw new System.InvalidOperationException("FlowManager 未正确注入");
            }
            
            if (_logService == null)
            {
                Debug.LogError("LogService 未正确注入");
                throw new System.InvalidOperationException("LogService 未正确注入");
            }
            
            // FlowManager设计为构造后即可用，无需异步初始化
            _logService.Info("✓ 依赖验证完成 - FlowManager构造后即可用");
            
            // 避免编译器警告
            await UniTask.CompletedTask;
        }
        
        /// <summary>
        /// 处理启动错误
        /// </summary>
        private async UniTask HandleStartupError(System.Exception ex)
        {
            _logService.Error($"处理启动错误: {ex.GetType().Name}");
            
            try
            {
                // 尝试显示错误对话框
                await ShowErrorDialog("游戏启动失败", ex.Message);
                
                // 清理FlowManager资源（如果存在的话）
                if (_flowManager != null)
                {
                    _logService.Info("清理Flow系统资源...");
                    _flowManager.Dispose();
                }
            }
            catch (System.Exception cleanupEx)
            {
                _logService.Error($"清理启动错误时发生异常: {cleanupEx.Message}");
            }
        }
        
        /// <summary>
        /// 显示错误对话框
        /// </summary>
        private async UniTask ShowErrorDialog(string title, string message)
        {
            // 这里可以集成UI系统显示错误对话框
            // 目前使用简单的日志输出
            _logService.Error($"错误对话框 - {title}: {message}");
            
            // 模拟对话框显示时间
            await UniTask.Delay(1000);
        }

        public void Tick()
        {
            // 游戏主循环更新逻辑
            // 可以在这里添加需要每帧更新的逻辑
        }
    }
}
