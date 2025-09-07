using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.HotFix.AssetSystem.Pool
{
    /// <summary>
    /// 资源对象池提供者接口：管理预制体的池化和资源生命周期
    /// </summary>
    public interface IAssetPoolProvider : IDisposable
    {
        /// <summary>
        /// 初始化对象池，预加载指定数量的对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="package">资源包名</param>
        /// <param name="address">资源地址</param>
        /// <param name="initialSize">初始池大小</param>
        /// <param name="maxSize">最大池大小</param>
        /// <param name="parent">对象父级</param>
        /// <param name="ct">取消令牌</param>
        /// <returns>初始化任务</returns>
        UniTask InitializePoolAsync<T>(string package, string address, int initialSize, int maxSize = -1,
            Transform parent = null, CancellationToken ct = default) where T : Component;

        /// <summary>
        /// 从池中获取对象，如果池为空则创建新对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="package">资源包名</param>
        /// <param name="address">资源地址</param>
        /// <param name="parent">对象父级</param>
        /// <param name="ct">取消令牌</param>
        /// <returns>池化对象</returns>
        UniTask<T> SpawnAsync<T>(string package, string address, Transform parent = null, 
            CancellationToken ct = default) where T : Component;

        /// <summary>
        /// 将对象归还到池中
        /// </summary>
        /// <param name="obj">要归还的对象</param>
        void Despawn(GameObject obj);

        /// <summary>
        /// 将组件对象归还到池中
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="component">要归还的组件</param>
        void Despawn<T>(T component) where T : Component;

        /// <summary>
        /// 预热指定池，提前创建对象
        /// </summary>
        /// <param name="package">资源包名</param>
        /// <param name="address">资源地址</param>
        /// <param name="count">预热数量</param>
        /// <param name="ct">取消令牌</param>
        /// <returns>预热任务</returns>
        UniTask WarmupPoolAsync(string package, string address, int count, CancellationToken ct = default);

        /// <summary>
        /// 清理指定池中的所有对象
        /// </summary>
        /// <param name="package">资源包名</param>
        /// <param name="address">资源地址</param>
        void ClearPool(string package, string address);

        /// <summary>
        /// 清理所有池
        /// </summary>
        void ClearAllPools();

        /// <summary>
        /// 获取池统计信息
        /// </summary>
        /// <param name="package">资源包名</param>
        /// <param name="address">资源地址</param>
        /// <returns>池统计信息</returns>
        PoolStats GetPoolStats(string package, string address);

        /// <summary>
        /// 获取所有池的统计信息
        /// </summary>
        /// <returns>所有池的统计信息</returns>
        System.Collections.Generic.IReadOnlyDictionary<string, PoolStats> GetAllPoolStats();
    }

    /// <summary>
    /// 池统计信息
    /// </summary>
    [Serializable]
    public sealed class PoolStats
    {
        /// <summary>
        /// 池中可用对象数量
        /// </summary>
        public int AvailableCount { get; set; }

        /// <summary>
        /// 已分配的总对象数量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 活跃（已借出）对象数量
        /// </summary>
        public int ActiveCount => TotalCount - AvailableCount;

        /// <summary>
        /// 池的最大容量
        /// </summary>
        public int MaxCapacity { get; set; }

        /// <summary>
        /// 累计创建次数
        /// </summary>
        public long TotalCreated { get; set; }

        /// <summary>
        /// 累计获取次数
        /// </summary>
        public long TotalSpawned { get; set; }

        /// <summary>
        /// 累计归还次数
        /// </summary>
        public long TotalDespawned { get; set; }

        /// <summary>
        /// 池命中率（归还次数/获取次数）
        /// </summary>
        public double HitRate => TotalSpawned > 0 ? (double)TotalDespawned / TotalSpawned : 0.0;

        /// <summary>
        /// 资源地址
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 最后活跃时间
        /// </summary>
        public DateTime LastActiveTime { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return $"Pool({Address}): {AvailableCount}/{TotalCount} available, " +
                   $"Hit Rate: {HitRate:P2}, Total Created: {TotalCreated}";
        }
    }
}