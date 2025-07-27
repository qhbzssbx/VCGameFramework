using Cysharp.Threading.Tasks;
using Game.Infrastructure.Resource.Core;
using UnityEngine;
using VContainer;

namespace Game.Infrastructure.Resource.Examples
{
    /// <summary>
    /// 简化的资源系统使用示例
    /// 注意：这个类没有实现IResourceHandleOwner，会使用兼容模式
    /// </summary>
    public class ResourceUsageExample : MonoBehaviour
    {
        [Inject] private IResourceService _resourceService;
        
        private ResourceHandle<AudioClip> _bgmHandle;

        private async void Start()
        {
            await DemonstrateResourceUsage();
        }

        private async UniTask DemonstrateResourceUsage()
        {
            Debug.Log("=== 简化的资源系统使用示例 ===");

            // 示例 1: 自动生命周期管理（推荐）
            await Example1_AutoLifecycle();

            // 示例 2: 手动管理
            await Example2_ManualManagement();

            // 示例 3: Using 语句
            await Example3_UsingStatement();

            // 示例 4: 批量加载
            await Example4_BatchLoading();

            // 示例 5: 预加载
            await Example5_Preloading();

            Debug.Log("=== 所有示例完成 ===");
        }

        /// <summary>
        /// 示例 1: 自动生命周期管理（推荐方式）
        /// </summary>
        private async UniTask Example1_AutoLifecycle()
        {
            Debug.Log("示例 1: 自动生命周期管理");
            
            try
            {
                // 绑定到当前MonoBehaviour，销毁时自动释放
                var iconHandle = await _resourceService.LoadAssetAsync<Texture2D>("PlayerIcon", this);
                
                // 隐式转换使用
                if (iconHandle) // 隐式转换到bool
                {
                    Texture2D texture = iconHandle; // 隐式转换到Texture2D
                    Debug.Log($"加载纹理: {texture.name}, 尺寸: {texture.width}x{texture.height}");
                }
                
                // 无需手动释放，组件销毁时自动处理
                Debug.Log("Handle会在组件销毁时自动释放");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"加载PlayerIcon失败: {ex.Message}");
            }
        }



        /// <summary>
        /// 示例 2: 手动管理
        /// </summary>
        private async UniTask Example2_ManualManagement()
        {
            Debug.Log("示例 4: 手动管理");
            
            try
            {
                _bgmHandle = await _resourceService.LoadAssetAsync<AudioClip>("BackgroundMusic");
                
                if (_bgmHandle.IsValid)
                {
                    var audioSource = GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.clip = _bgmHandle.Asset;
                        audioSource.Play();
                        Debug.Log($"Playing BGM: {_bgmHandle.Asset.name}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to load BackgroundMusic: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例 3: Using语句自动释放
        /// </summary>
        private async UniTask Example3_UsingStatement()
        {
            Debug.Log("示例 5: Using语句自动释放");
            
            try
            {
                using var prefabHandle = await _resourceService.LoadAssetAsync<GameObject>("PlayerPrefab");
                
                if (prefabHandle)
                {
                    var instance = Instantiate(prefabHandle.Asset);
                    instance.name = "PlayerInstance";
                    Debug.Log($"Instantiated: {instance.name}");
                    
                    // 销毁实例
                    DestroyImmediate(instance);
                }
                
                // using语句结束时自动释放Handle
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to load PlayerPrefab: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例 4: 批量加载
        /// </summary>
        private async UniTask Example4_BatchLoading()
        {
            Debug.Log("示例 6: 批量加载");
            
            try
            {
                string[] soundNames = { "ButtonClick", "ItemPickup", "Explosion" };
                var soundHandles = await _resourceService.LoadAssetsAsync<AudioClip>(soundNames);
                
                Debug.Log($"Loaded {soundHandles.Length} sound effects:");
                for (int i = 0; i < soundHandles.Length; i++)
                {
                    if (soundHandles[i].IsValid)
                    {
                        Debug.Log($"  - {soundHandles[i].Asset.name}");
                    }
                    
                    // 批量释放
                    soundHandles[i].Dispose();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to batch load sounds: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例 5: 预加载
        /// </summary>
        private async UniTask Example5_Preloading()
        {
            Debug.Log("示例 7: 预加载");
            
            try
            {
                // 预加载资源到缓存
                await _resourceService.PreloadAssetAsync("LevelBackground");
                Debug.Log("Level background preloaded to cache");
                
                // 后续加载会更快
                using var bgHandle = await _resourceService.LoadAssetAsync<Texture2D>("LevelBackground");
                if (bgHandle)
                {
                    Debug.Log($"Quickly loaded cached background: {bgHandle.Asset.name}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to preload: {ex.Message}");
            }
        }

        private void OnDestroy()
        {
            // 手动释放BGM Handle
            _bgmHandle?.Dispose();
            Debug.Log("ResourceUsageExample destroyed, manual handles released");
        }
    }
}