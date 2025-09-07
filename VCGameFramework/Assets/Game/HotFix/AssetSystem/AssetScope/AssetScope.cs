using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace Game.HotFix.AssetSystem.Core
{
    /// <summary>
    /// 每个 VContainer LifetimeScope 里注册一个 IAssetScope。
    /// - Pin：长期持有（例如窗口/对象池/场景常驻），Scope Dispose 时统一释放
    /// - Lease：短租，用完即还
    /// </summary>
    public sealed class AssetScope : IAssetScope, IAsyncStartable
    {
        private readonly IAssetRegistry _registry;
        private readonly CancellationToken _scopeCt;

        // 仅记录“本 Scope 亲手 Pin 的份额”，统一释放
        private readonly HashSet<(string pkg, string addr, Type type)> _pinned = new();
        private bool _disposed;

        public AssetScope(IAssetRegistry registry, CancellationToken scopeCt)
        {
            _registry = registry;
            _scopeCt = scopeCt;
        }

        public UniTask StartAsync(CancellationToken cancellation = default)
        {
            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public async UniTask<T> PinAsync<T>(string package, string address, CancellationToken ct = default)
            where T : UnityEngine.Object
        {
            EnsureNotDisposed();
            var token = Merge(_scopeCt, ct);
            var obj = await _registry.AcquireAsync<T>(package, address, token);
            _pinned.Add((package, address, typeof(T)));
            return obj;
        }

        public async UniTask<IUniTaskAsyncDisposable> LeaseAsync<T>(string package, string address, CancellationToken ct = default,
            Action<T>? onLoaded = null) where T : UnityEngine.Object
        {
            EnsureNotDisposed();
            var token = Merge(_scopeCt, ct);
            var obj = await _registry.AcquireAsync<T>(package, address, token);
            onLoaded?.Invoke(obj);
            return new AsyncDispose(() =>
            {
                _registry.Release(package, address, typeof(T));
                return UniTask.CompletedTask;
            });
        }

        public async UniTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (var (pkg, addr, type) in _pinned)
                _registry.Release(pkg, addr, type);
            _pinned.Clear();
            await UniTask.CompletedTask;
        }

        private static CancellationToken Merge(CancellationToken a, CancellationToken b)
            => CancellationTokenSource.CreateLinkedTokenSource(a, b).Token;

        private void EnsureNotDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AssetScope));
        }



        private sealed class AsyncDispose : IUniTaskAsyncDisposable
        {
            private readonly Func<UniTask> _act;
            private bool _done;
            public AsyncDispose(Func<UniTask> act) => _act = act;
            public async UniTask DisposeAsync()
            {
                if (_done) return; _done = true;
                await _act();
            }
        }
    }
}
