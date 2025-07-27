using Cysharp.Threading.Tasks;
using Game.Infrastructure.Resource.Core;
using Game.Infrastructure.Resource.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YooAsset;

namespace Game.Infrastructure.Resource.Providers.YooAssetProvider
{
    /// <summary>
    /// 基于YooAsset的资源服务实现
    /// </summary>
    public class YooAssetResourceService : IResourceService, IDisposable
    {
        // 简化的生命周期管理
        private readonly Dictionary<MonoBehaviour, List<IDisposable>> _ownerHandles = new();
        private bool _disposed = false;

        public YooAssetResourceService()
        {
            Debug.Log("YooAssetResourceService 已创建 - Infrastructure重构版本");
        }

        public async UniTask InitializeAsync()
        {
            try
            {
                YooAssets.Initialize();
                var package = YooAssets.TryGetPackage(ResourceConfig.PackageName) ?? YooAssets.CreatePackage(ResourceConfig.PackageName);
                YooAssets.SetDefaultPackage(package);
#if UNITY_EDITOR
                
                var buildResult = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");    
                var packageRoot = buildResult.PackageRootDirectory;
                var editorFileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                var initParameters = new EditorSimulateModeParameters();
                initParameters.EditorFileSystemParameters = editorFileSystemParams;
                var initOperation = package.InitializeAsync(initParameters);
                await initOperation;
    
                if(initOperation.Status == EOperationStatus.Succeed)
                    Debug.Log("资源包初始化成功！");
                else 
                    Debug.LogError($"资源包初始化失败：{initOperation.Error}");
#else
                var initParameters = new HostPlayModeParameters
                {
                    BuildinQueryServices = new GameQueryServices(),
                    SandboxRootDirectory = Application.persistentDataPath,
                    RemoteServices = new GameRemoteServices(ResourceConfig.RemoteURL)
                };
                await package.InitializeAsync(initParameters);

                if (ResourceConfig.EnableHotUpdate)
                {
                    Debug.Log("Starting hot update process...");
                    var updateOp = package.UpdatePackageVersionAsync();
                    await updateOp.ToUniTask();
                    
                    var manifestOp = package.UpdatePackageManifestAsync(updateOp.PackageVersion);
                    await manifestOp.ToUniTask();
                    
                    var downloader = package.CreateResourceDownloader(99, 5);
                    if (downloader.TotalDownloadCount > 0)
                    {
                        Debug.Log($"Downloading {downloader.TotalDownloadCount} files...");
                        await downloader.StartDownloadAsync().ToUniTask();
                    }
                    Debug.Log("Hot update completed");
                }
                Debug.Log("YooAsset initialized in Host Play Mode");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize YooAsset: {ex.Message}");
                throw;
            }
        }

        public async UniTask<ResourceHandle<T>> LoadAssetAsync<T>(string assetName) where T : UnityEngine.Object
        {
            try
            {
                var yooHandle = YooAssets.LoadAssetAsync<T>(assetName);
                await yooHandle.ToUniTask();

                if (yooHandle.AssetObject == null)
                {
                    throw new InvalidOperationException($"Failed to load asset: {assetName}");
                }

                var resourceHandle = new ResourceHandle<T>(yooHandle);
                Debug.Log($"Successfully loaded asset: {assetName}");
                return resourceHandle;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading asset '{assetName}': {ex.Message}");
                throw;
            }
        }

        public async UniTask<ResourceHandle<T>> LoadAssetAsync<T>(string assetName, MonoBehaviour owner) where T : UnityEngine.Object
        {
            var handle = await LoadAssetAsync<T>(assetName);
            
            // 简化的生命周期绑定
            if (owner != null)
            {
                BindToOwner(handle, owner);
            }
            
            return handle;
        }

        public async UniTask<ResourceHandle<T>[]> LoadAssetsAsync<T>(string[] assetNames) where T : UnityEngine.Object
        {
            if (assetNames == null || assetNames.Length == 0)
            {
                return new ResourceHandle<T>[0];
            }

            try
            {
                var tasks = assetNames.Select(name => LoadAssetAsync<T>(name));
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

        public async UniTask PreloadAssetAsync(string assetName)
        {
            try
            {
                var yooHandle = YooAssets.LoadAssetAsync(assetName);
                await yooHandle.ToUniTask();
                
                // 预加载后立即释放Handle，但资源会保留在缓存中
                yooHandle.Release();
                
                Debug.Log($"Successfully preloaded asset: {assetName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error preloading asset '{assetName}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 智能的生命周期绑定逻辑
        /// 优先使用IResourceHandleOwner接口，降级到内置管理
        /// </summary>
        private void BindToOwner(IDisposable handle, MonoBehaviour owner)
        {
            if (_disposed) return;
            
            // 优先检查是否实现了IResourceHandleOwner接口
            if (owner is IResourceHandleOwner handleOwner && handle is IResourceHandle resourceHandle)
            {
                handleOwner.RegisterHandleForAutoRelease(resourceHandle);
                Debug.Log($"使用IResourceHandleOwner接口绑定到 {owner.GetType().Name}（高性能模式）");
                return;
            }
            
            // 降级到内置管理（使用destroyCancellationToken）
            if (!_ownerHandles.ContainsKey(owner))
            {
                _ownerHandles[owner] = new List<IDisposable>();
                
                // 使用destroyCancellationToken监听销毁事件，避免轮询
                var token = owner.destroyCancellationToken;
                token.Register(() => ReleaseOwnerHandles(owner));
                
                Debug.Log($"使用内置管理绑定到 {owner.GetType().Name}（兼容模式）");
            }
            
            _ownerHandles[owner].Add(handle);
        }
        
        /// <summary>
        /// 释放指定Owner的所有Handle
        /// </summary>
        private void ReleaseOwnerHandles(MonoBehaviour owner)
        {
            if (!_ownerHandles.TryGetValue(owner, out var handles)) return;
            
            var count = 0;
            foreach (var handle in handles)
            {
                handle?.Dispose();
                count++;
            }
            
            _ownerHandles.Remove(owner);
            Debug.Log($"自动释放了 {count} 个Handle（Owner: {owner?.GetType().Name}）");
        }
        
        /// <summary>
        /// 释放所有资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            
            foreach (var handles in _ownerHandles.Values)
            {
                foreach (var handle in handles)
                {
                    handle?.Dispose();
                }
            }
            
            _ownerHandles.Clear();
            _disposed = true;
            
            Debug.Log("YooAssetResourceService 已释放所有资源");
        }
    }
}