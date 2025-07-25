using VContainer;
using VContainer.Unity;
using Game.Core.FlowSystem;
using Game.Core.FlowSystem.Examples;

namespace Game.Core
{
    /// <summary>
    /// 项目启动引导类 - 展示如何集成流程系统
    /// </summary>
    public class ProjectBootstrap : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // 注册核心流程系统
            builder.RegisterModule<FlowSystemModule>();
            
            // 注册具体的游戏流程
            builder.RegisterModule<GameFlowModule>();
            
            // 其他项目模块...
            // builder.RegisterModule<UIModule>();
            // builder.RegisterModule<NetworkModule>();
        }
    }
}