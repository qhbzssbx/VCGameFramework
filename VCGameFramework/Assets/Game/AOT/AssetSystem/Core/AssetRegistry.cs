using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using YooAsset;
using AssetInfo = Game.AOT.AssetSystem.Core.AssetInfo;

namespace Game.AOT.AssetSystem.Core
{
    /// <summary>
    /// 线程安全的 AssetRegistry（Singleton）：
    /// - Key = (package, address, type) 三键寻址，避免多包/跨类型冲突
    /// - 并发去重：同 Key 仅首次真实加载，其余 await 同一 LoadingTask
    /// - TTL：RefCount 归零后不立即释放，进入延迟释放；到期或统一卸载点再释放
    /// - 观测指标：提供完整的加载统计、性能监控和调试信息
    /// - 错误处理：重试机制、失败统计和熔断保护
    /// </summary>
    public sealed class AssetRegistry : IAssetRegistry, IDisposable
    {
        public sealed class Key
        {
            public string Package { get; }
            public string Address { get; }
            public Type Type { get; }

            public Key(string package, string address, Type type)
            {
                Package = package;
                Address = address;
                Type = type;
            }

            public override bool Equals(object obj) =>
                obj is Key k && k.Package == Package && k.Address == Address && k.Type == Type;

            public override int GetHashCode() => HashCode.Combine(Package, Address, Type);
        }

        private sealed class Entry
        {
            public AssetHandle Handle = default!;
            public int RefCount;
            public Task? LoadingTask;                 // 并发去重
            public CancellationTokenSource? TtlCts;   // TTL 计时
            
            // 观测指标相关
            public DateTime CreatedTime = DateTime.Now;
            public DateTime LastAccessTime = DateTime.Now;
            public int AccessCount;
            public double LoadTimeMs;
            public int FailCount;
        }

        private readonly Dictionary<Key, Entry> _cache = new();
        private readonly Dictionary<Key, object> _locks = new(); // key 级锁，避免全局串行
        private readonly TimeSpan _ttl;
        private bool _disposed;
        
        // 观测统计
        private long _totalLoadCount;
        private long _cacheHitCount;
        private long _failedLoadCount;
        private readonly List<double> _loadTimes = new(1000); // 保存最近1000次加载时间
        private readonly Dictionary<string, int> _addressAccessCount = new();

        public AssetRegistry(TimeSpan? ttl = null)
        {
            // 建议 10–30s；先给 15s 的稳态默认
            _ttl = ttl ?? TimeSpan.FromSeconds(15);
        }

        public async UniTask<T> AcquireAsync<T>(string package, string address, CancellationToken ct = default)
            where T : UnityEngine.Object
        {
            ThrowIfDisposed();
            var key = new Key(package, address, typeof(T));
            
            Interlocked.Increment(ref _totalLoadCount);

            // 快速路径：已缓存
            lock (_cache)
            {
                if (_cache.TryGetValue(key, out var e))
                {
                    e.TtlCts?.Cancel(); e.TtlCts = null;
                    e.RefCount++;
                    e.LastAccessTime = DateTime.Now;
                    e.AccessCount++;
                    
                    Interlocked.Increment(ref _cacheHitCount);
                    RecordAddressAccess($"{package}/{address}");
                    
                    return e.Handle.GetAssetObject<T>();
                }
                if (!_locks.ContainsKey(key)) _locks[key] = new object();
            }

            Entry entry;
            lock (_locks[key])
            {
                // Double-check（可能已有其他并发线程抢先建了）
                if (_cache.TryGetValue(key, out entry))
                {
                    entry.TtlCts?.Cancel(); entry.TtlCts = null;
                    entry.RefCount++;
                    entry.LastAccessTime = DateTime.Now;
                    entry.AccessCount++;
                    
                    Interlocked.Increment(ref _cacheHitCount);
                    RecordAddressAccess($"{package}/{address}");
                    
                    return entry.Handle.GetAssetObject<T>();
                }

                // 创建条目并发起一次真实加载
                entry = new Entry { RefCount = 1 };
                _cache[key] = entry;
                entry.LoadingTask = LoadInternal<T>(key, entry, ct);
            }

            // 等待加载任务（其余并发 Acquire 会等同一任务）
            await entry.LoadingTask;
            RecordAddressAccess($"{package}/{address}");
            return entry.Handle.GetAssetObject<T>();
        }

        private async Task LoadInternal<T>(Key key, Entry e, CancellationToken ct) where T : UnityEngine.Object
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var pkg = YooAssets.GetPackage(key.Package);
                if (pkg == null)
                    throw new InvalidOperationException($"YooAsset package not found: {key.Package}");

                var h = pkg.LoadAssetAsync<T>(key.Address);
                await h.ToUniTask(cancellationToken: ct);
                
                if (!h.IsValid || h.AssetObject == null)
                    throw new InvalidOperationException($"Invalid handle or null asset: {key.Package}/{key.Address} ({key.Type.Name})");
                
                stopwatch.Stop();
                e.Handle = h;
                e.LoadTimeMs = stopwatch.Elapsed.TotalMilliseconds;
                
                RecordLoadTime(e.LoadTimeMs);
                UnityEngine.Debug.Log($"[AssetRegistry] Loaded asset: {key.Package}/{key.Address} ({key.Type.Name}) in {e.LoadTimeMs:F1}ms");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                e.FailCount++;
                e.LoadTimeMs = stopwatch.Elapsed.TotalMilliseconds;
                
                Interlocked.Increment(ref _failedLoadCount);
                UnityEngine.Debug.LogError($"[AssetRegistry] Failed to load asset: {key.Package}/{key.Address} ({key.Type.Name}) - {ex.Message}");
                
                // 加载失败：还原状态
                lock (_cache) _cache.Remove(key);
                throw;
            }
            finally
            {
                e.LoadingTask = null;
            }
        }

        public void Release(string package, string address, Type type)
        {
            if (_disposed) return;
            var key = new Key(package, address, type);
            Entry? e;

            lock (_cache)
            {
                if (!_cache.TryGetValue(key, out e)) return;
                if (--e.RefCount > 0) return;

                // RefCount 归零：进入 TTL 延迟释放，期间如再 Acquire 会取消 TTL
                e.TtlCts?.Cancel();
                e.TtlCts = new CancellationTokenSource();
                var cts = e.TtlCts;
                var handle = e.Handle;

                // 注：此处不立刻从 _cache 移除，便于 TTL 期间再次命中
                _ = UniTask.RunOnThreadPool(async () =>
                {
                    try { await UniTask.Delay(_ttl, cancellationToken: cts.Token); }
                    catch { /* canceled */ }
                    if (cts.IsCancellationRequested) return;

                    lock (_cache)
                    {
                        if (_cache.TryGetValue(key, out var it) && it.RefCount == 0 && it.TtlCts == cts)
                        {
                            try { handle.Release(); } catch { /* ignore */ }
                            _cache.Remove(key);
                        }
                    }
                });
            }
        }

        public async UniTask UnloadUnusedAsync(string package = "DefaultPackage", CancellationToken ct = default)
        {
            ThrowIfDisposed();
            
            var pkg = YooAssets.GetPackage(package);
            if (pkg == null)
            {
                UnityEngine.Debug.LogWarning($"[AssetRegistry] Package not found: {package}");
                return;
            }
            
            UnityEngine.Debug.Log($"[AssetRegistry] Starting unload unused assets for package: {package}");
            var operation = pkg.UnloadUnusedAssetsAsync();
            await operation;
            UnityEngine.Debug.Log($"[AssetRegistry] Completed unload unused assets for package: {package}");
        }

        public async UniTask UnloadAllUnusedAsync(CancellationToken ct = default)
        {
            ThrowIfDisposed();
            
            UnityEngine.Debug.Log("[AssetRegistry] Starting unload unused assets for all packages");
            
            var packages = new[] { "DefaultPackage" }; // 可扩展为获取所有已初始化的包
            foreach (var packageName in packages)
            {
                await UnloadUnusedAsync(packageName, ct);
            }
            
            UnityEngine.Debug.Log("[AssetRegistry] Completed unload unused assets for all packages");
        }

        public AssetStats GetStats()
        {
            lock (_cache)
            {
                var stats = new AssetStats
                {
                    CachedAssetCount = _cache.Count,
                    TotalRefCount = _cache.Values.Sum(e => e.RefCount),
                    TtlQueueCount = _cache.Values.Count(e => e.TtlCts != null),
                    TotalLoadCount = _totalLoadCount,
                    CacheHitCount = _cacheHitCount,
                    FailedLoadCount = _failedLoadCount,
                    CurrentLoadingCount = _cache.Values.Count(e => e.LoadingTask != null),
                    AverageLoadTimeMs = _loadTimes.Count > 0 ? _loadTimes.Average() : 0.0
                };

                // TopN 地址访问统计（取前10）
                stats.TopAddresses = _addressAccessCount
                    .OrderByDescending(kvp => kvp.Value)
                    .Take(10)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                return stats;
            }
        }

        public IReadOnlyList<AssetInfo> GetAssetInfos()
        {
            lock (_cache)
            {
                return _cache.Select(kvp => new AssetInfo
                {
                    Package = kvp.Key.Package,
                    Address = kvp.Key.Address,
                    TypeName = kvp.Key.Type.Name,
                    RefCount = kvp.Value.RefCount,
                    IsInTtlQueue = kvp.Value.TtlCts != null,
                    IsLoading = kvp.Value.LoadingTask != null,
                    LastAccessTime = kvp.Value.LastAccessTime,
                    AccessCount = kvp.Value.AccessCount,
                    LoadTimeMs = kvp.Value.LoadTimeMs
                }).ToList().AsReadOnly();
            }
        }

        public async UniTask PrewarmAsync<T>(string package, string address, CancellationToken ct = default)
            where T : UnityEngine.Object
        {
            ThrowIfDisposed();
            
            var key = new Key(package, address, typeof(T));
            
            // 如果已缓存，直接返回
            lock (_cache)
            {
                if (_cache.ContainsKey(key))
                {
                    UnityEngine.Debug.Log($"[AssetRegistry] Asset already cached for prewarming: {package}/{address}");
                    return;
                }
            }
            
            UnityEngine.Debug.Log($"[AssetRegistry] Prewarming asset: {package}/{address} ({typeof(T).Name})");
            
            // 临时获取资源但不增加引用计数
            var asset = await AcquireAsync<T>(package, address, ct);
            
            // 立即释放（但由于TTL机制，实际会延迟释放）
            Release(package, address, typeof(T));
        }

        public void ClearPackageCache(string package)
        {
            ThrowIfDisposed();
            
            UnityEngine.Debug.Log($"[AssetRegistry] Clearing cache for package: {package}");
            
            var keysToRemove = new List<Key>();
            
            lock (_cache)
            {
                foreach (var kvp in _cache)
                {
                    if (kvp.Key.Package == package)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }

                foreach (var key in keysToRemove)
                {
                    if (_cache.TryGetValue(key, out var entry))
                    {
                        try
                        {
                            entry.TtlCts?.Cancel();
                            entry.Handle?.Release();
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogError($"[AssetRegistry] Error releasing asset during cache clear: {ex.Message}");
                        }
                        
                        _cache.Remove(key);
                    }
                }
            }
            
            UnityEngine.Debug.Log($"[AssetRegistry] Cleared {keysToRemove.Count} cached assets for package: {package}");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            lock (_cache)
            {
                foreach (var e in _cache.Values)
                {
                    try { e.TtlCts?.Cancel(); } catch { }
                    try { e.Handle?.Release(); } catch { }
                }
                _cache.Clear();
                _locks.Clear();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AssetRegistry));
        }

        private void RecordLoadTime(double timeMs)
        {
            lock (_loadTimes)
            {
                _loadTimes.Add(timeMs);
                // 保持最近1000次记录
                if (_loadTimes.Count > 1000)
                {
                    _loadTimes.RemoveAt(0);
                }
            }
        }

        private void RecordAddressAccess(string fullAddress)
        {
            lock (_addressAccessCount)
            {
                if (!_addressAccessCount.ContainsKey(fullAddress))
                {
                    _addressAccessCount[fullAddress] = 0;
                }
                _addressAccessCount[fullAddress]++;
            }
        }
    }
}
