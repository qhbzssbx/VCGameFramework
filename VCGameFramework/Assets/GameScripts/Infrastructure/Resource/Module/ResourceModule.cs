using Game.Core;
using Game.Infrastructure.Resource.Core;
using Game.Infrastructure.Resource.Providers.YooAssetProvider;
using VContainer;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Infrastructure.Resource.Module
{
    /// <summary>
    /// 资源模块 - 基础设施层级别的资源管理系统
    /// </summary>
    public class ResourceModule : IModuleWithOrder, IAsyncModule
    {
        /// <summary>
        /// 基础设施层高优先级，确保在业务模块前初始化
        /// </summary>
        public int Order => -500;
        
        public void Configure(IContainerBuilder builder)
        {
            // 直接注册为接口实现 - 更简洁高效
            builder.Register<IResourceService, YooAssetResourceService>(Lifetime.Singleton);
            
            Debug.Log("ResourceModule配置完成 - Infrastructure基础设施版本");
        }

        public async UniTask InitializeAsync(IObjectResolver resolver)
        {
            Debug.Log("ResourceModule初始化开始 - Infrastructure版本");
            
            var resourceService = resolver.Resolve<IResourceService>();
            await resourceService.InitializeAsync();
            
            Debug.Log("ResourceModule初始化完成 - 资源系统已就绪");
        }
    }
}