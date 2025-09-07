using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.HotFix.AssetSystem.Core
{
    /// <summary>
    /// 资源统一卸载管理器接口：负责在适当时机批量卸载未使用的资源
    /// </summary>
    public interface IAssetUnloadManager : IDisposable
    {
        /// <summary>
        /// 注册卸载触发器
        /// </summary>
        /// <param name="trigger">卸载触发器</param>
        void RegisterUnloadTrigger(IUnloadTrigger trigger);

        /// <summary>
        /// 注销卸载触发器
        /// </summary>
        /// <param name="trigger">卸载触发器</param>
        void UnregisterUnloadTrigger(IUnloadTrigger trigger);

        /// <summary>
        /// 手动触发一次资源卸载
        /// </summary>
        /// <param name="package">包名，null表示所有包</param>
        /// <param name="reason">卸载原因</param>
        /// <param name="ct">取消令牌</param>
        /// <returns>卸载任务</returns>
        UniTask TriggerUnloadAsync(string package = null, string reason = "Manual", CancellationToken ct = default);

        /// <summary>
        /// 设置自动卸载策略
        /// </summary>
        /// <param name="strategy">卸载策略</param>
        void SetUnloadStrategy(IUnloadStrategy strategy);

        /// <summary>
        /// 获取卸载统计信息
        /// </summary>
        /// <returns>卸载统计</returns>
        UnloadStats GetUnloadStats();

        /// <summary>
        /// 启动自动卸载（如果有配置）
        /// </summary>
        UniTask StartAsync(CancellationToken ct = default);

        /// <summary>
        /// 停止自动卸载
        /// </summary>
        UniTask StopAsync();
    }

    /// <summary>
    /// 卸载触发器接口
    /// </summary>
    public interface IUnloadTrigger
    {
        /// <summary>
        /// 触发器名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 检查是否应该触发卸载
        /// </summary>
        /// <param name="context">卸载上下文</param>
        /// <returns>是否应该卸载</returns>
        bool ShouldUnload(UnloadContext context);

        /// <summary>
        /// 获取要卸载的包列表，null表示所有包
        /// </summary>
        /// <param name="context">卸载上下文</param>
        /// <returns>包名列表</returns>
        IReadOnlyList<string> GetPackagesToUnload(UnloadContext context);
    }

    /// <summary>
    /// 卸载策略接口
    /// </summary>
    public interface IUnloadStrategy
    {
        /// <summary>
        /// 策略名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 获取自动卸载检查间隔
        /// </summary>
        /// <returns>检查间隔</returns>
        TimeSpan GetCheckInterval();

        /// <summary>
        /// 是否应该执行自动卸载
        /// </summary>
        /// <param name="context">卸载上下文</param>
        /// <returns>是否应该卸载</returns>
        bool ShouldAutoUnload(UnloadContext context);
    }

    /// <summary>
    /// 卸载上下文
    /// </summary>
    public sealed class UnloadContext
    {
        /// <summary>
        /// 当前时间
        /// </summary>
        public DateTime CurrentTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 距离上次卸载的时间
        /// </summary>
        public TimeSpan TimeSinceLastUnload { get; set; }

        /// <summary>
        /// 当前资源统计
        /// </summary>
        public AssetStats AssetStats { get; set; } = new();

        /// <summary>
        /// 系统内存信息
        /// </summary>
        public MemoryInfo MemoryInfo { get; set; } = new();

        /// <summary>
        /// 自定义属性
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new();

        /// <summary>
        /// 是否为低内存状态
        /// </summary>
        public bool IsLowMemory => MemoryInfo.UsedMemoryMB > MemoryInfo.TotalMemoryMB * 0.8f;
    }

    /// <summary>
    /// 内存信息
    /// </summary>
    public sealed class MemoryInfo
    {
        /// <summary>
        /// 总内存（MB）
        /// </summary>
        public float TotalMemoryMB { get; set; }

        /// <summary>
        /// 已用内存（MB）
        /// </summary>
        public float UsedMemoryMB { get; set; }

        /// <summary>
        /// GC分配内存（MB）
        /// </summary>
        public float GcMemoryMB { get; set; }

        /// <summary>
        /// 内存使用率
        /// </summary>
        public float UsageRatio => TotalMemoryMB > 0 ? UsedMemoryMB / TotalMemoryMB : 0f;

        /// <summary>
        /// 获取当前内存信息
        /// </summary>
        /// <returns>内存信息</returns>
        public static MemoryInfo GetCurrent()
        {
            return new MemoryInfo
            {
                TotalMemoryMB = UnityEngine.SystemInfo.systemMemorySize,
                UsedMemoryMB = GetTotalAllocatedMemoryMB(),
                GcMemoryMB = System.GC.GetTotalMemory(false) / (1024f * 1024f)
            };
        }

        /// <summary>
        /// 获取Unity分配的总内存（兼容不同Unity版本）
        /// </summary>
        /// <returns>内存大小（MB）</returns>
        private static float GetTotalAllocatedMemoryMB()
        {
            try
            {
#if UNITY_2020_2_OR_NEWER
                // Unity 2020.2+ 版本使用无参数方法
                return UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory() / (1024f * 1024f);
#else
                // 早期版本使用带参数方法
                return UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(false) / (1024f * 1024f);
#endif
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[MemoryInfo] Failed to get allocated memory: {ex.Message}");
                // 如果获取失败，使用GC内存作为替代
                return System.GC.GetTotalMemory(false) / (1024f * 1024f);
            }
        }
    }

    /// <summary>
    /// 卸载统计信息
    /// </summary>
    [Serializable]
    public sealed class UnloadStats
    {
        /// <summary>
        /// 总卸载次数
        /// </summary>
        public long TotalUnloadCount { get; set; }

        /// <summary>
        /// 最后卸载时间
        /// </summary>
        public DateTime LastUnloadTime { get; set; }

        /// <summary>
        /// 平均卸载耗时（毫秒）
        /// </summary>
        public double AverageUnloadTimeMs { get; set; }

        /// <summary>
        /// 各触发器的卸载次数
        /// </summary>
        public Dictionary<string, long> TriggerCounts { get; set; } = new();

        /// <summary>
        /// 各包的卸载次数
        /// </summary>
        public Dictionary<string, long> PackageUnloadCounts { get; set; } = new();

        /// <summary>
        /// 自启动以来的卸载历史（最近100次）
        /// </summary>
        public List<UnloadRecord> RecentUnloads { get; set; } = new();

        public override string ToString()
        {
            return $"UnloadStats: Total={TotalUnloadCount}, Last={LastUnloadTime:yyyy-MM-dd HH:mm:ss}, " +
                   $"AvgTime={AverageUnloadTimeMs:F1}ms";
        }
    }

    /// <summary>
    /// 卸载记录
    /// </summary>
    [Serializable]
    public sealed class UnloadRecord
    {
        public DateTime Time { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Package { get; set; } = string.Empty;
        public double DurationMs { get; set; }
        public int AssetCountBefore { get; set; }
        public int AssetCountAfter { get; set; }
    }
}