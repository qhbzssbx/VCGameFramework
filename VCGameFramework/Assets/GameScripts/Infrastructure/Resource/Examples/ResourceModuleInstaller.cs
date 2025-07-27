using Game.Infrastructure.Resource.Providers.YooAssetProvider;
using Game.Infrastructure.Resource.Core;
using UnityEngine;
using VContainer;

namespace Game.Infrastructure.Resource.Examples
{
    /// <summary>
    /// 资源模块VContainer配置示例 - Infrastructure版本
    /// </summary>
    public class ResourceModuleInstaller : MonoBehaviour
    {
        public void Install(IContainerBuilder builder)
        {
            // 直接注册为接口实现 - 最佳实践
            builder.Register<IResourceService, YooAssetResourceService>(Lifetime.Singleton);

            Debug.Log("资源模块已注册到VContainer - Infrastructure基础设施版本");
        }
    }
}