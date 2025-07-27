using Cysharp.Threading.Tasks;
using Game.Infrastructure.Resource.Core;
using Game.Infrastructure.Resource.Configuration;
using System;
using UnityEngine;
using YooAsset;

namespace Game.Infrastructure.Resource.Providers.YooAssetProvider
{
    /// <summary>
    /// 基于YooAsset的资源服务实现
    /// 专注于YooAsset初始化，资源加载通过ResourceLoader完成
    /// </summary>
    public class YooAssetResourceService : IResourceService, IDisposable
    {
        private bool _disposed = false;

        public YooAssetResourceService()
        {
            Debug.Log("YooAssetResourceService 已创建 - 简化版本，专注于初始化");
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

        /// <summary>
        /// 释放资源服务
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            
            _disposed = true;
            Debug.Log("YooAssetResourceService 已释放");
        }
    }
}