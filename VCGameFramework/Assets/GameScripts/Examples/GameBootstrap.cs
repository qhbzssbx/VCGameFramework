using UnityEngine;
using VContainer;
using VContainer.Unity;
using Game.Core.FlowSystem;
using Game.Flows.Main;
using Cysharp.Threading.Tasks;

namespace Game.Examples
{
    /// <summary>
    /// 游戏启动管理器 - 展示如何启动流程系统
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Inject] private IFlowManager _flowManager;
        
        private async void Start()
        {
            Debug.Log("游戏启动中...");
            
            try
            {
                // 启动第一个流程 - 启动流程
                await _flowManager.SwitchToFlow<LaunchFlow>();
                
                Debug.Log("流程系统启动成功！");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"流程系统启动失败: {ex.Message}");
            }
        }
        
        private void OnDestroy()
        {
            // 清理流程系统
            _flowManager?.Dispose();
        }
    }
}