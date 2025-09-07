using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.HotFix.AssetSystem.Core
{
    public interface IAssetScope : IUniTaskAsyncDisposable, IDisposable
    {
        UniTask<T> PinAsync<T>(string package, string address, CancellationToken ct = default)
            where T : UnityEngine.Object;

        UniTask<IUniTaskAsyncDisposable> LeaseAsync<T>(string package, string address, CancellationToken ct = default,
            Action<T>? onLoaded = null) where T : UnityEngine.Object;
    }
}
