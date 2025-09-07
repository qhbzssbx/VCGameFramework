using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Infrastructure.Resource.Extensions;
using UnityEngine;
using YooAsset;

namespace Game.Infrastructure.Resource.Core
{
    /// <summary>
    /// 资源加载器 - 组合式设计，支持智能的生命周期管理
    /// 借鉴GameFramework-Next的优秀设计理念，实现角色分离
    /// </summary>
    public class ResourceLoader : IDisposable
    {
        private readonly List<AssetHandle> _handles = new();
        private bool _disposed = false;

        /// <summary>
        /// 通用资源加载方法
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="assetName">资源名称</param>
        /// <param name="autoReleaseTarget">自动释放目标GameObject（可选）</param>
        /// <returns>YooAsset原生AssetHandle</returns>
        public async UniTask<AssetHandle> LoadAssetAsync<T>(string assetName, GameObject autoReleaseTarget = null) 
            where T : UnityEngine.Object
        {
            ThrowIfDisposed();
            
            try
            {
                var handle = YooAssets.LoadAssetAsync<T>(assetName);
                await handle;

                if (handle.AssetObject == null)
                {
                    throw new InvalidOperationException($"Failed to load asset: {assetName}");
                }

                // 智能生命周期管理
                if (autoReleaseTarget != null)
                {
                    // 绑定到指定GameObject，GameObject销毁时自动释放
                    var autoRelease = autoReleaseTarget.GetOrAddComponent<AutoResourceRelease>();
                    autoRelease.Register(handle);
                    Debug.Log($"资源 {assetName} 绑定到 {autoReleaseTarget.name} 的生命周期");
                }
                else
                {
                    // 加入ResourceLoader统一管理
                    _handles.Add(handle);
                    Debug.Log($"资源 {assetName} 加入ResourceLoader统一管理");
                }

                return handle;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading asset '{assetName}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 专门用于Prefab资源的加载方法
        /// Prefab资源建议由Manager统一管理，不绑定到实例GameObject
        /// 这样可以安全地进行Instantiate和Clone操作
        /// </summary>
        /// <typeparam name="T">资源类型（通常是GameObject）</typeparam>
        /// <param name="assetName">Prefab资源名称</param>
        /// <returns>YooAsset原生AssetHandle</returns>
        public async UniTask<AssetHandle> LoadPrefabForInstantiate<T>(string assetName) 
            where T : UnityEngine.Object
        {
            ThrowIfDisposed();
            
            try
            {
                var handle = YooAssets.LoadAssetAsync<T>(assetName);
                await handle;

                if (handle.AssetObject == null)
                {
                    throw new InvalidOperationException($"Failed to load prefab: {assetName}");
                }

                // Prefab资源始终由ResourceLoader统一管理
                _handles.Add(handle);
                Debug.Log($"Prefab资源 {assetName} 加载完成，由ResourceLoader统一管理，可安全进行实例化");

                return handle;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading prefab '{assetName}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 批量加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="assetNames">资源名称数组</param>
        /// <param name="autoReleaseTarget">自动释放目标GameObject（可选）</param>
        /// <returns>AssetHandle数组</returns>
        public async UniTask<AssetHandle[]> LoadAssetsAsync<T>(string[] assetNames, GameObject autoReleaseTarget = null) 
            where T : UnityEngine.Object
        {
            if (assetNames == null || assetNames.Length == 0)
            {
                return new AssetHandle[0];
            }

            try
            {
                var tasks = assetNames.Select(name => LoadAssetAsync<T>(name, autoReleaseTarget));
                var handles = await UniTask.WhenAll(tasks);
                
                Debug.Log($"Successfully loaded {handles.Length} assets");
                return handles;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in batch loading assets: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 预加载资源（加载后立即释放Handle，但资源保留在YooAsset缓存中）
        /// </summary>
        /// <param name="assetName">资源名称</param>
        public async UniTask PreloadAssetAsync(string assetName)
        {
            ThrowIfDisposed();
            
            try
            {
                var handle = YooAssets.LoadAssetAsync(assetName);
                await handle;
                
                // 预加载后立即释放Handle，但资源会保留在YooAsset的缓存中
                handle.Release();
                
                Debug.Log($"Successfully preloaded asset: {assetName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error preloading asset '{assetName}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 手动释放所有由ResourceLoader管理的资源
        /// </summary>
        public void ReleaseAll()
        {
            if (_disposed) return;
            
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
            Debug.Log($"ResourceLoader: 手动释放了 {count} 个Handle");
        }

        /// <summary>
        /// 释放ResourceLoader及其管理的所有资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            
            var count = _handles.Count;
            foreach (var handle in _handles)
            {
                handle?.Release();
            }
            
            _handles.Clear();
            _disposed = true;
            
            Debug.Log($"ResourceLoader已释放，共释放了 {count} 个Handle");
        }

        /// <summary>
        /// 获取当前管理的Handle数量（用于调试）
        /// </summary>
        public int HandleCount => _handles.Count;

        /// <summary>
        /// 检查是否已释放
        /// </summary>
        public bool IsDisposed => _disposed;

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ResourceLoader));
            }
        }
    }
}