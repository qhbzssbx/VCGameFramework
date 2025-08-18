using UnityEngine;
using VContainer;
using Game.Infrastructure.Camera.Core;
using Game.Core;

namespace Game.Infrastructure.Camera.Module
{
    /// <summary>
    /// 摄像机系统模块
    /// 负责摄像机管理系统的依赖注入和初始化
    /// </summary>
    public class CameraModule : IModule
    {
        public int Order => 50; // 在基础设施模块之后，游戏逻辑模块之前
        
        [SerializeField] private GameObject cameraManagerPrefab;
        [SerializeField] private bool createManagerIfNotExists = true;
        
        public void Configure(IContainerBuilder builder)
        {
            // 注册摄像机管理器接口
            builder.Register<ICameraManager>(resolver =>
            {
                // 尝试获取现有的CameraManager实例
                var existingManager = CameraManager.Instance;
                if (existingManager != null)
                {
                    return existingManager;
                }
                
                // 创建新的CameraManager
                return CreateCameraManager();
            }, Lifetime.Singleton);
            
            Debug.Log("[CameraModule] 摄像机模块配置完成");
        }
        
        /// <summary>
        /// 创建摄像机管理器
        /// </summary>
        private ICameraManager CreateCameraManager()
        {
            GameObject managerGO = null;
            
            // 尝试从预制体创建
            if (cameraManagerPrefab != null)
            {
                managerGO = Object.Instantiate(cameraManagerPrefab);
                managerGO.name = "CameraManager";
            }
            else if (createManagerIfNotExists)
            {
                // 创建空的GameObject并添加CameraManager组件
                managerGO = new GameObject("CameraManager");
                managerGO.AddComponent<CameraManager>();
            }
            
            if (managerGO != null)
            {
                Object.DontDestroyOnLoad(managerGO);
                var manager = managerGO.GetComponent<CameraManager>();
                
                if (manager != null)
                {
                    Debug.Log("[CameraModule] 创建摄像机管理器成功");
                    return manager;
                }
            }
            
            Debug.LogError("[CameraModule] 创建摄像机管理器失败");
            return null;
        }
    }
}