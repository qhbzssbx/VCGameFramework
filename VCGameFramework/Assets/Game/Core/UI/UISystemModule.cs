using VContainer;
using VContainer.Unity;
using Game.UI.Core;
using UnityEngine;

namespace Game.Core.UI
{
    /// <summary>
    /// UI系统模块
    /// 用于VContainer依赖注入的UI系统注册
    /// </summary>
    public class UISystemModule : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            // 注册UI管理器接口
            // 如果场景中存在UISystem实例，使用它；否则创建新的
            builder.Register<IUIManager>(resolver =>
            {
                var uiSystem = Object.FindObjectOfType<UISystem>();
                if (uiSystem == null)
                {
                    var go = new GameObject("UISystem");
                    uiSystem = go.AddComponent<UISystem>();
                    Object.DontDestroyOnLoad(go);
                    Debug.Log("[UISystemModule] 创建新的UISystem实例");
                }
                
                return uiSystem;
            }, Lifetime.Singleton);
            
            // 注册UI容器接口
            builder.Register<IUIContainer>(resolver => 
            {
                return resolver.Resolve<IUIManager>() as IUIContainer;
            }, Lifetime.Singleton);
            
            Debug.Log("[UISystemModule] UI系统模块已注册到DI容器");
        }
    }
}