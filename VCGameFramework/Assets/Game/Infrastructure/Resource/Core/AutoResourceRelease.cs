using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace Game.Infrastructure.Resource.Core
{
    /// <summary>
    /// 自动资源释放组件
    /// 附加到GameObject上，在OnDestroy时自动释放所有注册的AssetHandle
    /// 借鉴GameFramework-Next的AssetsReference设计理念
    /// </summary>
    public class AutoResourceRelease : MonoBehaviour
    {
        private readonly List<AssetHandle> _handles = new();

        /// <summary>
        /// 注册需要自动释放的AssetHandle
        /// </summary>
        /// <param name="handle">YooAsset的原生AssetHandle</param>
        public void Register(AssetHandle handle)
        {
            if (handle != null && handle.IsValid)
            {
                _handles.Add(handle);
                Debug.Log($"AutoResourceRelease: 注册Handle到 {gameObject.name} (当前总数: {_handles.Count})");
            }
        }

        /// <summary>
        /// 手动释放所有Handle（也可以等待OnDestroy自动调用）
        /// </summary>
        public void ReleaseAll()
        {
            var count = 0;
            foreach (var handle in _handles)
            {
                if (handle != null && handle.IsValid)
                {
                    handle.Release();
                    count++;
                }
            }
            
            _handles.Clear();
            Debug.Log($"AutoResourceRelease: 手动释放了 {count} 个Handle (GameObject: {gameObject.name})");
        }

        /// <summary>
        /// GameObject销毁时自动释放所有资源
        /// </summary>
        private void OnDestroy()
        {
            var count = 0;
            foreach (var handle in _handles)
            {
                if (handle != null && handle.IsValid)
                {
                    handle.Release();
                    count++;
                }
            }
            
            _handles.Clear();
            
            if (count > 0)
            {
                Debug.Log($"AutoResourceRelease: GameObject销毁时自动释放了 {count} 个Handle (GameObject: {gameObject.name})");
            }
        }

        /// <summary>
        /// 获取当前管理的Handle数量（用于调试）
        /// </summary>
        public int HandleCount => _handles.Count;

        /// <summary>
        /// 检查是否有有效的Handle（用于调试）
        /// </summary>
        public bool HasValidHandles
        {
            get
            {
                foreach (var handle in _handles)
                {
                    if (handle != null && handle.IsValid)
                        return true;
                }
                return false;
            }
        }
    }
}