using Game.Core;
using VContainer;
using VContainer.Unity;
using MessagePipe;

namespace Game.HotFix.Infrastructure.Bootstrap
{
    /// <summary>
    /// 项目启动引导类 - 展示如何集成流程系统
    /// </summary>
    public class ProjectBootstrap : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // 注册 MessagePipe 全局消息总线
            builder.RegisterMessagePipe();
            // 使用 ModuleLoader 自动发现和注册所有模块
            // 包括 FlowSystemModule, GameFlowModule 等所有实现了 IModule 的模块
            // ModuleLoader.RegisterAllModules(builder);
            SmartModuleLoader.RegisterAllModules(builder);

            builder.RegisterEntryPoint<GameBootstrap>();
        }
    }
}
