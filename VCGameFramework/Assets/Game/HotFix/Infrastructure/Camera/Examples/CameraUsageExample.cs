using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;
using Game.Infrastructure.Camera.Core;
using Game.Infrastructure.Camera.Controllers;
using Game.Infrastructure.Camera.Effects;
using CameraType = Game.Infrastructure.Camera.Core.CameraType;

namespace Game.Infrastructure.Camera.Examples
{
    /// <summary>
    /// 摄像机系统使用示例
    /// 演示如何使用摄像机管理系统的各种功能
    /// </summary>
    public class CameraUsageExample : MonoBehaviour
    {
        [Header("示例配置")]
        [SerializeField] private Transform followTarget;
        [SerializeField] private Transform[] cameraPositions;
        [SerializeField] private KeyCode switchCameraKey = KeyCode.C;
        [SerializeField] private KeyCode shakeKey = KeyCode.X;
        [SerializeField] private KeyCode transitionKey = KeyCode.V;
        
        [Inject] private ICameraManager cameraManager;
        
        private int currentPositionIndex = 0;
        
        private void Start()
        {
            InitializeExample();
        }
        
        private void Update()
        {
            HandleInput();
        }
        
        /// <summary>
        /// 初始化示例
        /// </summary>
        private void InitializeExample()
        {
            Debug.Log("[CameraUsageExample] === 摄像机系统使用示例 ===");
            
            // 等待摄像机管理器初始化
            if (cameraManager == null)
            {
                Debug.LogWarning("[CameraUsageExample] 摄像机管理器未注入");
                return;
            }
            
            // 展示基本信息
            ShowBasicInfo();
            
            // 演示摄像机创建
            DemoCreateCameras();
            
            // 演示摄像机切换
            DemoSwitchCameras();
            
            // 演示跟随功能
            if (followTarget != null)
            {
                DemoFollowTarget();
            }
        }
        
        /// <summary>
        /// 显示基本信息
        /// </summary>
        private void ShowBasicInfo()
        {
            var allCameras = cameraManager.GetAllCameras();
            Debug.Log($"[CameraUsageExample] 当前注册的摄像机数量: {allCameras.Count}");
            
            foreach (var kvp in allCameras)
            {
                var isActive = cameraManager.IsCameraActive(kvp.Key) ? "激活" : "未激活";
                Debug.Log($"[CameraUsageExample] - {kvp.Key}: {kvp.Value.name} ({isActive})");
            }
            
            var activeCamera = cameraManager.ActiveCamera;
            if (activeCamera != null)
            {
                Debug.Log($"[CameraUsageExample] 当前活动摄像机: {activeCamera.name}");
            }
        }
        
        /// <summary>
        /// 演示创建摄像机
        /// </summary>
        private void DemoCreateCameras()
        {
            Debug.Log("[CameraUsageExample] === 创建摄像机示例 ===");
            
            // 创建特效摄像机
            if (!cameraManager.HasCamera(CameraType.Effect))
            {
                var effectConfig = new CameraConfig(CameraType.Effect)
                {
                    position = new Vector3(0, 10, -10),
                    rotation = new Vector3(15, 0, 0)
                };
                
                var effectCamera = cameraManager.CreateCamera(effectConfig);
                if (effectCamera != null)
                {
                    Debug.Log("[CameraUsageExample] 创建特效摄像机成功");
                }
            }
            
            // 创建小地图摄像机
            if (!cameraManager.HasCamera(CameraType.Minimap))
            {
                var minimapConfig = new CameraConfig(CameraType.Minimap)
                {
                    position = new Vector3(0, 50, 0),
                    rotation = new Vector3(90, 0, 0),
                    orthographic = true,
                    orthographicSize = 20f,
                    viewportRect = new Rect(0.7f, 0.7f, 0.3f, 0.3f)
                };
                
                var minimapCamera = cameraManager.CreateCamera(minimapConfig);
                if (minimapCamera != null)
                {
                    cameraManager.ActivateCamera(CameraType.Minimap);
                    Debug.Log("[CameraUsageExample] 创建小地图摄像机成功");
                }
            }
        }
        
        /// <summary>
        /// 演示摄像机切换
        /// </summary>
        private async void DemoSwitchCameras()
        {
            Debug.Log("[CameraUsageExample] === 摄像机切换示例 ===");
            
            await UniTask.Delay(2000);
            
            // 切换到特效摄像机
            if (cameraManager.HasCamera(CameraType.Effect))
            {
                Debug.Log("[CameraUsageExample] 切换到特效摄像机");
                await cameraManager.SwitchCamera(CameraType.Effect, false);
                
                await UniTask.Delay(3000);
                
                // 切换回主摄像机
                Debug.Log("[CameraUsageExample] 切换回主摄像机");
                await cameraManager.SwitchCamera(CameraType.Main, false);
            }
        }
        
        /// <summary>
        /// 演示跟随目标
        /// </summary>
        private void DemoFollowTarget()
        {
            Debug.Log("[CameraUsageExample] === 跟随目标示例 ===");
            
            // 设置主摄像机跟随目标
            cameraManager.SetFollowTarget(CameraType.Main, followTarget, 0.5f);
            Debug.Log($"[CameraUsageExample] 设置主摄像机跟随目标: {followTarget.name}");
            
            // 如果有主摄像机控制器，也可以直接设置
            var mainCamera = cameraManager.GetCamera(CameraType.Main);
            if (mainCamera != null)
            {
                var controller = mainCamera.GetComponent<MainCameraController>();
                if (controller != null)
                {
                    controller.SetFollowTarget(followTarget, new Vector3(0, 5, -10));
                    Debug.Log("[CameraUsageExample] 通过控制器设置跟随目标");
                }
            }
        }
        
        /// <summary>
        /// 演示摄像机震动
        /// </summary>
        private void DemoShakeCamera()
        {
            Debug.Log("[CameraUsageExample] === 摄像机震动示例 ===");
            
            // 轻微震动
            cameraManager.ShakeCamera(0.5f, 0.2f, 10f);
            Debug.Log("[CameraUsageExample] 执行轻微震动");
        }
        
        /// <summary>
        /// 演示摄像机过渡
        /// </summary>
        private async void DemoTransitionCamera()
        {
            Debug.Log("[CameraUsageExample] === 摄像机过渡示例 ===");
            
            if (cameraPositions == null || cameraPositions.Length == 0)
            {
                Debug.LogWarning("[CameraUsageExample] 没有设置过渡位置");
                return;
            }
            
            var targetPos = cameraPositions[currentPositionIndex];
            currentPositionIndex = (currentPositionIndex + 1) % cameraPositions.Length;
            
            var mainCamera = cameraManager.GetCamera(CameraType.Main);
            if (mainCamera != null)
            {
                var transition = CameraTransition.AddTo(mainCamera.gameObject);
                if (transition != null)
                {
                    await transition.TransitionTo(
                        targetPos.position, 
                        targetPos.eulerAngles, 
                        2f
                    );
                    Debug.Log($"[CameraUsageExample] 过渡到位置: {targetPos.name}");
                }
            }
        }
        
        /// <summary>
        /// 演示带淡入淡出的摄像机过渡
        /// </summary>
        private async void DemoFadeTransition()
        {
            Debug.Log("[CameraUsageExample] === 淡入淡出过渡示例 ===");
            
            if (cameraPositions == null || cameraPositions.Length == 0) return;
            
            var targetPos = cameraPositions[currentPositionIndex];
            currentPositionIndex = (currentPositionIndex + 1) % cameraPositions.Length;
            
            var mainCamera = cameraManager.GetCamera(CameraType.Main);
            if (mainCamera != null)
            {
                var transition = CameraTransition.AddTo(mainCamera.gameObject);
                if (transition != null)
                {
                    await transition.TransitionWithFade(
                        targetPos.position,
                        targetPos.eulerAngles,
                        2f,
                        Color.black
                    );
                    Debug.Log($"[CameraUsageExample] 淡入淡出过渡到位置: {targetPos.name}");
                }
            }
        }
        
        /// <summary>
        /// 处理输入
        /// </summary>
        private void HandleInput()
        {
            if (Input.GetKeyDown(switchCameraKey))
            {
                SwitchToNextCamera();
            }
            
            if (Input.GetKeyDown(shakeKey))
            {
                DemoShakeCamera();
            }
            
            if (Input.GetKeyDown(transitionKey))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    DemoFadeTransition();
                }
                else
                {
                    DemoTransitionCamera();
                }
            }
        }
        
        /// <summary>
        /// 切换到下一个摄像机
        /// </summary>
        private async void SwitchToNextCamera()
        {
            var allCameras = cameraManager.GetAllCameras();
            var cameraTypes = new CameraType[allCameras.Count];
            allCameras.Keys.CopyTo(cameraTypes, 0);
            
            if (cameraTypes.Length <= 1) return;
            
            // 找到当前摄像机的索引
            int currentIndex = 0;
            for (int i = 0; i < cameraTypes.Length; i++)
            {
                if (cameraManager.IsCameraActive(cameraTypes[i]))
                {
                    currentIndex = i;
                    break;
                }
            }
            
            // 切换到下一个摄像机
            int nextIndex = (currentIndex + 1) % cameraTypes.Length;
            var nextCameraType = cameraTypes[nextIndex];
            
            Debug.Log($"[CameraUsageExample] 手动切换摄像机: {cameraTypes[currentIndex]} -> {nextCameraType}");
            await cameraManager.SwitchCamera(nextCameraType, false);
        }
        
        /// <summary>
        /// 在Inspector中显示帮助信息
        /// </summary>
        [ContextMenu("显示帮助")]
        private void ShowHelp()
        {
            Debug.Log("[CameraUsageExample] === 操作说明 ===");
            Debug.Log($"[CameraUsageExample] {switchCameraKey}: 切换摄像机");
            Debug.Log($"[CameraUsageExample] {shakeKey}: 摄像机震动");
            Debug.Log($"[CameraUsageExample] {transitionKey}: 摄像机过渡");
            Debug.Log($"[CameraUsageExample] Shift + {transitionKey}: 淡入淡出过渡");
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("摄像机系统示例", GUI.skin.box);
            
            if (cameraManager != null)
            {
                var activeCamera = cameraManager.ActiveCamera;
                GUILayout.Label($"当前摄像机: {(activeCamera ? activeCamera.name : "无")}");
                
                var allCameras = cameraManager.GetAllCameras();
                GUILayout.Label($"注册摄像机数量: {allCameras.Count}");
            }
            
            GUILayout.Space(10);
            GUILayout.Label("操作说明:");
            GUILayout.Label($"{switchCameraKey}: 切换摄像机");
            GUILayout.Label($"{shakeKey}: 摄像机震动");
            GUILayout.Label($"{transitionKey}: 摄像机过渡");
            GUILayout.Label($"Shift + {transitionKey}: 淡入淡出过渡");
            
            GUILayout.EndArea();
        }
    }
}