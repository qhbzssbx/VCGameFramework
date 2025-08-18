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
    /// 负责YooAsset的完整初始化和配置管理
    /// </summary>
    public class YooAssetResourceService : IResourceService, IDisposable
    {
        private bool _disposed = false;
        private YooAssetConfig _config;
        private ResourcePackage _package;

        public YooAssetResourceService()
        {
            LoadConfig();
            Debug.Log($"YooAssetResourceService 已创建 - 运行模式: {_config.playMode}");
        }
        
        /// <summary>
        /// 加载配置文件
        /// </summary>
        private void LoadConfig()
        {
            _config = Resources.Load<YooAssetConfig>("YooAssetConfig");
            if (_config == null)
            {
                Debug.LogWarning("未找到YooAssetConfig配置文件，使用默认配置");
                _config = ScriptableObject.CreateInstance<YooAssetConfig>();
            }
        }

        public async UniTask InitializeAsync()
        {
            try
            {
                if (_config.enableLog)
                {
                    Debug.Log("=== YooAsset 初始化开始 ===");
                }
                
                // 初始化YooAsset
                YooAssets.Initialize();
                
                // 创建或获取资源包
                _package = YooAssets.TryGetPackage(_config.packageName) ?? YooAssets.CreatePackage(_config.packageName);
                YooAssets.SetDefaultPackage(_package);
                
                // 根据配置初始化包
                var success = await InitializePackage();
                if (!success)
                {
                    throw new Exception("YooAsset包初始化失败");
                }
                
                // 联机模式下需要更新版本信息
                // if (_config.playMode == EPlayMode.HostPlayMode || _config.playMode == EPlayMode.WebPlayMode)
                // {
                    await UpdatePackageVersion();
                // }
                
                if (_config.enableLog)
                {
                    Debug.Log("=== YooAsset 初始化完成 ===");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"YooAsset初始化失败: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
        
        /// <summary>
        /// 初始化资源包
        /// </summary>
        private async UniTask<bool> InitializePackage()
        {
            InitializationOperation initOperation = null;
            
            switch (_config.playMode)
            {
                case EPlayMode.EditorSimulateMode:
                    initOperation = await InitializeEditorSimulateMode();
                    break;
                    
                case EPlayMode.OfflinePlayMode:
                    initOperation = await InitializeOfflineMode();
                    break;
                    
                case EPlayMode.HostPlayMode:
                    initOperation = await InitializeHostMode();
                    break;
                    
                case EPlayMode.WebPlayMode:
                    initOperation = await InitializeWebMode();
                    break;
                    
                default:
                    Debug.LogError($"不支持的运行模式: {_config.playMode}");
                    return false;
            }

            if (initOperation != null)
            {
                if (initOperation.Status == EOperationStatus.Succeed)
                {
                    if (_config.enableLog)
                    {
                        Debug.Log($"资源包初始化成功 - 模式: {_config.playMode}");
                    }
                    return true;
                }
                else
                {
                    Debug.LogError($"资源包初始化失败 - 模式: {_config.playMode}, 错误: {initOperation.Error}");
                    return false;
                }
            }

            return false;
        }
        
        /// <summary>
        /// 编辑器模拟模式初始化
        /// </summary>
        private async UniTask<InitializationOperation> InitializeEditorSimulateMode()
        {
            var buildResult = EditorSimulateModeHelper.SimulateBuild(_config.packageName);
            var packageRoot = buildResult.PackageRootDirectory;
            var editorFileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            
            var initParameters = new EditorSimulateModeParameters();
            initParameters.EditorFileSystemParameters = editorFileSystemParams;
            
            var initOperation = _package.InitializeAsync(initParameters);
            await initOperation;
            return initOperation;
        }
        
        /// <summary>
        /// 离线模式初始化
        /// </summary>
        private async UniTask<InitializationOperation> InitializeOfflineMode()
        {
            var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            
            var initParameters = new OfflinePlayModeParameters();
            initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
            
            var initOperation = _package.InitializeAsync(initParameters);
            await initOperation;
            return initOperation;
        }
        
        /// <summary>
        /// 联机模式初始化
        /// </summary>
        private async UniTask<InitializationOperation> InitializeHostMode()
        {
            string defaultHostServer = _config.GetPlatformHostServer();
            string fallbackHostServer = _config.fallbackHostServer;
            
            IRemoteServices remoteServices = new GameRemoteServices(defaultHostServer, fallbackHostServer);
            var cacheFileSystemParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
            var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();

            var initParameters = new HostPlayModeParameters();
            initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
            initParameters.CacheFileSystemParameters = cacheFileSystemParams;
            
            var initOperation = _package.InitializeAsync(initParameters);
            await initOperation;
            return initOperation;
        }
        
        /// <summary>
        /// WebGL模式初始化
        /// </summary>
        private async UniTask<InitializationOperation> InitializeWebMode()
        {
            string defaultHostServer = _config.GetPlatformHostServer();
            string fallbackHostServer = _config.fallbackHostServer;
            
            IRemoteServices remoteServices = new GameRemoteServices(defaultHostServer, fallbackHostServer);
            var webServerFileSystemParams = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
            var webRemoteFileSystemParams = FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(remoteServices);

            var initParameters = new WebPlayModeParameters();
            initParameters.WebServerFileSystemParameters = webServerFileSystemParams;
            initParameters.WebRemoteFileSystemParameters = webRemoteFileSystemParams;
            
            var initOperation = _package.InitializeAsync(initParameters);
            await initOperation;
            return initOperation;
        }
        
        /// <summary>
        /// 更新包版本信息
        /// </summary>
        private async UniTask UpdatePackageVersion()
        {
            if (_config.enableLog)
            {
                Debug.Log("开始更新资源包版本信息...");
            }
            
            // 获取资源版本
            var versionOperation = _package.RequestPackageVersionAsync();
            await versionOperation;
            
            if (versionOperation.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"获取资源版本失败: {versionOperation.Error}");
                return;
            }
            
            string packageVersion = versionOperation.PackageVersion;
            if (_config.enableLog)
            {
                Debug.Log($"当前资源版本: {packageVersion}");
            }
            
            // 更新包Manifest文件
            var updateOperation = _package.UpdatePackageManifestAsync(packageVersion);
            await updateOperation;
            
            if (updateOperation.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"更新Manifest失败: {updateOperation.Error}");
                return;
            }
            
            if (_config.enableLog)
            {
                Debug.Log("资源版本信息更新完成");
            }
        }

        /// <summary>
        /// 释放资源服务
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            
            try
            {
                // 清理资源包
                if (_package != null && _config.enableLog)
                {
                    Debug.Log("正在清理YooAsset资源包...");
                }
                
                _disposed = true;
                
                if (_config?.enableLog == true)
                {
                    Debug.Log("YooAssetResourceService 已释放");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"YooAssetResourceService释放时发生错误: {ex.Message}");
            }
        }
    }
}