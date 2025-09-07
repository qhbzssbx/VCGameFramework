using System.Threading;
using Cysharp.Threading.Tasks;
using Game.HotFix.AssetSystem.Pool;
using UnityEngine;
using VContainer;

namespace Game.HotFix.AssetSystem.Examples
{
    /// <summary>
    /// 对象池使用示例：展示如何使用AssetPoolProvider管理高频使用的对象
    /// </summary>
    public sealed class ObjectPoolExample : MonoBehaviour
    {
        [Inject] private IAssetPoolProvider _poolProvider;
        [Inject] private CancellationToken _scopeCancellationToken;
        
        private const string BULLET_PACKAGE = "effects";
        private const string BULLET_ADDRESS = "bullet/StandardBullet";
        
        private void Start()
        {
            // 异步初始化对象池
            InitializePoolsAsync(_scopeCancellationToken).Forget();
        }
        
        private async UniTask InitializePoolsAsync(CancellationToken ct)
        {
            try
            {
                Debug.Log("[ObjectPoolExample] Initializing bullet pool...");
                
                // 初始化子弹池：初始10个，最大50个
                await _poolProvider.InitializePoolAsync<BulletComponent>(
                    BULLET_PACKAGE, BULLET_ADDRESS, 
                    initialSize: 10, maxSize: 50, 
                    parent: transform, ct: ct);
                
                // 预热池 - 创建额外的20个对象
                await _poolProvider.WarmupPoolAsync(BULLET_PACKAGE, BULLET_ADDRESS, 20, ct);
                
                Debug.Log("[ObjectPoolExample] Pool initialized successfully");
                
                // 显示池统计信息
                ShowPoolStats();
                
                // 开始模拟发射
                StartSimulatedShooting(ct).Forget();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ObjectPoolExample] Failed to initialize pool: {ex.Message}");
            }
        }
        
        private async UniTaskVoid StartSimulatedShooting(CancellationToken ct)
        {
            Debug.Log("[ObjectPoolExample] Starting simulated shooting...");
            
            try
            {
                // 模拟每秒发射5发子弹
                while (!ct.IsCancellationRequested)
                {
                    await UniTask.Delay(200, cancellationToken: ct); // 每200ms发射一次
                    
                    FireBullet(ct).Forget();
                }
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("[ObjectPoolExample] Simulated shooting stopped");
            }
        }
        
        private async UniTaskVoid FireBullet(CancellationToken ct)
        {
            try
            {
                // 从池中获取子弹
                var bullet = await _poolProvider.SpawnAsync<BulletComponent>(
                    BULLET_PACKAGE, BULLET_ADDRESS, parent: transform, ct: ct);
                
                // 设置子弹初始位置和方向
                bullet.transform.position = transform.position + Random.insideUnitSphere;
                bullet.transform.forward = Random.onUnitSphere;
                
                Debug.Log($"[ObjectPoolExample] Fired bullet: {bullet.name}");
                
                // 模拟子弹飞行2秒后回收
                _ = UniTask.Delay(2000, cancellationToken: ct)
                    .ContinueWith(() => _poolProvider.Despawn(bullet))
                    .SuppressCancellationThrow();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ObjectPoolExample] Failed to fire bullet: {ex.Message}");
            }
        }
        
        private void ShowPoolStats()
        {
            var stats = _poolProvider.GetPoolStats(BULLET_PACKAGE, BULLET_ADDRESS);
            Debug.Log($"[ObjectPoolExample] Pool Stats: {stats}");
            
            // 每5秒显示一次统计信息
            _ = UniTask.Create(async () =>
            {
                while (!_scopeCancellationToken.IsCancellationRequested)
                {
                    await UniTask.Delay(5000, cancellationToken: _scopeCancellationToken);
                    var currentStats = _poolProvider.GetPoolStats(BULLET_PACKAGE, BULLET_ADDRESS);
                    Debug.Log($"[ObjectPoolExample] Current Pool Stats: {currentStats}");
                }
            }).SuppressCancellationThrow();
        }
        
        private void OnDestroy()
        {
            // 清理池（可选，因为PoolProvider会在Scope销毁时自动清理）
            _poolProvider?.ClearPool(BULLET_PACKAGE, BULLET_ADDRESS);
            Debug.Log("[ObjectPoolExample] Destroyed and cleaned up pool");
        }
    }
    
    /// <summary>
    /// 示例子弹组件
    /// </summary>
    public sealed class BulletComponent : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private ParticleSystem trail;
        
        private void OnEnable()
        {
            // 子弹被激活时的逻辑
            if (trail != null)
            {
                trail.Play();
            }
            
            Debug.Log($"[BulletComponent] Bullet enabled: {name}");
        }
        
        private void OnDisable()
        {
            // 子弹被回收时的清理逻辑
            if (trail != null)
            {
                trail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            
            Debug.Log($"[BulletComponent] Bullet disabled: {name}");
        }
        
        private void Update()
        {
            // 简单的移动逻辑
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }
}