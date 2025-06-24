using Cysharp.Threading.Tasks;
using Game.Modules.Resource.Domain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;


namespace Game.Modules.Resource.Infrastructure
{
    public class YooAssetResourceProvider : IResourceService
    {
        private ResourceConfig _config;

        public YooAssetResourceProvider(ResourceConfig config)
        {
            _config = config;
        }

        public async UniTask InitializeAsync()
        {
            YooAssets.Initialize();
            var package = YooAssets.TryGetPackage(_config.PackageName) ?? YooAssets.CreatePackage(_config.PackageName);
            YooAssets.SetDefaultPackage(package);

#if UNITY_EDITOR
            var initParameters = new EditorSimulateModeParameters();
            await package.InitializeAsync(initParameters);
#else
        var initParameters = new HostPlayModeParameters
        {
            BuildinQueryServices = new GameQueryServices(),
            SandboxRootDirectory = Application.persistentDataPath,
            RemoteServices = new GameRemoteServices(_config.RemoteURL)
        };
        await package.InitializeAsync(initParameters);

        if (_config.EnableHotUpdate)
        {
            var updateOp = package.UpdatePackageVersionAsync();
            await updateOp.ToUniTask();
            var manifestOp = package.UpdatePackageManifestAsync(updateOp.PackageVersion);
            await manifestOp.ToUniTask();
            var downloader = package.CreateResourceDownloader(99, 5);
            if (downloader.TotalDownloadCount > 0)
            {
                await downloader.StartDownloadAsync().ToUniTask();
            }
        }
#endif
        }

        public async UniTask<T> LoadAssetAsync<T>(string assetName) where T : UnityEngine.Object
        {
            var handle = YooAssets.LoadAssetAsync<T>(assetName);
            await handle.ToUniTask();
            return handle.AssetObject as T;
        }

        public void load()
        {
            var go = LoadAssetAsync<GameObject>("AAAAAAa");
            Debug.Log(go);
        }

        public void UnloadAsset(Object asset)
        {
            if (asset != null)
                Resources.UnloadAsset(asset);
        }
    }
}

