using UnityEngine;
using Game.Infrastructure.Camera.Core;
using CameraType = Game.Infrastructure.Camera.Core.CameraType;

namespace Game.Infrastructure.Camera.Controllers
{
    /// <summary>
    /// 摄像机控制器基类
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public abstract class BaseCameraController : MonoBehaviour
    {
        [Header("基本设置")]
        [SerializeField] protected CameraType cameraType = CameraType.Main;
        [SerializeField] protected CameraConfig cameraConfig;
        [SerializeField] protected bool autoRegister = true;
        
        protected UnityEngine.Camera controlledCamera;
        protected ICameraManager cameraManager;
        protected bool isInitialized = false;
        
        #region Unity生命周期
        
        protected virtual void Awake()
        {
            controlledCamera = GetComponent<UnityEngine.Camera>();
            
            // 如果没有配置，创建默认配置
            if (cameraConfig == null)
            {
                cameraConfig = new CameraConfig(cameraType);
            }
        }
        
        protected virtual void Start()
        {
            Initialize();
        }
        
        protected virtual void Update()
        {
            if (isInitialized)
            {
                OnUpdate();
            }
        }
        
        protected virtual void LateUpdate()
        {
            if (isInitialized)
            {
                OnLateUpdate();
            }
        }
        
        protected virtual void OnDestroy()
        {
            Cleanup();
        }
        
        #endregion
        
        #region 初始化和清理
        
        /// <summary>
        /// 初始化摄像机控制器
        /// </summary>
        protected virtual void Initialize()
        {
            // 获取摄像机管理器
            cameraManager = CameraManager.Instance;
            if (cameraManager == null)
            {
                Debug.LogWarning($"[{GetType().Name}] 未找到CameraManager实例");
                return;
            }
            
            // 应用配置到摄像机
            if (cameraConfig != null)
            {
                cameraConfig.ApplyTo(controlledCamera);
            }
            
            // 自动注册到管理器
            if (autoRegister)
            {
                RegisterToManager();
            }
            
            // 执行子类特定的初始化
            OnInitialize();
            
            isInitialized = true;
            Debug.Log($"[{GetType().Name}] 初始化完成: {cameraType}");
        }
        
        /// <summary>
        /// 注册到摄像机管理器
        /// </summary>
        public virtual void RegisterToManager()
        {
            if (cameraManager != null && controlledCamera != null && cameraConfig != null)
            {
                cameraManager.RegisterCamera(controlledCamera, cameraConfig);
            }
        }
        
        /// <summary>
        /// 从摄像机管理器注销
        /// </summary>
        public virtual void UnregisterFromManager()
        {
            if (cameraManager != null)
            {
                cameraManager.UnregisterCamera(cameraType);
            }
        }
        
        /// <summary>
        /// 清理资源
        /// </summary>
        protected virtual void Cleanup()
        {
            OnCleanup();
            
            if (autoRegister)
            {
                UnregisterFromManager();
            }
            
            isInitialized = false;
        }
        
        #endregion
        
        #region 公共接口
        
        /// <summary>
        /// 获取摄像机类型
        /// </summary>
        public CameraType CameraType => cameraType;
        
        /// <summary>
        /// 获取受控摄像机
        /// </summary>
        public UnityEngine.Camera ControlledCamera => controlledCamera;
        
        /// <summary>
        /// 获取摄像机配置
        /// </summary>
        public CameraConfig Config => cameraConfig;
        
        /// <summary>
        /// 设置摄像机配置
        /// </summary>
        /// <param name="newConfig">新的配置</param>
        public virtual void SetConfig(CameraConfig newConfig)
        {
            if (newConfig == null) return;
            
            cameraConfig = newConfig;
            
            if (controlledCamera != null)
            {
                cameraConfig.ApplyTo(controlledCamera);
            }
            
            if (cameraManager != null)
            {
                cameraManager.SetCameraConfig(cameraType, cameraConfig);
            }
        }
        
        /// <summary>
        /// 激活摄像机
        /// </summary>
        public virtual void Activate()
        {
            if (controlledCamera != null)
            {
                controlledCamera.enabled = true;
                OnActivate();
            }
        }
        
        /// <summary>
        /// 停用摄像机
        /// </summary>
        public virtual void Deactivate()
        {
            if (controlledCamera != null)
            {
                controlledCamera.enabled = false;
                OnDeactivate();
            }
        }
        
        /// <summary>
        /// 检查摄像机是否激活
        /// </summary>
        public virtual bool IsActive()
        {
            return controlledCamera != null && controlledCamera.enabled;
        }
        
        #endregion
        
        #region 受保护的虚方法 - 供子类重写
        
        /// <summary>
        /// 子类特定的初始化逻辑
        /// </summary>
        protected virtual void OnInitialize() { }
        
        /// <summary>
        /// 每帧更新逻辑
        /// </summary>
        protected virtual void OnUpdate() { }
        
        /// <summary>
        /// 每帧后期更新逻辑
        /// </summary>
        protected virtual void OnLateUpdate() { }
        
        /// <summary>
        /// 摄像机激活时的逻辑
        /// </summary>
        protected virtual void OnActivate() { }
        
        /// <summary>
        /// 摄像机停用时的逻辑
        /// </summary>
        protected virtual void OnDeactivate() { }
        
        /// <summary>
        /// 清理资源时的逻辑
        /// </summary>
        protected virtual void OnCleanup() { }
        
        #endregion
        
        #region Inspector工具方法
        
        [ContextMenu("重新应用配置")]
        protected void ReapplyConfig()
        {
            if (cameraConfig != null && controlledCamera != null)
            {
                cameraConfig.ApplyTo(controlledCamera);
                Debug.Log($"[{GetType().Name}] 重新应用配置完成");
            }
        }
        
        [ContextMenu("注册到管理器")]
        protected void ForceRegister()
        {
            RegisterToManager();
        }
        
        [ContextMenu("从管理器注销")]
        protected void ForceUnregister()
        {
            UnregisterFromManager();
        }
        
        #endregion
    }
}