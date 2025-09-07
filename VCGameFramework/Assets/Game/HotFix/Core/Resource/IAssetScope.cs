using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Core.Resource
{
    public interface IAssetScope
    {
        UniTask<T> PinAsync<T>(string package, string address, CancellationToken ct) where T : UnityEngine.Object;
        // Lease：返回 IAsyncDisposable；离开 using 范围释放“本 Scope 的那一份”引用
        // UniTask<IAsyncDisposable> LeaseAsync<T>(string package, string address, CancellationToken ct, Action<T>? onLoaded = null) where T : UnityEngine.Object;
        UniTask<IAsyncDisposable> LeaseAsync<T>(string package, string address, CancellationToken ct, Action<T> onLoaded = null) where T : UnityEngine.Object;
    }
}