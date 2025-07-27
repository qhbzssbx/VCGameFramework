using Cysharp.Threading.Tasks;
using Game.Infrastructure.Resource.Core;
using Game.Infrastructure.Resource.Extensions;
using UnityEngine;

namespace Game.Infrastructure.Resource.Examples
{
    /// <summary>
    /// 新资源系统基础使用示例
    /// 展示ResourceLoader的三种主要使用模式
    /// </summary>
    public class BasicUsageExample : MonoBehaviour
    {
        [Header("测试资源名称")]
        [SerializeField] private string textureAssetName = "PlayerIcon";
        [SerializeField] private string prefabAssetName = "PlayerPrefab";
        [SerializeField] private string audioAssetName = "BackgroundMusic";

        private ResourceLoader _resourceLoader = new();

        private async void Start()
        {
            Debug.Log("=== 新资源系统基础使用示例 ===");
            
            await DemonstrateThreeUsageModes();
            
            Debug.Log("=== 基础示例完成 ===");
        }

        private async UniTask DemonstrateThreeUsageModes()
        {
            // 模式1：手动管理 - 由ResourceLoader统一管理
            await Mode1_ManualManagement();
            
            await UniTask.Delay(1000);
            
            // 模式2：自动释放 - 绑定到GameObject生命周期
            await Mode2_AutoRelease();
            
            await UniTask.Delay(1000);
            
            // 模式3：临时使用 - using语句自动释放
            await Mode3_TemporaryUsage();
        }

        /// <summary>
        /// 模式1：手动管理（适用于全局共享资源）
        /// </summary>
        private async UniTask Mode1_ManualManagement()
        {
            Debug.Log("--- 模式1：手动管理（ResourceLoader统一管理）---");
            
            try
            {
                // 资源由ResourceLoader统一管理，适用于Manager类的全局资源
                var audioHandle = await _resourceLoader.LoadAssetAsync<AudioClip>(audioAssetName);
                
                if (audioHandle.IsValid && audioHandle.AssetObject != null)
                {
                    var audioClip = audioHandle.AssetObject as AudioClip;
                    Debug.Log($"✅ 手动管理模式加载音频: {audioClip.name}");
                    
                    // 可以正常使用资源...
                    var audioSource = GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.clip = audioClip;
                        Debug.Log("🎵 音频已设置到AudioSource");
                    }
                }
                
                // 资源会在_resourceLoader.Dispose()时统一释放
                Debug.Log($"📊 当前ResourceLoader管理的Handle数量: {_resourceLoader.HandleCount}");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"❌ 模式1加载失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 模式2：自动释放（适用于UI组件、特定GameObject相关资源）
        /// </summary>
        private async UniTask Mode2_AutoRelease()
        {
            Debug.Log("--- 模式2：自动释放（绑定到GameObject生命周期）---");
            
            try
            {
                // 创建一个临时GameObject来演示自动释放
                var tempGameObject = new GameObject("TempResourceHolder");
                
                // 资源绑定到这个GameObject，当GameObject销毁时自动释放
                var textureHandle = await _resourceLoader.LoadAssetAsync<Texture2D>(textureAssetName, tempGameObject);
                
                if (textureHandle.IsValid && textureHandle.AssetObject != null)
                {
                    var texture = textureHandle.AssetObject as Texture2D;
                    Debug.Log($"✅ 自动释放模式加载纹理: {texture.name}");
                    Debug.Log($"📐 纹理尺寸: {texture.width}x{texture.height}");
                }
                
                // 检查自动释放组件
                var autoRelease = tempGameObject.GetComponent<AutoResourceRelease>();
                if (autoRelease != null)
                {
                    Debug.Log($"📋 GameObject上管理的Handle数量: {autoRelease.HandleCount}");
                }
                
                Debug.Log("⏰ 3秒后销毁GameObject，观察自动释放...");
                await UniTask.Delay(3000);
                
                // 销毁GameObject，观察自动释放
                DestroyImmediate(tempGameObject);
                Debug.Log("🗑️ GameObject已销毁，资源应该自动释放");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"❌ 模式2加载失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 模式3：临时使用（适用于短期使用的资源）
        /// </summary>
        private async UniTask Mode3_TemporaryUsage()
        {
            Debug.Log("--- 模式3：临时使用（using语句自动释放）---");

            try
            {
                // 创建一个新的ResourceLoader用于演示using语句
                using var tempLoader = new ResourceLoader();

                var prefabHandle = await tempLoader.LoadPrefabForInstantiate<GameObject>(prefabAssetName);

                if (prefabHandle.IsValid && prefabHandle.AssetObject != null)
                {
                    var prefab = prefabHandle.AssetObject as GameObject;
                    Debug.Log($"✅ 临时使用模式加载Prefab: {prefab.name}");

                    // 实例化Prefab
                    var instance = Instantiate(prefab);
                    instance.name = "TempInstance";
                    Debug.Log($"🎮 实例化GameObject: {instance.name}");

                    // 使用完毕，清理实例
                    DestroyImmediate(instance);
                    Debug.Log("🗑️ 实例已清理");
                }

                Debug.Log($"📊 临时ResourceLoader管理的Handle数量: {tempLoader.HandleCount}");

                // using语句结束时，tempLoader会自动Dispose，释放所有Handle
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"<UNK> <UNK>3<UNK>: {ex.Message}");
            }
            
            Debug.Log("✨ using语句结束，临时ResourceLoader已自动释放");
        }

        /// <summary>
        /// 演示扩展方法的使用
        /// </summary>
        private void DemonstrateExtensions()
        {
            Debug.Log("--- 扩展方法演示 ---");
            
            // 使用扩展方法检查资源管理状态
            var handleCount = this.gameObject.GetManagedHandleCount();
            Debug.Log($"📊 当前GameObject管理的Handle数量: {handleCount}");
            
            // 检查是否有自动释放组件
            var hasAutoRelease = this.gameObject.HasAutoResourceRelease();
            Debug.Log($"🔧 是否有AutoResourceRelease组件: {hasAutoRelease}");
            
            // 手动释放所有资源（如果有的话）
            var released = this.gameObject.ReleaseAllResources();
            Debug.Log($"🗑️ 手动释放结果: {released}");
        }

        /// <summary>
        /// MonoBehaviour销毁时释放ResourceLoader
        /// </summary>
        private void OnDestroy()
        {
            Debug.Log($"🔄 BasicUsageExample销毁，释放ResourceLoader (管理的Handle数量: {_resourceLoader?.HandleCount ?? 0})");
            _resourceLoader?.Dispose();
        }

        /// <summary>
        /// 在Inspector中提供测试按钮
        /// </summary>
        [ContextMenu("演示扩展方法")]
        private void TestExtensions()
        {
            DemonstrateExtensions();
        }

        [ContextMenu("显示资源状态")]
        private void ShowResourceStatus()
        {
            Debug.Log($"=== 资源状态 ===");
            Debug.Log($"ResourceLoader Handle数量: {_resourceLoader?.HandleCount ?? 0}");
            Debug.Log($"GameObject Handle数量: {this.gameObject.GetManagedHandleCount()}");
            Debug.Log($"是否已释放: {_resourceLoader?.IsDisposed ?? true}");
        }
    }
}