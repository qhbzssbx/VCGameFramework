using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Game.HotFix.AssetSystem.Core;
using YooAsset;

namespace Game.Infrastructure.AssetSystem.Module
{
    /// <summary>
    /// 资源系统模块（替代旧 ResourceModule）：
    /// - 注册 IAssetRegistry (Singleton)。
    /// - 初始化 YooAsset（沿用现有 Resources/YooAssetConfig.asset 配置）。
    /// 说明：保留初始化逻辑的最小实现，避免大范围入侵。
    /// </summary>
    public class AssetModule : Game.Core.IModuleWithOrder, Game.Core.IAsyncModule
    {
        public int Order => -500;

        public void Configure(IContainerBuilder builder)
        {
            builder.Register<IAssetRegistry, AssetRegistry>(Lifetime.Singleton);
        }

        public async UniTask InitializeAsync(IObjectResolver resolver)
        {
            // 读取旧配置类型（保持类型名与命名空间不变以兼容现有 asset）
            var config = Resources.Load<Game.HotFix.AssetSystem.Configuration.YooAssetConfig>("YooAssetConfig");
            if (config == null)
            {
                Debug.LogWarning("YooAssetConfig not found, using defaults.");
            }

            YooAssets.Initialize();

            var packageName = config != null ? config.packageName : "DefaultPackage";
            var package = YooAssets.TryGetPackage(packageName) ?? YooAssets.CreatePackage(packageName);
            YooAssets.SetDefaultPackage(package);

            // 简化：统一走 Editor/Offline/Host/Web 的初始化（与旧实现一致但去掉细节日志）
            InitializationOperation initOp;
#if UNITY_EDITOR
            var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
            var editorFs = FileSystemParameters.CreateDefaultEditorFileSystemParameters(buildResult.PackageRootDirectory);
            var editorParams = new EditorSimulateModeParameters { EditorFileSystemParameters = editorFs };
            initOp = package.InitializeAsync(editorParams);
#else
            var buildinFs = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            var offlineParams = new OfflinePlayModeParameters { BuildinFileSystemParameters = buildinFs };
            initOp = package.InitializeAsync(offlineParams);
#endif
            await initOp;
            if (initOp.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"YooAsset init failed: {initOp.Error}");
            }
        }
    }
}
