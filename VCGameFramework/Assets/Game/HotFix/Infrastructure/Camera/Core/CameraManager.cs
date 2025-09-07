using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Game.Infrastructure.Camera.Effects;

namespace Game.Infrastructure.Camera.Core
{
    /// <summary>
    /// 摄像机管理器实现
    /// </summary>
    public class CameraManager : MonoBehaviour, ICameraManager
    {
        public static CameraManager Instance { get; private set; }
        
        [Header("摄像机管理器设置")]
        [SerializeField] private bool autoRegisterMainCamera = true;
        [SerializeField] private Transform cameraRoot;
        
        private readonly Dictionary<CameraType, UnityEngine.Camera> cameras = new();
        private readonly Dictionary<CameraType, CameraConfig> cameraConfigs = new();
        private readonly Dictionary<CameraType, Transform> followTargets = new();
        private readonly Dictionary<CameraType, float> smoothTimes = new();
        private readonly Dictionary<CameraType, Vector3> velocities = new();
        
        // 效果组件
        private readonly Dictionary<CameraType, CameraShake> cameraShakes = new();
        private readonly Dictionary<CameraType, CameraTransition> cameraTransitions = new();
        
        private CameraType currentCameraType = CameraType.Main;
        private bool isTransitioning = false;
        private bool disposed = false;
        
        public UnityEngine.Camera ActiveCamera => GetCamera(currentCameraType);
        
        public event Action<CameraType, CameraType> OnCameraSwitched;
        
        #region Unity生命周期
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Update()
        {
            if (disposed) return;
            
            UpdateFollowTargets();
        }
        
        private void OnDestroy()
        {
            Dispose();
        }
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 初始化摄像机管理器
        /// </summary>
        private void Initialize()
        {
            // 创建摄像机根节点
            if (cameraRoot == null)
            {
                var rootGO = new GameObject("CameraRoot");
                rootGO.transform.SetParent(transform);
                cameraRoot = rootGO.transform;
            }
            
            // 自动注册主摄像机
            if (autoRegisterMainCamera)
            {
                var mainCamera = UnityEngine.Camera.main;
                if (mainCamera != null)
                {
                    var config = new CameraConfig(CameraType.Main);
                    RegisterCamera(mainCamera, config);
                    Debug.Log($"[CameraManager] 自动注册主摄像机: {mainCamera.name}");
                }
            }
        }
        
        #endregion
        
        #region ICameraManager实现
        
        public bool RegisterCamera(UnityEngine.Camera camera, CameraConfig config)
        {
            if (camera == null || config == null)
            {
                Debug.LogWarning("[CameraManager] 注册摄像机失败：摄像机或配置为空");
                return false;
            }
            
            if (cameras.ContainsKey(config.cameraType))
            {
                Debug.LogWarning($"[CameraManager] 摄像机类型 {config.cameraType} 已存在，将覆盖");
                UnregisterCamera(config.cameraType);
            }
            
            cameras[config.cameraType] = camera;
            cameraConfigs[config.cameraType] = config;
            
            // 应用配置
            config.ApplyTo(camera);
            
            // 设置父节点
            if (config.isPersistent && cameraRoot != null)
            {
                camera.transform.SetParent(cameraRoot);
            }
            
            Debug.Log($"[CameraManager] 注册摄像机: {config.cameraType} - {camera.name}");
            return true;
        }
        
        public bool UnregisterCamera(CameraType cameraType)
        {
            if (!cameras.ContainsKey(cameraType))
            {
                return false;
            }
            
            // 清理跟随目标
            RemoveFollowTarget(cameraType);
            
            cameras.Remove(cameraType);
            cameraConfigs.Remove(cameraType);
            
            Debug.Log($"[CameraManager] 注销摄像机: {cameraType}");
            return true;
        }
        
        public UnityEngine.Camera GetCamera(CameraType cameraType)
        {
            cameras.TryGetValue(cameraType, out var camera);
            return camera;
        }
        
        public Dictionary<CameraType, UnityEngine.Camera> GetAllCameras()
        {
            return new Dictionary<CameraType, UnityEngine.Camera>(cameras);
        }
        
        public async UniTask SwitchCamera(CameraType cameraType, bool immediate = false)
        {
            if (!cameras.ContainsKey(cameraType))
            {
                Debug.LogWarning($"[CameraManager] 切换摄像机失败：类型 {cameraType} 不存在");
                return;
            }
            
            if (currentCameraType == cameraType)
            {
                Debug.Log($"[CameraManager] 已经是当前摄像机: {cameraType}");
                return;
            }
            
            if (isTransitioning && !immediate)
            {
                Debug.LogWarning($"[CameraManager] 正在切换摄像机中，忽略切换到 {cameraType}");
                return;
            }
            
            var oldCameraType = currentCameraType;
            var oldCamera = GetCamera(oldCameraType);
            var newCamera = GetCamera(cameraType);
            
            isTransitioning = true;
            
            try
            {
                if (immediate || oldCamera == null)
                {
                    // 立即切换
                    if (oldCamera != null) oldCamera.enabled = false;
                    newCamera.enabled = true;
                }
                else
                {
                    // 平滑切换 (这里可以添加更复杂的过渡效果)
                    newCamera.enabled = true;
                    await UniTask.Delay(100); // 简单的延迟，可以替换为更复杂的过渡动画
                    oldCamera.enabled = false;
                }
                
                currentCameraType = cameraType;
                OnCameraSwitched?.Invoke(oldCameraType, cameraType);
                
                Debug.Log($"[CameraManager] 切换摄像机: {oldCameraType} -> {cameraType}");
            }
            finally
            {
                isTransitioning = false;
            }
        }
        
        public void ActivateCamera(CameraType cameraType)
        {
            var camera = GetCamera(cameraType);
            if (camera != null)
            {
                camera.enabled = true;
                Debug.Log($"[CameraManager] 激活摄像机: {cameraType}");
            }
        }
        
        public void DeactivateCamera(CameraType cameraType)
        {
            var camera = GetCamera(cameraType);
            if (camera != null)
            {
                camera.enabled = false;
                Debug.Log($"[CameraManager] 停用摄像机: {cameraType}");
            }
        }
        
        public bool SetCameraConfig(CameraType cameraType, CameraConfig config)
        {
            if (!cameras.ContainsKey(cameraType) || config == null)
            {
                return false;
            }
            
            cameraConfigs[cameraType] = config;
            config.ApplyTo(cameras[cameraType]);
            
            Debug.Log($"[CameraManager] 更新摄像机配置: {cameraType}");
            return true;
        }
        
        public CameraConfig GetCameraConfig(CameraType cameraType)
        {
            cameraConfigs.TryGetValue(cameraType, out var config);
            return config;
        }
        
        public void ShakeCamera(float duration, float magnitude = 1f, float roughness = 1f, float fadeIn = 0f, float fadeOut = 0f)
        {
            var camera = ActiveCamera;
            if (camera == null) return;
            
            var shake = GetOrCreateCameraShake(currentCameraType);
            if (shake != null)
            {
                shake.StartShake(duration, magnitude, roughness, fadeIn, fadeOut);
                Debug.Log($"[CameraManager] 摄像机震动: 持续{duration}秒，强度{magnitude}");
            }
        }
        
        public void StopCameraShake()
        {
            var shake = GetCameraShake(currentCameraType);
            if (shake != null)
            {
                shake.StopShake();
                Debug.Log("[CameraManager] 停止摄像机震动");
            }
        }
        
        public void SetFollowTarget(CameraType cameraType, Transform target, float smoothTime = 0.3f)
        {
            if (!cameras.ContainsKey(cameraType))
            {
                Debug.LogWarning($"[CameraManager] 设置跟随目标失败：摄像机类型 {cameraType} 不存在");
                return;
            }
            
            followTargets[cameraType] = target;
            smoothTimes[cameraType] = smoothTime;
            velocities[cameraType] = Vector3.zero;
            
            Debug.Log($"[CameraManager] 设置摄像机跟随目标: {cameraType} -> {target.name}");
        }
        
        public void RemoveFollowTarget(CameraType cameraType)
        {
            if (followTargets.Remove(cameraType))
            {
                smoothTimes.Remove(cameraType);
                velocities.Remove(cameraType);
                Debug.Log($"[CameraManager] 移除摄像机跟随目标: {cameraType}");
            }
        }
        
        public UnityEngine.Camera CreateCamera(CameraConfig config, Transform parent = null)
        {
            if (config == null)
            {
                Debug.LogWarning("[CameraManager] 创建摄像机失败：配置为空");
                return null;
            }
            
            var cameraGO = new GameObject($"Camera_{config.cameraType}");
            if (parent != null)
            {
                cameraGO.transform.SetParent(parent);
            }
            else if (cameraRoot != null)
            {
                cameraGO.transform.SetParent(cameraRoot);
            }
            
            var camera = cameraGO.AddComponent<UnityEngine.Camera>();
            config.ApplyTo(camera);
            
            Debug.Log($"[CameraManager] 创建摄像机: {config.cameraType}");
            return camera;
        }
        
        public bool DestroyCamera(CameraType cameraType)
        {
            var camera = GetCamera(cameraType);
            if (camera == null)
            {
                return false;
            }
            
            UnregisterCamera(cameraType);
            
            if (camera.gameObject != null)
            {
                Destroy(camera.gameObject);
            }
            
            Debug.Log($"[CameraManager] 销毁摄像机: {cameraType}");
            return true;
        }
        
        public bool HasCamera(CameraType cameraType)
        {
            return cameras.ContainsKey(cameraType);
        }
        
        public bool IsCameraActive(CameraType cameraType)
        {
            var camera = GetCamera(cameraType);
            return camera != null && camera.enabled;
        }
        
        public void Dispose()
        {
            if (disposed) return;
            
            try
            {
                Debug.Log("[CameraManager] 开始清理资源");
                
                // 清理所有跟随目标
                followTargets.Clear();
                smoothTimes.Clear();
                velocities.Clear();
                
                // 清理摄像机配置
                cameraConfigs.Clear();
                
                // 清理摄像机引用
                cameras.Clear();
                
                // 清理事件
                OnCameraSwitched = null;
                
                disposed = true;
                Debug.Log("[CameraManager] 资源清理完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CameraManager] 清理资源时发生错误: {ex.Message}");
            }
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 更新跟随目标
        /// </summary>
        private void UpdateFollowTargets()
        {
            if (followTargets.Count == 0) return;
            
            foreach (var kvp in followTargets.ToList())
            {
                var cameraType = kvp.Key;
                var target = kvp.Value;
                
                if (target == null)
                {
                    RemoveFollowTarget(cameraType);
                    continue;
                }
                
                var camera = GetCamera(cameraType);
                if (camera == null) continue;
                
                var smoothTime = smoothTimes.GetValueOrDefault(cameraType, 0.3f);
                var velocity = velocities.GetValueOrDefault(cameraType, Vector3.zero);
                
                // 平滑跟随目标
                var targetPosition = target.position;
                var currentPosition = camera.transform.position;
                var newPosition = Vector3.SmoothDamp(currentPosition, targetPosition, ref velocity, smoothTime);
                
                camera.transform.position = newPosition;
                velocities[cameraType] = velocity;
            }
        }
        
        /// <summary>
        /// 获取或创建摄像机震动组件
        /// </summary>
        private CameraShake GetOrCreateCameraShake(CameraType cameraType)
        {
            if (!cameraShakes.TryGetValue(cameraType, out var shake))
            {
                var camera = GetCamera(cameraType);
                if (camera != null)
                {
                    shake = CameraShake.AddTo(camera.gameObject);
                    if (shake != null)
                    {
                        cameraShakes[cameraType] = shake;
                    }
                }
            }
            return shake;
        }
        
        /// <summary>
        /// 获取摄像机震动组件
        /// </summary>
        private CameraShake GetCameraShake(CameraType cameraType)
        {
            cameraShakes.TryGetValue(cameraType, out var shake);
            return shake;
        }
        
        /// <summary>
        /// 获取或创建摄像机过渡组件
        /// </summary>
        private CameraTransition GetOrCreateCameraTransition(CameraType cameraType)
        {
            if (!cameraTransitions.TryGetValue(cameraType, out var transition))
            {
                var camera = GetCamera(cameraType);
                if (camera != null)
                {
                    transition = CameraTransition.AddTo(camera.gameObject);
                    if (transition != null)
                    {
                        cameraTransitions[cameraType] = transition;
                    }
                }
            }
            return transition;
        }
        
        /// <summary>
        /// 获取摄像机过渡组件
        /// </summary>
        private CameraTransition GetCameraTransition(CameraType cameraType)
        {
            cameraTransitions.TryGetValue(cameraType, out var transition);
            return transition;
        }
        
        #endregion
    }
}