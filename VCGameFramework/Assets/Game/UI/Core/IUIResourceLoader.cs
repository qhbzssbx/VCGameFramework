using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

namespace Game.UI.Core
{
    /// <summary>
    /// UI资源加载器接口
    /// </summary>
    public interface IUIResourceLoader : IDisposable
    {
        /// <summary>
        /// 异步加载UI预制体
        /// </summary>
        /// <param name="assetKey">资源键名</param>
        /// <returns>UI预制体GameObject</returns>
        UniTask<GameObject> LoadUIPrefabAsync(string assetKey);
        
        /// <summary>
        /// 加载UI纹理
        /// </summary>
        /// <param name="assetKey">资源键名</param>
        /// <returns>纹理对象</returns>
        UniTask<Texture2D> LoadUITextureAsync(string assetKey);
        
        /// <summary>
        /// 加载UI精灵
        /// </summary>
        /// <param name="assetKey">资源键名</param>
        /// <returns>精灵对象</returns>
        UniTask<Sprite> LoadUISpriteAsync(string assetKey);
        
        /// <summary>
        /// 预加载UI资源
        /// </summary>
        /// <param name="assetKeys">资源键名列表</param>
        /// <returns>加载任务</returns>
        UniTask PreloadUIAssetsAsync(params string[] assetKeys);
        
        /// <summary>
        /// 释放UI资源
        /// </summary>
        /// <param name="assetKey">资源键名</param>
        void ReleaseUIAsset(string assetKey);
        
        /// <summary>
        /// 清理所有UI资源缓存
        /// </summary>
        void ClearCache();
    }
}