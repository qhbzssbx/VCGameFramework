using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Game.HotFix.AssetSystem.Core
{
    /// <summary>
    /// 资源统一卸载管理器实现
    /// </summary>
    public sealed class AssetUnloadManager : IAssetUnloadManager
    {
        private readonly IAssetRegistry _assetRegistry;
        private readonly List<IUnloadTrigger> _triggers = new();
        private readonly List<double> _unloadTimes = new(100);
        private readonly object _lock = new();
        
        private IUnloadStrategy _strategy;
        private CancellationTokenSource _autoUnloadCts;
        private DateTime _lastUnloadTime = DateTime.MinValue;
        private bool _disposed;
        private bool _isRunning;

        // 统计信息
        private long _totalUnloadCount;
        private readonly Dictionary<string, long> _triggerCounts = new();
        private readonly Dictionary<string, long> _packageUnloadCounts = new();
        private readonly List<UnloadRecord> _recentUnloads = new();

        public AssetUnloadManager(IAssetRegistry assetRegistry)
        {
            _assetRegistry = assetRegistry ?? throw new ArgumentNullException(nameof(assetRegistry));
            
            // 设置默认策略
            _strategy = new DefaultUnloadStrategy();
            
            Debug.Log("[AssetUnloadManager] Initialized with default unload strategy");
        }

        public void RegisterUnloadTrigger(IUnloadTrigger trigger)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));
            ThrowIfDisposed();

            lock (_lock)
            {
                if (!_triggers.Contains(trigger))
                {
                    _triggers.Add(trigger);
                    Debug.Log($"[AssetUnloadManager] Registered trigger: {trigger.Name}");
                }
            }
        }

        public void UnregisterUnloadTrigger(IUnloadTrigger trigger)
        {
            if (trigger == null) return;
            ThrowIfDisposed();

            lock (_lock)
            {
                if (_triggers.Remove(trigger))
                {
                    Debug.Log($"[AssetUnloadManager] Unregistered trigger: {trigger.Name}");
                }
            }
        }

        public async UniTask TriggerUnloadAsync(string package = null, string reason = "Manual", CancellationToken ct = default)
        {
            ThrowIfDisposed();
            
            var stopwatch = Stopwatch.StartNew();
            var startTime = DateTime.Now;
            var assetStatsBefore = _assetRegistry.GetStats();
            
            Debug.Log($"[AssetUnloadManager] Starting unload - Package: {package ?? "All"}, Reason: {reason}");

            try
            {
                if (string.IsNullOrEmpty(package))
                {
                    await _assetRegistry.UnloadAllUnusedAsync(ct);
                }
                else
                {
                    await _assetRegistry.UnloadUnusedAsync(package, ct);
                }

                stopwatch.Stop();
                var duration = stopwatch.Elapsed.TotalMilliseconds;
                var assetStatsAfter = _assetRegistry.GetStats();

                // 更新统计
                RecordUnload(reason, package ?? "All", duration, assetStatsBefore.CachedAssetCount, 
                    assetStatsAfter.CachedAssetCount);

                _lastUnloadTime = startTime;
                Debug.Log($"[AssetUnloadManager] Completed unload in {duration:F1}ms - " +
                         $"Assets: {assetStatsBefore.CachedAssetCount} -> {assetStatsAfter.CachedAssetCount}");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Debug.LogError($"[AssetUnloadManager] Unload failed: {ex.Message}");
                throw;
            }
        }

        public void SetUnloadStrategy(IUnloadStrategy strategy)
        {
            ThrowIfDisposed();
            
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            Debug.Log($"[AssetUnloadManager] Set unload strategy: {strategy.Name}");
        }

        public UnloadStats GetUnloadStats()
        {
            ThrowIfDisposed();

            lock (_lock)
            {
                return new UnloadStats
                {
                    TotalUnloadCount = _totalUnloadCount,
                    LastUnloadTime = _lastUnloadTime,
                    AverageUnloadTimeMs = _unloadTimes.Count > 0 ? _unloadTimes.Average() : 0.0,
                    TriggerCounts = _triggerCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    PackageUnloadCounts = _packageUnloadCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    RecentUnloads = _recentUnloads.ToList()
                };
            }
        }

        public async UniTask StartAsync(CancellationToken ct = default)
        {
            ThrowIfDisposed();
            
            if (_isRunning)
            {
                Debug.LogWarning("[AssetUnloadManager] Already running");
                return;
            }

            _isRunning = true;
            _autoUnloadCts?.Cancel();
            _autoUnloadCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            Debug.Log("[AssetUnloadManager] Starting auto-unload loop");
            
            // 启动自动卸载循环
            _ = AutoUnloadLoop(_autoUnloadCts.Token);
            
            await UniTask.CompletedTask;
        }

        public async UniTask StopAsync()
        {
            if (!_isRunning) return;

            Debug.Log("[AssetUnloadManager] Stopping auto-unload loop");
            
            _isRunning = false;
            _autoUnloadCts?.Cancel();
            _autoUnloadCts?.Dispose();
            _autoUnloadCts = null;

            await UniTask.CompletedTask;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            StopAsync().Forget();
            
            lock (_lock)
            {
                _triggers.Clear();
            }

            Debug.Log("[AssetUnloadManager] Disposed");
        }

        private async UniTaskVoid AutoUnloadLoop(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested && _isRunning)
                {
                    var interval = _strategy?.GetCheckInterval() ?? TimeSpan.FromMinutes(5);
                    await UniTask.Delay(interval, cancellationToken: ct);

                    if (ct.IsCancellationRequested) break;

                    await CheckAndExecuteUnload(ct);
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消，忽略
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AssetUnloadManager] Auto-unload loop error: {ex.Message}");
            }
        }

        private async UniTask CheckAndExecuteUnload(CancellationToken ct)
        {
            try
            {
                var context = CreateUnloadContext();
                var triggersToExecute = new List<(IUnloadTrigger trigger, IReadOnlyList<string> packages)>();

                // 检查所有触发器
                lock (_lock)
                {
                    foreach (var trigger in _triggers)
                    {
                        try
                        {
                            if (trigger.ShouldUnload(context))
                            {
                                var packages = trigger.GetPackagesToUnload(context);
                                triggersToExecute.Add((trigger, packages));
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[AssetUnloadManager] Error checking trigger {trigger.Name}: {ex.Message}");
                        }
                    }
                }

                // 检查策略
                var shouldAutoUnload = _strategy?.ShouldAutoUnload(context) ?? false;
                if (shouldAutoUnload)
                {
                    triggersToExecute.Add((new StrategyTrigger(_strategy.Name), null));
                }

                // 执行卸载
                foreach (var (trigger, packages) in triggersToExecute)
                {
                    try
                    {
                        if (packages == null || packages.Count == 0)
                        {
                            await TriggerUnloadAsync(null, trigger.Name, ct);
                        }
                        else
                        {
                            foreach (var package in packages)
                            {
                                await TriggerUnloadAsync(package, trigger.Name, ct);
                            }
                        }
                        
                        // 记录触发器使用次数
                        lock (_lock)
                        {
                            if (!_triggerCounts.ContainsKey(trigger.Name))
                                _triggerCounts[trigger.Name] = 0;
                            _triggerCounts[trigger.Name]++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[AssetUnloadManager] Error executing unload for trigger {trigger.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AssetUnloadManager] Error in check and execute unload: {ex.Message}");
            }
        }

        private UnloadContext CreateUnloadContext()
        {
            var now = DateTime.Now;
            return new UnloadContext
            {
                CurrentTime = now,
                TimeSinceLastUnload = now - _lastUnloadTime,
                AssetStats = _assetRegistry.GetStats(),
                MemoryInfo = MemoryInfo.GetCurrent()
            };
        }

        private void RecordUnload(string reason, string package, double durationMs, int assetsBefore, int assetsAfter)
        {
            lock (_lock)
            {
                _totalUnloadCount++;
                
                _unloadTimes.Add(durationMs);
                if (_unloadTimes.Count > 100) // 保持最近100次记录
                {
                    _unloadTimes.RemoveAt(0);
                }

                if (!_packageUnloadCounts.ContainsKey(package))
                    _packageUnloadCounts[package] = 0;
                _packageUnloadCounts[package]++;

                var record = new UnloadRecord
                {
                    Time = DateTime.Now,
                    Reason = reason,
                    Package = package,
                    DurationMs = durationMs,
                    AssetCountBefore = assetsBefore,
                    AssetCountAfter = assetsAfter
                };

                _recentUnloads.Add(record);
                if (_recentUnloads.Count > 100) // 保持最近100次记录
                {
                    _recentUnloads.RemoveAt(0);
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AssetUnloadManager));
        }

        // 内部策略触发器
        private sealed class StrategyTrigger : IUnloadTrigger
        {
            public string Name { get; }

            public StrategyTrigger(string strategyName)
            {
                Name = $"Strategy({strategyName})";
            }

            public bool ShouldUnload(UnloadContext context) => true;
            public IReadOnlyList<string> GetPackagesToUnload(UnloadContext context) => null;
        }
    }

    /// <summary>
    /// 默认卸载策略
    /// </summary>
    public sealed class DefaultUnloadStrategy : IUnloadStrategy
    {
        public string Name => "Default";

        public TimeSpan GetCheckInterval() => TimeSpan.FromMinutes(5);

        public bool ShouldAutoUnload(UnloadContext context)
        {
            // 超过10分钟没卸载且TTL队列有对象时触发卸载
            return context.TimeSinceLastUnload.TotalMinutes > 10 && 
                   context.AssetStats.TtlQueueCount > 0;
        }
    }

    /// <summary>
    /// 内存压力卸载策略
    /// </summary>
    public sealed class MemoryPressureUnloadStrategy : IUnloadStrategy
    {
        private readonly float _memoryThreshold;
        private readonly TimeSpan _checkInterval;

        public string Name => "MemoryPressure";

        public MemoryPressureUnloadStrategy(float memoryThreshold = 0.8f, TimeSpan? checkInterval = null)
        {
            _memoryThreshold = memoryThreshold;
            _checkInterval = checkInterval ?? TimeSpan.FromMinutes(2);
        }

        public TimeSpan GetCheckInterval() => _checkInterval;

        public bool ShouldAutoUnload(UnloadContext context)
        {
            // 内存使用率超过阈值时触发卸载
            return context.MemoryInfo.UsageRatio > _memoryThreshold &&
                   context.AssetStats.TtlQueueCount > 0;
        }
    }
}