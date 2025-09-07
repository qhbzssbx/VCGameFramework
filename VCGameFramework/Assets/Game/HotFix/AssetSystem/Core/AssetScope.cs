using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.HotFix.AssetSystem.Core
{
    /// <summary>
    /// IAssetScope 默认实现：
    /// - 持有对 IAssetRegistry 的引用。
    /// - 记录本 Scope 内部 Pin 的租约，Dispose 时统一释放。
    /// - Lease 仍需业务自行释放。
    /// </summary>
    public sealed class AssetScope : IAssetScope
    {
        private readonly IAssetRegistry _registry;
        private readonly List<IDisposable> _pinned = new();
        private bool _disposed;

        public AssetScope(IAssetRegistry registry)
        {
            _registry = registry;
        }

        public async UniTask<Lease<T>> PinAsync<T>(string address, CancellationToken ct = default) where T : UnityEngine.Object
        {
            ThrowIfDisposed();
            var lease = await _registry.PinAsync<T>(address, ct);
            _pinned.Add(lease);
            return lease;
        }

        public async UniTask<Lease<T>> LeaseAsync<T>(string address, CancellationToken ct = default) where T : UnityEngine.Object
        {
            ThrowIfDisposed();
            return await _registry.LeaseAsync<T>(address, ct);
        }

        public async UniTask PreloadAsync(string address, CancellationToken ct = default)
        {
            ThrowIfDisposed();
            await _registry.PreloadAsync(address, ct);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            for (int i = _pinned.Count - 1; i >= 0; i--)
            {
                try { _pinned[i]?.Dispose(); } catch { /* ignore */ }
            }
            _pinned.Clear();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AssetScope));
        }
    }
}

