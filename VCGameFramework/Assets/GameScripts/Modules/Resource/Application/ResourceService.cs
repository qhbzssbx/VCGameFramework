using Cysharp.Threading.Tasks;
using Game.Modules.Resource.Domain;
using UnityEngine;

namespace Game.Modules.Resource.Application
{
    public class ResourceService : IResourceService
    {
        private IResourceService _impl;
        public ResourceService(IResourceService impl) => _impl = impl;

        public UniTask InitializeAsync() => _impl.InitializeAsync();
        public UniTask<T> LoadAssetAsync<T>(string assetName) where T : UnityEngine.Object => _impl.LoadAssetAsync<T>(assetName);
        public void UnloadAsset(Object asset) => _impl.UnloadAsset(asset);
    }
}

