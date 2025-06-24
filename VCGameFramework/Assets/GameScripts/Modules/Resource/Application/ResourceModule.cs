using Game.Core;
using Game.Modules.Resource.Infrastructure;
using Game.Modules.Resource.Domain;
using VContainer;

namespace Game.Modules.Resource.Application
{
    public class ResourceModule : IModuleWithOrder
    {
        //[SerializeField] private ScriptableResourceConfig resourceConfig;
        public int Order => -99;
        public void Configure(IContainerBuilder builder)
        {
            //builder.RegisterInstance(resourceConfig.ToConfig());
            builder.Register<IResourceService, YooAssetResourceProvider>(Lifetime.Singleton);
            builder.Register<ResourceService>(Lifetime.Singleton);
        }
    }
}

