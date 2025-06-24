using Cysharp.Threading.Tasks;


namespace Game.Modules.Resource.Domain
{
    public interface IResourceService
    {
        UniTask InitializeAsync();
        UniTask<T> LoadAssetAsync<T>(string assetName) where T : UnityEngine.Object;
        void UnloadAsset(UnityEngine.Object asset);
    }
}