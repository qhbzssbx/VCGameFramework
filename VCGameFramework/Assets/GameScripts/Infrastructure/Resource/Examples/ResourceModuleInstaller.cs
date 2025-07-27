using Game.Infrastructure.Resource.Providers.YooAssetProvider;
using Game.Infrastructure.Resource.Core;
using UnityEngine;
using VContainer;

namespace Game.Infrastructure.Resource.Examples
{
    /// <summary>
    /// 资源模块VContainer配置示例 - 简化版本
    /// 
    /// 新的使用方式说明：
    /// 
    /// 1. 模块注册（仅注册IResourceService用于初始化）：
    ///    builder.Register<IResourceService, YooAssetResourceService>(Lifetime.Singleton);
    /// 
    /// 2. 实际使用（在需要资源的类中）：
    ///    private ResourceLoader _loader = new();
    ///    var handle = await _loader.LoadAssetAsync<Texture2D>("AssetName");
    ///    // 或绑定到GameObject: 
    ///    var handle = await _loader.LoadAssetAsync<Texture2D>("AssetName", this.gameObject);
    /// 
    /// 3. 清理资源：
    ///    _loader.Dispose(); // 在OnDestroy中调用
    /// </summary>
    public class ResourceModuleInstaller : MonoBehaviour
    {
        [Header("示例用法")]
        [SerializeField] private bool showUsageExample = true;
        
        private ResourceLoader _exampleLoader;

        public void Install(IContainerBuilder builder)
        {
            // 仅注册资源服务用于初始化YooAsset
            builder.Register<IResourceService, YooAssetResourceService>(Lifetime.Singleton);

            Debug.Log("资源模块已注册 - 简化版本，使用ResourceLoader进行实际资源加载");
        }

        /// <summary>
        /// 演示新的资源加载方式
        /// </summary>
        private async void Start()
        {
            if (!showUsageExample) return;
            
            Debug.Log("=== ResourceModuleInstaller使用示例 ===");
            
            // 创建ResourceLoader实例
            _exampleLoader = new ResourceLoader();
            
            try
            {
                // 示例：加载资源并绑定到当前GameObject
                // var handle = await _exampleLoader.LoadAssetAsync<Texture2D>("ExampleTexture", this.gameObject);
                
                // 示例：加载Prefab资源用于实例化
                // var prefabHandle = await _exampleLoader.LoadPrefabForInstantiate<GameObject>("ExamplePrefab");
                
                Debug.Log("ResourceLoader使用示例：请查看代码中的注释");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"示例资源加载失败（这是正常的，因为示例资源可能不存在）: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理示例ResourceLoader
        /// </summary>
        private void OnDestroy()
        {
            _exampleLoader?.Dispose();
            Debug.Log("ResourceModuleInstaller示例清理完成");
        }

        /// <summary>
        /// Inspector测试方法
        /// </summary>
        [ContextMenu("显示使用说明")]
        private void ShowUsageInstructions()
        {
            Debug.Log(@"
=== 新资源系统使用说明 ===

1. 初始化（通过ResourceModule自动完成）：
   - IResourceService负责YooAsset初始化

2. 日常使用：
   private ResourceLoader _loader = new();
   
   // 通用资源加载
   var handle = await _loader.LoadAssetAsync<Texture2D>(""AssetName"");
   
   // 绑定到GameObject自动释放
   var handle = await _loader.LoadAssetAsync<Texture2D>(""AssetName"", this.gameObject);
   
   // Prefab专用加载
   var prefabHandle = await _loader.LoadPrefabForInstantiate<GameObject>(""PrefabName"");

3. 资源清理：
   void OnDestroy() 
   {
       _loader?.Dispose(); // 手动释放ResourceLoader管理的资源
       // GameObject绑定的资源会通过AutoResourceRelease自动释放
   }

参考示例：
- BasicUsageExample：基础用法
- ManagerPatternExample：Manager模式
- UIResourceExample：UI资源管理
            ");
        }
    }
}