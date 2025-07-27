using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Infrastructure.Resource.Core;
using UnityEngine;
using VContainer;

namespace Game.Infrastructure.Resource.Examples
{
    /// <summary>
    /// 优化的资源使用示例 - 实现IResourceHandleOwner接口获得最佳性能
    /// </summary>
    public class OptimizedResourceExample : MonoBehaviour, IResourceHandleOwner
    {
        [Inject] private IResourceService _resourceService;
        
        // 实现接口，手动管理Handle列表（最佳性能）
        private readonly List<IResourceHandle> _autoReleaseHandles = new();

        /// <summary>
        /// 实现IResourceHandleOwner接口 - 高性能模式
        /// </summary>
        public void RegisterHandleForAutoRelease(IResourceHandle handle)
        {
            if (handle != null && handle.IsValid)
            {
                _autoReleaseHandles.Add(handle);
                Debug.Log($"OptimizedResourceExample: 使用IResourceHandleOwner接口注册Handle (最佳性能模式)");
            }
        }

        private async void Start()
        {
            await DemonstrateOptimizedUsage();
        }

        private async UniTask DemonstrateOptimizedUsage()
        {
            Debug.Log("=== 优化的资源使用示例（IResourceHandleOwner接口） ===");

            try
            {
                // 这些调用会自动检测到IResourceHandleOwner接口，使用高性能模式
                var iconHandle = await _resourceService.LoadAssetAsync<Texture2D>("PlayerIcon", this);
                var bgHandle = await _resourceService.LoadAssetAsync<Texture2D>("Background", this);
                
                Debug.Log($"成功加载2个资源，全部通过IResourceHandleOwner接口管理");
                Debug.Log($"当前管理的Handle数量: {_autoReleaseHandles.Count}");
                
                // 正常使用资源
                if (iconHandle)
                {
                    Debug.Log($"使用Icon: {iconHandle.Asset.name}");
                }
                
                if (bgHandle)
                {
                    Debug.Log($"使用Background: {bgHandle.Asset.name}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"优化示例执行失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 手动清理所有Handle（也可以让OnDestroy自动处理）
        /// </summary>
        public void CleanupAllHandles()
        {
            var count = _autoReleaseHandles.Count;
            foreach (var handle in _autoReleaseHandles)
            {
                handle?.Dispose();
            }
            _autoReleaseHandles.Clear();
            
            Debug.Log($"OptimizedResourceExample: 手动清理了 {count} 个Handle");
        }

        /// <summary>
        /// 组件销毁时自动清理所有Handle
        /// </summary>
        private void OnDestroy()
        {
            var count = _autoReleaseHandles.Count;
            foreach (var handle in _autoReleaseHandles)
            {
                handle?.Dispose();
            }
            _autoReleaseHandles.Clear();
            
            Debug.Log($"OptimizedResourceExample destroyed: 自动释放了 {count} 个Handle");
        }
    }
}