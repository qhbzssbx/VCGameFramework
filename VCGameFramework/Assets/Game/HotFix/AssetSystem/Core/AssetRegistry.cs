using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Game.HotFix.AssetSystem.Core;
using YooAsset;

namespace Game.HotFix.AssetSystem.Core
{
    /// <summary>
    /// IAssetRegistry 默认实现（Singleton）：
    /// - 使用 address -> Entry 的字典管理 YooAsset 句柄与 RefCount。
    /// - Pin/Lease 统一走 Entry，Lease.Dispose/Pin 归还会减少 RefCount 并在为 0 时释放句柄。
    /// - 预留 TTL/统计/调试 UI 钩子（此版本不实现，留给项目侧按需扩展）。
    /// </summary>
    public sealed class AssetRegistry : IAssetRegistry, IDisposable
    {
        private readonly Dictionary<string, Entry> _entries = new();
        private readonly object _gate = new();
        private bool _disposed;

        private sealed class Entry
        {
            public AssetHandle Handle;
            public UnityEngine.Object Asset;
            public int RefCount;
            public DateTime LastAccess;
        }

        public async UniTask<Lease<T>> LeaseAsync<T>(string address, CancellationToken ct = default) where T : UnityEngine.Object
        {
            ThrowIfDisposed();
            var entry = await GetOrLoadEntryAsync<T>(address, ct);
            Touch(entry);
            Increase(entry); // Lease 也占用一次引用计数
            return new Lease<T>(address, (T)entry.Asset, () => ReleaseInternal(address));
        }

        public async UniTask<Lease<T>> PinAsync<T>(string address, CancellationToken ct = default) where T : UnityEngine.Object
        {
            // 与 Lease 行为一致：交由上层 Scope 管理释放时机
            return await LeaseAsync<T>(address, ct);
        }

        public async UniTask PreloadAsync(string address, CancellationToken ct = default)
        {
            ThrowIfDisposed();
            // 采用 YooAsset 的加载+立刻 Release 策略，利用其内部缓存
            var handle = YooAssets.LoadAssetAsync(address);
            await handle.ToUniTask(cancellationToken: ct);
            handle.Release();
        }

        public UniTask CleanupAsync(CancellationToken ct = default)
        {
            // 预留：可在此加入 TTL 淘汰、TopN 统计、调用 YooAssets.UnloadUnusedAssetsAsync 等
            // 为减少入侵度，此处先空实现，交由上层在合适时机调用 YooAssets.UnloadUnusedAssetsAsync。
            return UniTask.CompletedTask;
        }

        private async UniTask<Entry> GetOrLoadEntryAsync<T>(string address, CancellationToken ct) where T : UnityEngine.Object
        {
            Entry entry;
            lock (_gate)
            {
                if (_entries.TryGetValue(address, out entry))
                {
                    return entry;
                }
            }

            // 未命中则加载
            var handle = YooAssets.LoadAssetAsync<T>(address);
            await handle.ToUniTask(cancellationToken: ct);
            if (handle.AssetObject == null)
                throw new InvalidOperationException($"Failed to load asset: {address}");

            var newEntry = new Entry
            {
                Handle = handle,
                Asset = handle.AssetObject,
                RefCount = 0,
                LastAccess = DateTime.UtcNow
            };

            lock (_gate)
            {
                if (_entries.TryGetValue(address, out entry))
                {
                    // 并发加载竞争：复用已存在条目，释放新句柄
                    handle.Release();
                    return entry;
                }
                _entries[address] = newEntry;
                return newEntry;
            }
        }

        private static void Touch(Entry e) => e.LastAccess = DateTime.UtcNow;

        private static void Increase(Entry e) => e.RefCount++;

        private void ReleaseInternal(string address)
        {
            lock (_gate)
            {
                if (!_entries.TryGetValue(address, out var entry)) return;
                entry.RefCount = Math.Max(0, entry.RefCount - 1);
                if (entry.RefCount == 0)
                {
                    entry.Handle?.Release();
                    _entries.Remove(address);
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            lock (_gate)
            {
                foreach (var kv in _entries)
                {
                    kv.Value.Handle?.Release();
                }
                _entries.Clear();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AssetRegistry));
        }
    }
}

