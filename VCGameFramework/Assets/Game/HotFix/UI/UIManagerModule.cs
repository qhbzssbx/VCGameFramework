using Game.Core;
using Game.UI.Core;
using VContainer;
using VContainer.Unity;

namespace Game.UI
{
    /// <summary>
    /// UI管理模块
    /// 负责注册UI系统相关的服务和组件
    /// </summary>
    public class UIManagerModule : IModuleWithOrder
    {
        public int Order => -50; // 在基础系统之后，业务模块之前
        
        public void Configure(IContainerBuilder builder)
        {
            // 注册UI资源加载器
            builder.Register<IUIResourceLoader, ScopedUIResourceLoader>(Lifetime.Singleton);

            // 注册UI系统组件
            var c = builder.RegisterComponentOnNewGameObject<UISystem>(Lifetime.Singleton, "UIManager");
            c.AsImplementedInterfaces();
            c.DontDestroyOnLoad();
        }
    }
}
