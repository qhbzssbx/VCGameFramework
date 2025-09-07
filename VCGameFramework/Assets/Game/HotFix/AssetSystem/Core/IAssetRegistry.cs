using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.HotFix.AssetSystem.Core
{
    public interface IAssetRegistry : IDisposable
    {
        /// <summary>获取或加载资源，并增加引用计数（并发去重）。返回句柄中的 Unity 对象。</summary>
        UniTask<T> AcquireAsync<T>(string package, string address, CancellationToken ct = default)
            where T : UnityEngine.Object;

        /// <summary>减少引用计数。RefCount 归零 → 进入 TTL 延迟释放队列（不会立刻释放）。</summary>
        void Release(string package, string address, Type type);

        /// <summary>立即触发指定包的 Unload（常在切场景/副本结束时调用）。</summary>
        UniTask UnloadUnusedAsync(string package = "DefaultPackage", CancellationToken ct = default);

        /// <summary>触发所有包的 Unload。</summary>
        UniTask UnloadAllUnusedAsync(CancellationToken ct = default);

        /// <summary>获取当前资源统计快照。</summary>
        AssetStats GetStats();

        /// <summary>获取所有缓存资源的详细信息。</summary>
        IReadOnlyList<AssetInfo> GetAssetInfos();

        /// <summary>预热指定资源（仅加载不增加引用计数）。</summary>
        UniTask PrewarmAsync<T>(string package, string address, CancellationToken ct = default)
            where T : UnityEngine.Object;

        /// <summary>清空指定包的所有缓存（强制释放）。</summary>
        void ClearPackageCache(string package);
    }
}
