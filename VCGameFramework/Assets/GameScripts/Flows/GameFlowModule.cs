using Game.Core;
using VContainer;
using Game.Core.FlowSystem;
using Game.Flows.Main;
using Game.Flows.Sub;

namespace Game.Flows
{
    /// <summary>
    /// 游戏流程模块示例，展示如何在具体项目中注册自定义主流程和子流程
    /// </summary>
    public class GameFlowModule : IModuleWithOrder
    {
        /// <summary>
        /// 模块加载优先级，应该在FlowSystemModule之后加载
        /// </summary>
        public int Order => -50;
        
        /// <summary>
        /// 配置容器注册
        /// </summary>
        /// <param name="builder">容器构建器</param>
        public void Configure(IContainerBuilder builder)
        {
            UnityEngine.Debug.Log("Configuring GameFlowModule...");
            
            // 注册主流程
            RegisterMainFlows(builder);
            
            // 注册子流程
            RegisterSubFlows(builder);
            
            UnityEngine.Debug.Log("GameFlowModule configuration completed");
        }
        
        /// <summary>
        /// 注册主流程
        /// </summary>
        private void RegisterMainFlows(IContainerBuilder builder)
        {
            // 注册所有主流程
            FlowSystemModule.RegisterFlow<LaunchFlow>(builder);
            FlowSystemModule.RegisterFlow<HotUpdateFlow>(builder);
            FlowSystemModule.RegisterFlow<LoginFlow>(builder);
            FlowSystemModule.RegisterFlow<GameMainFlow>(builder);
            
            UnityEngine.Debug.Log("Main flows registered successfully");
        }
        
        /// <summary>
        /// 注册子流程
        /// </summary>
        private void RegisterSubFlows(IContainerBuilder builder)
        {
            // 注册所有子流程
            FlowSystemModule.RegisterSubFlow<GamePlaySubFlow>(builder);
            FlowSystemModule.RegisterSubFlow<PauseMenuSubFlow>(builder);
            FlowSystemModule.RegisterSubFlow<SettingsSubFlow>(builder);
            FlowSystemModule.RegisterSubFlow<InventorySubFlow>(builder);
            
            UnityEngine.Debug.Log("Sub flows registered successfully");
        }
    }
}