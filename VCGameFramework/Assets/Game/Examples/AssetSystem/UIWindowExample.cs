using System.Threading;
using Cysharp.Threading.Tasks;
using Game.HotFix.AssetSystem.Core;
using UnityEngine;
using VContainer;

namespace Game.HotFix.AssetSystem.Examples
{
    /// <summary>
    /// UI窗口使用示例：展示如何在UI窗口中使用AssetScope进行资源管理
    /// </summary>
    public sealed class UIWindowExample : MonoBehaviour
    {
        [Inject] private IAssetScope _assetScope;
        [Inject] private CancellationToken _scopeCancellationToken;
        
        private GameObject _backgroundImage;
        private GameObject _titleImage;
        
        public async UniTask OpenWindowAsync()
        {
            Debug.Log("[UIWindowExample] Opening window...");
            
            try
            {
                // Pin 窗口背景图片 - 生命周期与窗口一致
                var backgroundPrefab = await _assetScope.PinAsync<GameObject>(
                    "ui", "common/WindowBackground", _scopeCancellationToken);
                _backgroundImage = Instantiate(backgroundPrefab, transform);
                
                // Pin 标题图片 - 生命周期与窗口一致  
                var titlePrefab = await _assetScope.PinAsync<GameObject>(
                    "ui", "common/WindowTitle", _scopeCancellationToken);
                _titleImage = Instantiate(titlePrefab, transform);
                
                // Lease 一次性配置数据 - 使用后自动释放
                await using (await _assetScope.LeaseAsync<TextAsset>(
                    "config", "ui/WindowConfig", _scopeCancellationToken,
                    config => Debug.Log($"Window config loaded: {config.text}")))
                {
                    // 配置数据在这个作用域内可用
                    // 离开作用域时自动释放
                }
                
                Debug.Log("[UIWindowExample] Window opened successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[UIWindowExample] Failed to open window: {ex.Message}");
                throw;
            }
        }
        
        public void CloseWindow()
        {
            Debug.Log("[UIWindowExample] Closing window...");
            
            // 销毁实例化的对象
            if (_backgroundImage != null)
            {
                Destroy(_backgroundImage);
                _backgroundImage = null;
            }
            
            if (_titleImage != null)
            {
                Destroy(_titleImage);
                _titleImage = null;
            }
            
            // 资源会在Scope销毁时自动释放，无需手动管理
            Debug.Log("[UIWindowExample] Window closed");
        }
        
        private void OnDestroy()
        {
            CloseWindow();
            // AssetScope会在LifetimeScope销毁时自动Dispose并释放所有Pin的资源
        }
    }
}