using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.HotFix.AssetSystem.Core
{
    /// <summary>
    /// 绑定到业务生命周期的 Scope：
    /// - 典型绑定：VContainer LifetimeScope、系统/界面/子系统的生命周期。
    /// - 在 Scope.Dispose 时统一释放 Pin 的资源；Lease 由调用者自行释放。
    /// - 不直接暴露 YooAsset API。
    /// </summary>
    public interface IAssetScope : IDisposable
    {
        UniTask<Lease<T>> PinAsync<T>(string address, CancellationToken ct = default) where T : UnityEngine.Object;
        UniTask<Lease<T>> LeaseAsync<T>(string address, CancellationToken ct = default) where T : UnityEngine.Object;
        UniTask PreloadAsync(string address, CancellationToken ct = default);
    }
}

