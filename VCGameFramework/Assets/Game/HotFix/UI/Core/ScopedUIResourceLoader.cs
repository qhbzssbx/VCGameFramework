using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Game.HotFix.AssetSystem.Core;

namespace Game.UI.Core
{
    /// <summary>
    /// UI 资源加载适配器：基于新 AssetScope/Registry 实现，提供与原 IUIResourceLoader 相同接口。
    /// - 单例使用一个内部 Scope，在 UISystem 销毁时统一释放。
    /// - 业务仍可通过 UIManagerModule 注入 IUIResourceLoader，无需感知 YooAsset。
    /// </summary>
    public sealed class ScopedUIResourceLoader : IUIResourceLoader
    {
        private readonly IAssetScope _scope;
        private bool _disposed;

        public ScopedUIResourceLoader(Game.HotFix.AssetSystem.Core.IAssetRegistry registry)
        {
            _scope = new AssetScope(registry);
        }

        public async UniTask<GameObject> LoadUIPrefabAsync(string assetKey)
        {
            ThrowIfDisposed();
            var address = $"Assets/AssetRaw/UI/{assetKey}.prefab";
            using var lease = await _scope.PinAsync<GameObject>(address, CancellationToken.None);
            return lease.Value;
        }

        public async UniTask<Texture2D> LoadUITextureAsync(string assetKey)
        {
            ThrowIfDisposed();
            var address = assetKey; // 若需要，可按项目地址规范改为统一前缀
            using var lease = await _scope.PinAsync<Texture2D>(address, CancellationToken.None);
            return lease.Value;
        }

        public async UniTask<Sprite> LoadUISpriteAsync(string assetKey)
        {
            ThrowIfDisposed();
            var address = assetKey;
            using var lease = await _scope.PinAsync<Sprite>(address, CancellationToken.None);
            return lease.Value;
        }

        public async UniTask PreloadUIAssetsAsync(params string[] assetKeys)
        {
            ThrowIfDisposed();
            if (assetKeys == null || assetKeys.Length == 0) return;
            foreach (var key in assetKeys)
            {
                var address = key.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
                    ? key
                    : $"Assets/AssetRaw/UI/{key}.prefab";
                await _scope.PreloadAsync(address);
            }
        }

        public void ReleaseUIAsset(string assetKey)
        {
            // 新系统下：资源由 Scope 统一释放，无需局部释放。
            // 若需要“提前释放”功能，可扩展局部缓存并在此 Dispose 对应租约。
        }

        public void ClearCache()
        {
            // 新系统无本地缓存；Pin 资源在 Dispose 时统一释放。
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _scope.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ScopedUIResourceLoader));
        }
    }
}

