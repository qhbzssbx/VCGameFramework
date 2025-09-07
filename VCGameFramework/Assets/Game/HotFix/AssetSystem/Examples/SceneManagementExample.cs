// using System.Collections.Generic;
// using System.Threading;
// using Cysharp.Threading.Tasks;
// using Game.HotFix.AssetSystem.Core;
// using Game.HotFix.AssetSystem.Pool;
// using Game.HotFix.AssetSystem.Utilities;
// using UnityEngine;
// using VContainer;
// using VContainer.Unity;

// namespace Game.HotFix.AssetSystem.Examples
// {
//     /// <summary>
//     /// 场景管理使用示例：展示如何在场景切换时使用资源管理系统
//     /// </summary>
//     public sealed class SceneManagementExample : MonoBehaviour
//     {
//         [Inject] private IAssetRegistry _assetRegistry;
//         [Inject] private IAssetUnloadManager _unloadManager;
//         [Inject] private IAssetPoolProvider _poolProvider;
//         [Inject] private LifetimeScope _lifetimeScope;
        
//         private LifetimeScope _currentSceneScope;
//         private readonly Dictionary<string, SceneAssets> _sceneAssets = new();
        
//         private void Start()
//         {
//             // 设置卸载策略和触发器
//             SetupUnloadSystem();
            
//             // 预热常用资源
//             PrewarmCommonAssets().Forget();
//         }
        
//         private void SetupUnloadSystem()
//         {
//             Debug.Log("[SceneManagementExample] Setting up unload system...");
            
//             // 使用内存压力策略
//             var memoryStrategy = new MemoryPressureUnloadStrategy(0.75f, System.TimeSpan.FromMinutes(1));
//             _unloadManager.SetUnloadStrategy(memoryStrategy);
            
//             // 注册场景切换触发器
//             var sceneTrigger = new SceneChangeTrigger();
//             _unloadManager.RegisterUnloadTrigger(sceneTrigger);
            
//             // 启动自动卸载
//             _unloadManager.StartAsync().Forget();
            
//             Debug.Log("[SceneManagementExample] Unload system configured");
//         }
        
//         private async UniTask PrewarmCommonAssets()
//         {
//             Debug.Log("[SceneManagementExample] Prewarming common assets...");
            
//             try
//             {
//                 // 预热UI相关资源
//                 await _assetRegistry.PrewarmAsync<GameObject>("ui", "common/LoadingScreen");
//                 await _assetRegistry.PrewarmAsync<GameObject>("ui", "common/MessageBox");
                
//                 // 预热音效资源
//                 await _assetRegistry.PrewarmAsync<AudioClip>("audio", "sfx/ButtonClick");
//                 await _assetRegistry.PrewarmAsync<AudioClip>("audio", "sfx/WindowOpen");
                
//                 Debug.Log("[SceneManagementExample] Common assets prewarmed");
//             }
//             catch (System.Exception ex)
//             {
//                 Debug.LogError($"[SceneManagementExample] Failed to prewarm assets: {ex.Message}");
//             }
//         }
        
//         public async UniTask LoadSceneAsync(string sceneName, CancellationToken ct = default)
//         {
//             Debug.Log($"[SceneManagementExample] Loading scene: {sceneName}");
            
//             try
//             {
//                 // 1. 显示加载界面
//                 await ShowLoadingScreenAsync(ct);
                
//                 // 2. 清理当前场景资源
//                 await CleanupCurrentSceneAsync(ct);
                
//                 // 3. 创建新场景的资源作用域
//                 _currentSceneScope = _lifetimeScope.CreateChild(builder =>
//                 {
//                     // 为新场景配置专门的资源服务
//                     var cts = new CancellationTokenSource();
//                     builder.RegisterInstance(cts.Token);
                    
//                     builder.Register<IAssetScope>(provider =>
//                     {
//                         var registry = provider.Resolve<IAssetRegistry>();
//                         var token = provider.Resolve<CancellationToken>();
//                         return new Game.HotFix.AssetSystem.Core.AssetScope(registry, token);
//                     }, Lifetime.Scoped).AsImplementedInterfaces().As<IAsyncStartable>();
                    
//                     builder.Register<IAssetPoolProvider>(provider =>
//                     {
//                         var scope = provider.Resolve<IAssetScope>();
//                         return new AssetPoolProvider(scope);
//                     }, Lifetime.Scoped);
//                 });
                
//                 _currentSceneScope.name = $"Scene_{sceneName}";
                
//                 // 4. 加载场景特定资源
//                 await LoadSceneAssetsAsync(sceneName, ct);
                
//                 // 5. 模拟场景加载
//                 await UniTask.Delay(1000, cancellationToken: ct);
                
//                 // 6. 隐藏加载界面
//                 await HideLoadingScreenAsync(ct);
                
//                 Debug.Log($"[SceneManagementExample] Scene loaded successfully: {sceneName}");
//             }
//             catch (System.Exception ex)
//             {
//                 Debug.LogError($"[SceneManagementExample] Failed to load scene {sceneName}: {ex.Message}");
//                 throw;
//             }
//         }
        
//         private async UniTask LoadSceneAssetsAsync(string sceneName, CancellationToken ct)
//         {
//             var sceneScope = _currentSceneScope.Container.Resolve<IAssetScope>();
//             var scenePool = _currentSceneScope.Container.Resolve<IAssetPoolProvider>();
//             var sceneAssets = new SceneAssets();
            
//             switch (sceneName)
//             {
//                 case "MainCity":
//                     // 主城场景资源
//                     sceneAssets.Background = await sceneScope.PinAsync<GameObject>("scene", "city/CityBackground", ct);
//                     sceneAssets.BGM = await sceneScope.PinAsync<AudioClip>("audio", "bgm/CityTheme", ct);
                    
//                     // 初始化NPC池
//                     await scenePool.InitializePoolAsync<NPCComponent>("scene", "city/NPC_Citizen", 20, 50, ct: ct);
//                     break;
                    
//                 case "Dungeon":
//                     // 地牢场景资源
//                     sceneAssets.Background = await sceneScope.PinAsync<GameObject>("scene", "dungeon/DungeonBackground", ct);
//                     sceneAssets.BGM = await sceneScope.PinAsync<AudioClip>("audio", "bgm/DungeonTheme", ct);
                    
//                     // 初始化怪物和特效池
//                     await scenePool.InitializePoolAsync<MonsterComponent>("scene", "dungeon/Monster_Goblin", 15, 30, ct: ct);
//                     await scenePool.InitializePoolAsync<ParticleSystem>("effects", "combat/HitEffect", 10, 20, ct: ct);
//                     break;
                    
//                 case "Arena":
//                     // 竞技场场景资源
//                     sceneAssets.Background = await sceneScope.PinAsync<GameObject>("scene", "arena/ArenaBackground", ct);
//                     sceneAssets.BGM = await sceneScope.PinAsync<AudioClip>("audio", "bgm/ArenaTheme", ct);
                    
//                     // 初始化技能特效池
//                     await scenePool.InitializePoolAsync<ParticleSystem>("effects", "skills/FireBall", 5, 15, ct: ct);
//                     await scenePool.InitializePoolAsync<ParticleSystem>("effects", "skills/Lightning", 5, 15, ct: ct);
//                     break;
//             }
            
//             _sceneAssets[sceneName] = sceneAssets;
//             Debug.Log($"[SceneManagementExample] Loaded assets for scene: {sceneName}");
//         }
        
//         private async UniTask ShowLoadingScreenAsync(CancellationToken ct)
//         {
//             // 使用全局资源加载界面
//             var loadingPrefab = await _assetRegistry.AcquireAsync<GameObject>("ui", "common/LoadingScreen", ct);
//             var loadingScreen = Instantiate(loadingPrefab);
//             loadingScreen.name = "LoadingScreen";
            
//             // 模拟加载动画
//             await UniTask.Delay(500, cancellationToken: ct);
            
//             Debug.Log("[SceneManagementExample] Loading screen shown");
//         }
        
//         private async UniTask HideLoadingScreenAsync(CancellationToken ct)
//         {
//             var loadingScreen = GameObject.Find("LoadingScreen");
//             if (loadingScreen != null)
//             {
//                 Destroy(loadingScreen);
//             }
            
//             // 释放加载界面资源
//             _assetRegistry.Release("ui", "common/LoadingScreen", typeof(GameObject));
            
//             await UniTask.Delay(200, cancellationToken: ct);
//             Debug.Log("[SceneManagementExample] Loading screen hidden");
//         }
        
//         private async UniTask CleanupCurrentSceneAsync(CancellationToken ct)
//         {
//             if (_currentSceneScope != null)
//             {
//                 Debug.Log("[SceneManagementExample] Cleaning up current scene...");
                
//                 // 销毁场景Scope - 这会自动释放所有Pin的资源和清理对象池
//                 _currentSceneScope.Dispose();
//                 _currentSceneScope = null;
                
//                 // 触发一次资源卸载
//                 await _unloadManager.TriggerUnloadAsync(reason: "SceneChange", ct: ct);
                
//                 Debug.Log("[SceneManagementExample] Current scene cleaned up");
//             }
//         }
        
//         public void ShowResourceStats()
//         {
//             var stats = _assetRegistry.GetStats();
//             var unloadStats = _unloadManager.GetUnloadStats();
            
//             Debug.Log($"[SceneManagementExample] Asset Stats: {stats}");
//             Debug.Log($"[SceneManagementExample] Unload Stats: {unloadStats}");
            
//             // 显示池统计
//             var poolStats = _poolProvider.GetAllPoolStats();
//             foreach (var kvp in poolStats)
//             {
//                 Debug.Log($"[SceneManagementExample] Pool {kvp.Key}: {kvp.Value}");
//             }
//         }
        
//         private void OnDestroy()
//         {
//             // 清理当前场景
//             CleanupCurrentSceneAsync().Forget();
            
//             // 停止卸载管理器
//             _unloadManager?.StopAsync().Forget();
//         }
        
//         // 私有类
//         private sealed class SceneAssets
//         {
//             public GameObject Background;
//             public AudioClip BGM;
//         }
        
//         private sealed class SceneChangeTrigger : IUnloadTrigger
//         {
//             public string Name => "SceneChange";
            
//             public bool ShouldUnload(UnloadContext context)
//             {
//                 // 在场景切换时总是触发卸载
//                 return context.Properties.ContainsKey("SceneChanged");
//             }
            
//             public IReadOnlyList<string> GetPackagesToUnload(UnloadContext context)
//             {
//                 // 卸载所有包
//                 return null;
//             }
//         }
//     }
    
//     // 示例组件
//     public sealed class NPCComponent : MonoBehaviour
//     {
//         public void OnSpawned()
//         {
//             Debug.Log($"[NPCComponent] NPC spawned: {name}");
//         }
//     }
    
//     public sealed class MonsterComponent : MonoBehaviour
//     {
//         public void OnSpawned()
//         {
//             Debug.Log($"[MonsterComponent] Monster spawned: {name}");
//         }
//     }
// }