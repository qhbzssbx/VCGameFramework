using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Game.HotFix.AssetSystem.Core;

namespace Game.UI.Core
{
    /// <summary>
    /// UI 资源加载适配器：基于新 AssetRegistry 实现，提供与原 IUIResourceLoader 相同接口。
    /// - 直接使用 AssetRegistry 进行资源管理
    /// - 支持手动释放和缓存清理
    /// - 利用 TTL 机制自动管理资源生命周期
    /// </summary>
    public sealed class ScopedUIResourceLoader : IUIResourceLoader
    {
        private readonly IAssetRegistry _registry;
        private bool _disposed;
        private readonly CancellationTokenSource _cts = new();
        private readonly Dictionary<string, Type> _loadedAssetTypes = new();

        private string packageName = "DefaultPackage";

        public ScopedUIResourceLoader(Game.HotFix.AssetSystem.Core.IAssetRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public async UniTask<GameObject> LoadUIPrefabAsync(string assetKey)
        {
            ThrowIfDisposed();
            var address = GetPath(assetKey);
            var asset = await _registry.AcquireAsync<GameObject>(packageName, address, _cts.Token);
            
            // 记录加载的资源类型，用于释放时确定类型
            lock (_loadedAssetTypes)
            {
                _loadedAssetTypes[assetKey] = typeof(GameObject);
            }
            
            return asset;
        }

        public async UniTask<Texture2D> LoadUITextureAsync(string assetKey)
        {
            ThrowIfDisposed();
            var address = GetPath(assetKey);
            var asset = await _registry.AcquireAsync<Texture2D>(packageName, address, _cts.Token);
            
            lock (_loadedAssetTypes)
            {
                _loadedAssetTypes[assetKey] = typeof(Texture2D);
            }
            
            return asset;
        }

        public async UniTask<Sprite> LoadUISpriteAsync(string assetKey)
        {
            ThrowIfDisposed();
            var address = GetPath(assetKey);
            var asset = await _registry.AcquireAsync<Sprite>(packageName, address, _cts.Token);
            
            lock (_loadedAssetTypes)
            {
                _loadedAssetTypes[assetKey] = typeof(Sprite);
            }
            
            return asset;
        }

        // public async UniTask PreloadUIAssetsAsync(params string[] assetKeys)
        // {
        //     ThrowIfDisposed();
        //     if (assetKeys == null || assetKeys.Length == 0) return;
        //     foreach (var key in assetKeys)
        //     {
        //         var address = key.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
        //             ? key
        //             : $"Assets/AssetRaw/UI/{key}.prefab";
        //         await _scope.PreloadAsync(address);
        //     }
        // }

        public void ReleaseUIAsset(string assetKey)
        {
            // 手动释放指定UI资源
            if (_disposed) return;
            
            Type assetType;
            lock (_loadedAssetTypes)
            {
                if (!_loadedAssetTypes.TryGetValue(assetKey, out assetType))
                {
                    // 如果没有记录，默认尝试GameObject类型
                    assetType = typeof(GameObject);
                }
                else
                {
                    // 移除记录
                    _loadedAssetTypes.Remove(assetKey);
                }
            }
            
            _registry.Release(packageName, GetPath(assetKey), assetType);
        }

        public void ClearCache()
        {
            // 清空指定包的缓存和本地类型记录
            if (_disposed) return;
            
            lock (_loadedAssetTypes)
            {
                _loadedAssetTypes.Clear();
            }
            
            _registry.ClearPackageCache(packageName);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            _cts.Cancel();   // ✅ 取消所有挂起任务
            _cts.Dispose();
            
            // 清理类型记录
            lock (_loadedAssetTypes)
            {
                _loadedAssetTypes.Clear();
            }
            
            // Registry是单例，不需要在这里Dispose
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ScopedUIResourceLoader));
        }

        private string GetPath(string assetKey)
        {
            return $"Assets/AssetRaw/UI/{assetKey}.prefab";
        }
    }
}

