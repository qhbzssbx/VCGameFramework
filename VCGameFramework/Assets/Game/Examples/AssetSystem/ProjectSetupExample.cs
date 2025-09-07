using System;
using Game.Core;
using Game.HotFix.AssetSystem.Core;
using Game.HotFix.AssetSystem.Module;
using Game.HotFix.AssetSystem.Pool;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.HotFix.AssetSystem.Examples
{
    /// <summary>
    /// 项目配置示例：展示如何在项目中完整配置资源管理系统
    /// </summary>
    public sealed class ProjectSetupExample : LifetimeScope
    {
        [SerializeField] private AssetSystemConfig _config;
        
        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("[ProjectSetupExample] Configuring asset management system...");
            
            // 1. 配置资源管理核心组件
            ConfigureAssetCore(builder);
            
            // 2. 配置地址映射
            ConfigureAddressMapping(builder);
            
            // 3. 配置对象池
            ConfigureObjectPool(builder);
            
            // 4. 配置卸载管理
            ConfigureUnloadManagement(builder);
            
            // 5. 注册扩展服务
            RegisterExtensions(builder);
            
            Debug.Log("[ProjectSetupExample] Asset management system configured successfully");
        }
        
        private void ConfigureAssetCore(IContainerBuilder builder)
        {
            // 注册资源管理模块 - 这会自动配置AssetRegistry和AssetScope
            var assetModule = new AssetSystemModule();
            assetModule.Configure(builder);
            
            // 可以覆盖默认配置
            builder.Register<IAssetRegistry>(provider =>
            {
                var config = _config ?? AssetSystemConfig.Default;
                return new AssetRegistry(TimeSpan.FromSeconds(config.DefaultTtlSeconds));
            }, Lifetime.Singleton);
            
            Debug.Log("[ProjectSetupExample] Asset core configured");
        }
        
        private void ConfigureAddressMapping(IContainerBuilder builder)
        {
            // 注册地址映射器
            builder.Register<IAddressMapper, AddressMapper>(Lifetime.Singleton);
            
            // 配置映射规则
            builder.RegisterBuildCallback(resolver =>
            {
                var mapper = resolver.Resolve<IAddressMapper>();
                
                // 低画质设备使用低分辨率纹理
                mapper.RegisterMappingRule(new PrefixAddressMappingRule(
                    priority: 100,
                    prefix: "ui/textures/",
                    newPrefix: "ui/textures/low/",
                    contextPredicate: ctx => ctx.QualityLevel == "Low"));
                
                // 移动设备使用压缩音频
                mapper.RegisterMappingRule(new RegexAddressMappingRule(
                    priority: 90,
                    pattern: @"audio/(.+)\.wav$",
                    replacement: "audio/$1.ogg",
                    contextPredicate: ctx => ctx.DeviceType == "Mobile"));
                
                // 中文地区使用本地化资源
                mapper.RegisterMappingRule(new PrefixAddressMappingRule(
                    priority: 80,
                    prefix: "ui/localization/",
                    newPrefix: "ui/localization/cn/",
                    contextPredicate: ctx => ctx.RegionCode == "CN"));
                
                Debug.Log("[ProjectSetupExample] Address mapping rules configured");
            });
        }
        
        private void ConfigureObjectPool(IContainerBuilder builder)
        {
            // 注册对象池提供者
            builder.Register<IAssetPoolProvider>(provider =>
            {
                var scope = provider.Resolve<IAssetScope>();
                return new AssetPoolProvider(scope);
            }, Lifetime.Scoped);
            
            Debug.Log("[ProjectSetupExample] Object pool configured");
        }
        
        private void ConfigureUnloadManagement(IContainerBuilder builder)
        {
            // 注册卸载管理器
            builder.Register<IAssetUnloadManager>(provider =>
            {
                var registry = provider.Resolve<IAssetRegistry>();
                return new AssetUnloadManager(registry);
            }, Lifetime.Singleton);
            
            // 配置卸载策略和触发器
            builder.RegisterBuildCallback(resolver =>
            {
                var unloadManager = resolver.Resolve<IAssetUnloadManager>();
                var config = _config ?? AssetSystemConfig.Default;
                
                // 设置内存压力策略
                var memoryStrategy = new MemoryPressureUnloadStrategy(
                    config.MemoryThreshold, TimeSpan.FromMinutes(config.CheckIntervalMinutes));
                unloadManager.SetUnloadStrategy(memoryStrategy);
                
                // 注册定时卸载触发器
                var timeTrigger = new TimeBasedUnloadTrigger(TimeSpan.FromMinutes(config.AutoUnloadIntervalMinutes));
                unloadManager.RegisterUnloadTrigger(timeTrigger);
                
                // 注册低内存触发器
                var lowMemoryTrigger = new LowMemoryUnloadTrigger();
                unloadManager.RegisterUnloadTrigger(lowMemoryTrigger);
                
                // 启动自动卸载
                unloadManager.StartAsync();
                
                Debug.Log("[ProjectSetupExample] Unload management configured");
            });
        }
        
        private void RegisterExtensions(IContainerBuilder builder)
        {
            // 注册UI资源加载器 - 使用新的资源管理系统
            builder.Register<Game.UI.Core.IUIResourceLoader>(provider =>
            {
                var registry = provider.Resolve<IAssetRegistry>();
                return new Game.UI.Core.ScopedUIResourceLoader(registry);
            }, Lifetime.Scoped);
            
            // 注册资源监控服务
            builder.Register<AssetMonitorService>(Lifetime.Singleton);
            
            Debug.Log("[ProjectSetupExample] Extensions registered");
        }
    }
    
    /// <summary>
    /// 资源系统配置
    /// </summary>
    [Serializable]
    public sealed class AssetSystemConfig
    {
        [Header("TTL Configuration")]
        [SerializeField] public int DefaultTtlSeconds = 15;
        
        [Header("Memory Management")]
        [SerializeField] public float MemoryThreshold = 0.8f;
        [SerializeField] public int CheckIntervalMinutes = 2;
        [SerializeField] public int AutoUnloadIntervalMinutes = 10;
        
        [Header("Pool Configuration")]
        [SerializeField] public int DefaultPoolSize = 10;
        [SerializeField] public int MaxPoolSize = 50;
        
        public static AssetSystemConfig Default => new AssetSystemConfig
        {
            DefaultTtlSeconds = 15,
            MemoryThreshold = 0.8f,
            CheckIntervalMinutes = 2,
            AutoUnloadIntervalMinutes = 10,
            DefaultPoolSize = 10,
            MaxPoolSize = 50
        };
    }
    
    /// <summary>
    /// 基于时间的卸载触发器
    /// </summary>
    public sealed class TimeBasedUnloadTrigger : IUnloadTrigger
    {
        private readonly TimeSpan _interval;
        private DateTime _lastTrigger = DateTime.MinValue;
        
        public string Name => "TimeBased";
        
        public TimeBasedUnloadTrigger(TimeSpan interval)
        {
            _interval = interval;
        }
        
        public bool ShouldUnload(UnloadContext context)
        {
            if (context.CurrentTime - _lastTrigger >= _interval)
            {
                _lastTrigger = context.CurrentTime;
                return true;
            }
            return false;
        }
        
        public System.Collections.Generic.IReadOnlyList<string> GetPackagesToUnload(UnloadContext context)
        {
            return null; // 卸载所有包
        }
    }
    
    /// <summary>
    /// 低内存卸载触发器
    /// </summary>
    public sealed class LowMemoryUnloadTrigger : IUnloadTrigger
    {
        public string Name => "LowMemory";
        
        public bool ShouldUnload(UnloadContext context)
        {
            return context.IsLowMemory && context.AssetStats.TtlQueueCount > 5;
        }
        
        public System.Collections.Generic.IReadOnlyList<string> GetPackagesToUnload(UnloadContext context)
        {
            return null; // 卸载所有包
        }
    }
    
    /// <summary>
    /// 资源监控服务
    /// </summary>
    public sealed class AssetMonitorService : IDisposable
    {
        private readonly IAssetRegistry _registry;
        private readonly IAssetUnloadManager _unloadManager;
        private bool _disposed;
        
        public AssetMonitorService(IAssetRegistry registry, IAssetUnloadManager unloadManager)
        {
            _registry = registry;
            _unloadManager = unloadManager;
            
            // 启动监控
            StartMonitoring().Forget();
        }
        
        private async Cysharp.Threading.Tasks.UniTaskVoid StartMonitoring()
        {
            Debug.Log("[AssetMonitorService] Starting resource monitoring...");
            
            while (!_disposed)
            {
                try
                {
                    await Cysharp.Threading.Tasks.UniTask.Delay(TimeSpan.FromMinutes(1));
                    
                    var stats = _registry.GetStats();
                    var unloadStats = _unloadManager.GetUnloadStats();
                    
                    Debug.Log($"[AssetMonitorService] Stats - Assets: {stats.CachedAssetCount}, " +
                             $"Hit Rate: {stats.CacheHitRate:P2}, Memory: {MemoryInfo.GetCurrent().UsageRatio:P2}");
                    
                    // 检查异常情况
                    if (stats.FailureRate > 0.1f) // 失败率超过10%
                    {
                        Debug.LogWarning($"[AssetMonitorService] High failure rate detected: {stats.FailureRate:P2}");
                    }
                    
                    if (stats.CachedAssetCount > 100) // 缓存资源超过100个
                    {
                        Debug.LogWarning($"[AssetMonitorService] High asset count: {stats.CachedAssetCount}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[AssetMonitorService] Monitoring error: {ex.Message}");
                }
            }
        }
        
        public void Dispose()
        {
            _disposed = true;
            Debug.Log("[AssetMonitorService] Monitoring stopped");
        }
    }
}