using System;
using System.Collections.Generic;

namespace Game.HotFix.AssetSystem.Core
{
    /// <summary>
    /// 资源统计信息
    /// </summary>
    [Serializable]
    public sealed class AssetStats
    {
        /// <summary>
        /// 当前缓存的资源数量
        /// </summary>
        public int CachedAssetCount { get; set; }
        
        /// <summary>
        /// 总引用计数
        /// </summary>
        public int TotalRefCount { get; set; }
        
        /// <summary>
        /// TTL队列中的资源数量
        /// </summary>
        public int TtlQueueCount { get; set; }
        
        /// <summary>
        /// 累计加载次数
        /// </summary>
        public long TotalLoadCount { get; set; }
        
        /// <summary>
        /// 缓存命中次数
        /// </summary>
        public long CacheHitCount { get; set; }
        
        /// <summary>
        /// 失败加载次数
        /// </summary>
        public long FailedLoadCount { get; set; }
        
        /// <summary>
        /// 平均加载时间（毫秒）
        /// </summary>
        public double AverageLoadTimeMs { get; set; }
        
        /// <summary>
        /// TopN最常用的资源地址及其使用次数
        /// </summary>
        public Dictionary<string, int> TopAddresses { get; set; } = new();
        
        /// <summary>
        /// 当前正在加载的资源数量
        /// </summary>
        public int CurrentLoadingCount { get; set; }
        
        /// <summary>
        /// 缓存命中率
        /// </summary>
        public double CacheHitRate => TotalLoadCount > 0 ? (double)CacheHitCount / TotalLoadCount : 0.0;
        
        /// <summary>
        /// 失败率
        /// </summary>
        public double FailureRate => TotalLoadCount > 0 ? (double)FailedLoadCount / TotalLoadCount : 0.0;
        
        /// <summary>
        /// 统计快照时间
        /// </summary>
        public DateTime SnapshotTime { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return $"AssetStats: Cached={CachedAssetCount}, RefCount={TotalRefCount}, " +
                   $"HitRate={CacheHitRate:P2}, FailRate={FailureRate:P2}, " +
                   $"AvgLoadTime={AverageLoadTimeMs:F1}ms, Loading={CurrentLoadingCount}";
        }
    }

    /// <summary>
    /// 单个资源的详细信息
    /// </summary>
    [Serializable]
    public sealed class AssetInfo
    {
        public string Package { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public int RefCount { get; set; }
        public bool IsInTtlQueue { get; set; }
        public bool IsLoading { get; set; }
        public DateTime LastAccessTime { get; set; }
        public int AccessCount { get; set; }
        public double LoadTimeMs { get; set; }
    }
}