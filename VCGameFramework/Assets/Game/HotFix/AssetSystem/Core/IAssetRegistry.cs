using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.HotFix.AssetSystem.Core
{
    /// <summary>
    /// 统一的资源注册中心（Singleton）：
    /// - 负责 (package,address) -> YooAsset 句柄的集中管理、RefCount、最后访问时间、TTL 策略等。
    /// - 对业务隐藏 YooAsset API，提供 Pin/Lease 语义，确保生命周期可控。
    /// - 线程安全：内部使用轻量锁保证并发获取时的正确性（Unity 主线程为主，异步加载可 await）。
    /// </summary>
    public interface IAssetRegistry
    {
        /// <summary>
        /// 获取一个可释放的租约（Lease）：调用方负责在合适时机释放。
        /// 典型用法：短期使用资源（如加载一次纹理）。
        /// </summary>
        UniTask<Lease<T>> LeaseAsync<T>(string address, CancellationToken ct = default) where T : UnityEngine.Object;

        /// <summary>
        /// 固定（Pin）到 Registry：引用计数+1，并返回可释放的租约。
        /// 典型用法：高频访问或需要在一段时间内常驻的资源，由上层 Scope 统一释放。
        /// </summary>
        UniTask<Lease<T>> PinAsync<T>(string address, CancellationToken ct = default) where T : UnityEngine.Object;

        /// <summary>
        /// 预加载：加载并立刻释放句柄，交由底层缓存维持。
        /// </summary>
        UniTask PreloadAsync(string address, CancellationToken ct = default);

        /// <summary>
        /// 主动触发一次清理策略（如调用 YooAssets.UnloadUnusedAssetsAsync 或 TTL 淘汰）。
        /// 注：默认实现可能是空操作，留给项目方按需实现策略。
        /// </summary>
        UniTask CleanupAsync(CancellationToken ct = default);
    }

    /// <summary>
    /// 资源租约：封装具体对象与释放逻辑。Dispose 表示归还引用。
    /// </summary>
    public sealed class Lease<T> : IDisposable where T : UnityEngine.Object
    {
        internal readonly string Address;
        internal readonly T Asset;
        private readonly Action _onDispose;
        private bool _disposed;

        internal Lease(string address, T asset, Action onDispose)
        {
            Address = address;
            Asset = asset;
            _onDispose = onDispose;
        }

        public T Value => Asset;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _onDispose?.Invoke();
        }
    }
}

