using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Infrastructure.Resource.Core
{
    /// <summary>
    /// 简化的资源服务接口
    /// </summary>
    public interface IResourceService
    {
        /// <summary>
        /// 初始化资源服务
        /// </summary>
        UniTask InitializeAsync();
        
        /// <summary>
        /// 加载资源并返回Handle
        /// </summary>
        UniTask<ResourceHandle<T>> LoadAssetAsync<T>(string assetName) where T : UnityEngine.Object;
        
        /// <summary>
        /// 加载资源并绑定到MonoBehaviour生命周期（简化版本）
        /// </summary>
        UniTask<ResourceHandle<T>> LoadAssetAsync<T>(string assetName, MonoBehaviour owner) where T : UnityEngine.Object;
        
        /// <summary>
        /// 批量加载资源
        /// </summary>
        UniTask<ResourceHandle<T>[]> LoadAssetsAsync<T>(string[] assetNames) where T : UnityEngine.Object;
        
        /// <summary>
        /// 预加载资源（仅缓存，不返回Handle）
        /// </summary>
        UniTask PreloadAssetAsync(string assetName);
    }
}