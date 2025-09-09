using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.AOT.AssetSystem.Core;
using UnityEngine;

namespace Game.AOT.AssetSystem.Pool
{
    /// <summary>
    /// 基于AssetScope的对象池提供者实现
    /// </summary>
    public sealed class AssetPoolProvider : IAssetPoolProvider
    {
        private sealed class PoolInfo
        {
            public string Package { get; }
            public string Address { get; }
            public GameObject Prefab = null!;
            public Transform PoolRoot = null!;
            public Queue<GameObject> Available { get; } = new();
            public HashSet<GameObject> Active { get; } = new();
            public PoolStats Stats { get; } = new();
            public int MaxCapacity { get; set; } = -1; // -1 = 无限制

            public PoolInfo(string package, string address)
            {
                Package = package;
                Address = address;
                Stats.Address = $"{package}/{address}";
                Stats.MaxCapacity = MaxCapacity;
            }
        }

        private readonly IAssetScope _assetScope;
        private readonly Dictionary<string, PoolInfo> _pools = new();
        private readonly Transform _poolContainer;
        private readonly object _lock = new();
        private bool _disposed;

        public AssetPoolProvider(IAssetScope assetScope)
        {
            _assetScope = assetScope ?? throw new ArgumentNullException(nameof(assetScope));
            
            // 创建池容器
            var containerGo = new GameObject("[AssetPoolProvider]");
            _poolContainer = containerGo.transform;
            UnityEngine.Object.DontDestroyOnLoad(containerGo);
            
            Debug.Log("[AssetPoolProvider] Initialized with pool container");
        }

        public async UniTask InitializePoolAsync<T>(string package, string address, int initialSize, int maxSize = -1, 
            Transform parent = null, CancellationToken ct = default) where T : Component
        {
            ThrowIfDisposed();
            
            var key = GetPoolKey(package, address);
            PoolInfo pool;
            
            lock (_lock)
            {
                if (_pools.TryGetValue(key, out pool))
                {
                    Debug.LogWarning($"[AssetPoolProvider] Pool already exists: {key}");
                    return;
                }
                
                pool = new PoolInfo(package, address) { MaxCapacity = maxSize };
                _pools[key] = pool;
                
                // 创建池根节点
                var poolRootGo = new GameObject($"Pool_{package}_{address.Replace('/', '_')}");
                pool.PoolRoot = poolRootGo.transform;
                pool.PoolRoot.SetParent(parent ?? _poolContainer);
            }

            try
            {
                // Pin预制体到资源作用域，确保在Pool生命周期内不被释放
                pool.Prefab = await _assetScope.PinAsync<GameObject>(package, address, ct);
                Debug.Log($"[AssetPoolProvider] Pinned prefab for pool: {key}");

                // 预创建对象
                for (int i = 0; i < initialSize; i++)
                {
                    var obj = CreatePoolObject(pool);
                    pool.Available.Enqueue(obj);
                }

                pool.Stats.TotalCreated = initialSize;
                pool.Stats.AvailableCount = initialSize;
                pool.Stats.TotalCount = initialSize;
                
                Debug.Log($"[AssetPoolProvider] Initialized pool: {key} with {initialSize} objects");
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    _pools.Remove(key);
                    if (pool.PoolRoot != null)
                    {
                        UnityEngine.Object.Destroy(pool.PoolRoot.gameObject);
                    }
                }
                
                Debug.LogError($"[AssetPoolProvider] Failed to initialize pool {key}: {ex.Message}");
                throw;
            }
        }

        public async UniTask<T> SpawnAsync<T>(string package, string address, Transform parent = null, 
            CancellationToken ct = default) where T : Component
        {
            ThrowIfDisposed();
            
            var key = GetPoolKey(package, address);
            PoolInfo pool;
            
            lock (_lock)
            {
                if (!_pools.TryGetValue(key, out pool))
                {
                    // 池不存在，自动初始化
                    Debug.LogWarning($"[AssetPoolProvider] Pool not found, auto-initializing: {key}");
                }
            }

            // 如果池不存在，先初始化
            if (pool == null)
            {
                await InitializePoolAsync<T>(package, address, 1, -1, null, ct);
                lock (_lock)
                {
                    pool = _pools[key];
                }
            }

            GameObject obj;
            lock (_lock)
            {
                if (pool.Available.Count > 0)
                {
                    obj = pool.Available.Dequeue();
                    pool.Stats.AvailableCount--;
                }
                else
                {
                    // 检查是否超出最大容量限制
                    if (pool.MaxCapacity > 0 && pool.Stats.TotalCount >= pool.MaxCapacity)
                    {
                        Debug.LogWarning($"[AssetPoolProvider] Pool reached max capacity: {key} ({pool.MaxCapacity})");
                        // 等待有对象归还或直接返回null（根据具体需求决定）
                        throw new InvalidOperationException($"Pool {key} reached maximum capacity ({pool.MaxCapacity})");
                    }

                    obj = CreatePoolObject(pool);
                    pool.Stats.TotalCreated++;
                    pool.Stats.TotalCount++;
                }

                pool.Active.Add(obj);
                pool.Stats.TotalSpawned++;
                pool.Stats.LastActiveTime = DateTime.Now;
            }

            // 激活对象并设置父级
            obj.SetActive(true);
            if (parent != null)
            {
                obj.transform.SetParent(parent, false);
            }

            var component = obj.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"[AssetPoolProvider] Object does not have component {typeof(T).Name}: {key}");
                Despawn(obj); // 归还到池中
                throw new InvalidOperationException($"Object does not have required component: {typeof(T).Name}");
            }

            Debug.Log($"[AssetPoolProvider] Spawned object: {key} (Active: {pool.Stats.ActiveCount})");
            return component;
        }

        public void Despawn(GameObject obj)
        {
            if (obj == null) return;
            ThrowIfDisposed();

            var poolKey = FindPoolKeyForObject(obj);
            if (poolKey == null)
            {
                Debug.LogWarning($"[AssetPoolProvider] Object not managed by any pool: {obj.name}");
                UnityEngine.Object.Destroy(obj);
                return;
            }

            lock (_lock)
            {
                if (!_pools.TryGetValue(poolKey, out var pool))
                {
                    Debug.LogWarning($"[AssetPoolProvider] Pool not found for object: {poolKey}");
                    UnityEngine.Object.Destroy(obj);
                    return;
                }

                if (!pool.Active.Remove(obj))
                {
                    Debug.LogWarning($"[AssetPoolProvider] Object not in active set: {obj.name}");
                    return;
                }

                // 重置对象状态
                obj.SetActive(false);
                obj.transform.SetParent(pool.PoolRoot, false);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;

                pool.Available.Enqueue(obj);
                pool.Stats.AvailableCount++;
                pool.Stats.TotalDespawned++;
                pool.Stats.LastActiveTime = DateTime.Now;
            }

            Debug.Log($"[AssetPoolProvider] Despawned object: {poolKey}");
        }

        public void Despawn<T>(T component) where T : Component
        {
            if (component == null) return;
            Despawn(component.gameObject);
        }

        public async UniTask WarmupPoolAsync(string package, string address, int count, CancellationToken ct = default)
        {
            ThrowIfDisposed();
            
            var key = GetPoolKey(package, address);
            if (!_pools.TryGetValue(key, out var pool))
            {
                Debug.LogWarning($"[AssetPoolProvider] Pool not found for warmup: {key}");
                return;
            }

            var objectsToCreate = Math.Max(0, count - pool.Stats.AvailableCount);
            if (objectsToCreate == 0)
            {
                Debug.Log($"[AssetPoolProvider] Pool already warmed up: {key}");
                return;
            }

            for (int i = 0; i < objectsToCreate; i++)
            {
                ct.ThrowIfCancellationRequested();
                
                lock (_lock)
                {
                    if (pool.MaxCapacity > 0 && pool.Stats.TotalCount >= pool.MaxCapacity)
                    {
                        Debug.LogWarning($"[AssetPoolProvider] Cannot warmup beyond max capacity: {key}");
                        break;
                    }

                    var obj = CreatePoolObject(pool);
                    pool.Available.Enqueue(obj);
                    pool.Stats.TotalCreated++;
                    pool.Stats.TotalCount++;
                    pool.Stats.AvailableCount++;
                }

                // 每创建10个对象让出一帧，避免卡顿
                if (i > 0 && i % 10 == 0)
                {
                    await UniTask.Yield();
                }
            }

            Debug.Log($"[AssetPoolProvider] Warmed up pool: {key} with {objectsToCreate} additional objects");
        }

        public void ClearPool(string package, string address)
        {
            ThrowIfDisposed();
            
            var key = GetPoolKey(package, address);
            
            lock (_lock)
            {
                if (!_pools.TryGetValue(key, out var pool))
                {
                    Debug.LogWarning($"[AssetPoolProvider] Pool not found for clearing: {key}");
                    return;
                }

                // 销毁所有活跃和可用对象
                foreach (var obj in pool.Active)
                {
                    if (obj != null) UnityEngine.Object.Destroy(obj);
                }
                while (pool.Available.Count > 0)
                {
                    var obj = pool.Available.Dequeue();
                    if (obj != null) UnityEngine.Object.Destroy(obj);
                }

                // 销毁池根节点
                if (pool.PoolRoot != null)
                {
                    UnityEngine.Object.Destroy(pool.PoolRoot.gameObject);
                }

                _pools.Remove(key);
            }

            Debug.Log($"[AssetPoolProvider] Cleared pool: {key}");
        }

        public void ClearAllPools()
        {
            ThrowIfDisposed();
            
            lock (_lock)
            {
                var poolKeys = _pools.Keys.ToList();
                foreach (var key in poolKeys)
                {
                    var parts = key.Split('|');
                    if (parts.Length >= 2)
                    {
                        ClearPool(parts[0], parts[1]);
                    }
                }
            }

            Debug.Log("[AssetPoolProvider] Cleared all pools");
        }

        public PoolStats GetPoolStats(string package, string address)
        {
            ThrowIfDisposed();
            
            var key = GetPoolKey(package, address);
            lock (_lock)
            {
                return _pools.TryGetValue(key, out var pool) ? pool.Stats : new PoolStats { Address = $"{package}/{address}" };
            }
        }

        public IReadOnlyDictionary<string, PoolStats> GetAllPoolStats()
        {
            ThrowIfDisposed();
            
            lock (_lock)
            {
                return _pools.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Stats);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            ClearAllPools();
            
            if (_poolContainer != null)
            {
                UnityEngine.Object.Destroy(_poolContainer.gameObject);
            }

            Debug.Log("[AssetPoolProvider] Disposed");
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AssetPoolProvider));
        }

        private static string GetPoolKey(string package, string address)
        {
            return $"{package}|{address}";
        }

        private GameObject CreatePoolObject(PoolInfo pool)
        {
            var obj = UnityEngine.Object.Instantiate(pool.Prefab, pool.PoolRoot);
            obj.name = $"{pool.Prefab.name}(Pooled)";
            obj.SetActive(false);
            
            // 添加池化标记组件
            var marker = obj.GetComponent<PooledObjectMarker>();
            if (marker == null)
            {
                marker = obj.AddComponent<PooledObjectMarker>();
            }
            marker.PoolKey = GetPoolKey(pool.Package, pool.Address);
            
            return obj;
        }

        private string FindPoolKeyForObject(GameObject obj)
        {
            var marker = obj.GetComponent<PooledObjectMarker>();
            return marker?.PoolKey;
        }
    }

    /// <summary>
    /// 池化对象标记组件：标识对象属于哪个池
    /// </summary>
    internal sealed class PooledObjectMarker : MonoBehaviour
    {
        public string PoolKey { get; set; } = string.Empty;
    }
}