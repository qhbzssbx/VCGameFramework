using Game.Core;
using Game.Modules.Resource.Infrastructure;
using Game.Modules.Resource.Domain;
using VContainer;
using Cysharp.Threading.Tasks;

namespace Game.Modules.Resource.Application
{
    public class ResourceModule : IModuleWithOrder, IAsyncModule
    {
        //[SerializeField] private ScriptableResourceConfig resourceConfig;
        public int Order => -99;
        public void Configure(IContainerBuilder builder)
        {
            //builder.RegisterInstance(resourceConfig.ToConfig());
            builder.Register<IResourceService, YooAssetResourceProvider>(Lifetime.Singleton);
            builder.Register<ResourceService>(Lifetime.Singleton);
        }

        public async UniTask InitializeAsync(IObjectResolver resolver)
        {
            await resolver.Resolve<ResourceService>().InitializeAsync();
        }
    }
}

