using UnityEngine;
using Game.Infrastructure.Camera.Core;
using CameraType = Game.Infrastructure.Camera.Core.CameraType;

namespace Game.Infrastructure.Camera.Controllers
{
    /// <summary>
    /// UI摄像机控制器 - 专门用于渲染UI界面
    /// </summary>
    public class UICameraController : BaseCameraController
    {
        [Header("UI摄像机设置")]
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private float uiDistance = 100f;
        [SerializeField] private bool autoFindCanvas = true;
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            cameraType = CameraType.UI;
            
            // 设置UI摄像机特有配置
            if (cameraConfig == null)
            {
                cameraConfig = new CameraConfig(CameraType.UI);
            }
            
            // UI摄像机通常使用正交模式
            cameraConfig.orthographic = true;
            cameraConfig.clearFlags = CameraClearFlags.Depth;
            cameraConfig.depth = 10; // 确保UI摄像机在主摄像机之上
            
            // 自动查找Canvas
            if (autoFindCanvas && targetCanvas == null)
            {
                FindAndSetupCanvas();
            }
            
            SetupUICamera();
        }
        
        /// <summary>
        /// 查找并设置Canvas
        /// </summary>
        private void FindAndSetupCanvas()
        {
            // 首先尝试找到UI层的Canvas
            var canvases = FindObjectsOfType<Canvas>();
            
            foreach (var canvas in canvases)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
                {
                    SetTargetCanvas(canvas);
                    Debug.Log($"[UICameraController] 自动找到Canvas: {canvas.name}");
                    break;
                }
            }
            
            // 如果没有找到合适的Canvas，尝试找到第一个Canvas
            if (targetCanvas == null && canvases.Length > 0)
            {
                SetTargetCanvas(canvases[0]);
                Debug.Log($"[UICameraController] 使用第一个找到的Canvas: {canvases[0].name}");
            }
        }
        
        /// <summary>
        /// 设置UI摄像机
        /// </summary>
        private void SetupUICamera()
        {
            if (controlledCamera == null) return;
            
            // 应用UI摄像机特有设置
            controlledCamera.orthographic = true;
            controlledCamera.clearFlags = CameraClearFlags.Depth;
            controlledCamera.cullingMask = 1 << LayerMask.NameToLayer("UI");
            controlledCamera.depth = 10;
            
            // 设置位置
            transform.position = new Vector3(0, 0, -uiDistance);
            transform.rotation = Quaternion.identity;
            
            Debug.Log("[UICameraController] UI摄像机设置完成");
        }
        
        /// <summary>
        /// 设置目标Canvas
        /// </summary>
        /// <param name="canvas">目标Canvas</param>
        public void SetTargetCanvas(Canvas canvas)
        {
            if (canvas == null) return;
            
            targetCanvas = canvas;
            
            // 设置Canvas使用此摄像机
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera || 
                canvas.renderMode == RenderMode.WorldSpace)
            {
                canvas.worldCamera = controlledCamera;
                canvas.planeDistance = uiDistance;
            }
            
            Debug.Log($"[UICameraController] 设置目标Canvas: {canvas.name}");
        }
        
        /// <summary>
        /// 设置UI距离
        /// </summary>
        /// <param name="distance">UI距离</param>
        public void SetUIDistance(float distance)
        {
            uiDistance = distance;
            
            // 更新摄像机位置
            var pos = transform.position;
            pos.z = -distance;
            transform.position = pos;
            
            // 更新Canvas距离
            if (targetCanvas != null)
            {
                targetCanvas.planeDistance = distance;
            }
        }
        
        /// <summary>
        /// 调整UI摄像机的正交大小以适应屏幕
        /// </summary>
        public void AdjustOrthographicSize()
        {
            if (controlledCamera == null || !controlledCamera.orthographic) return;
            
            // 基于屏幕分辨率调整正交大小
            float screenHeight = Screen.height;
            float screenWidth = Screen.width;
            float aspectRatio = screenWidth / screenHeight;
            
            // 通常UI摄像机的正交大小应该是屏幕高度的一半
            controlledCamera.orthographicSize = screenHeight * 0.5f;
            
            Debug.Log($"[UICameraController] 调整正交大小: {controlledCamera.orthographicSize}, 宽高比: {aspectRatio}");
        }
        
        /// <summary>
        /// 设置UI层遮罩
        /// </summary>
        /// <param name="layerMask">层遮罩</param>
        public void SetUILayerMask(LayerMask layerMask)
        {
            if (controlledCamera != null)
            {
                controlledCamera.cullingMask = layerMask;
                cameraConfig.cullingMask = layerMask;
            }
        }
        
        protected override void OnActivate()
        {
            base.OnActivate();
            
            // 当UI摄像机激活时，确保Canvas使用正确的摄像机
            if (targetCanvas != null && targetCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                targetCanvas.worldCamera = controlledCamera;
            }
            
            Debug.Log("[UICameraController] UI摄像机激活");
        }
        
        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            
            // 当UI摄像机停用时，可能需要将Canvas切换回Overlay模式
            if (targetCanvas != null)
            {
                // 可以选择切换回ScreenSpaceOverlay模式
                // targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                // targetCanvas.worldCamera = null;
            }
            
            Debug.Log("[UICameraController] UI摄像机停用");
        }
        
        /// <summary>
        /// 处理屏幕分辨率变化
        /// </summary>
        private void OnRectTransformDimensionsChange()
        {
            if (controlledCamera != null && controlledCamera.orthographic)
            {
                AdjustOrthographicSize();
            }
        }
        
        #region Inspector工具方法
        
        [ContextMenu("自动查找Canvas")]
        private void FindCanvas()
        {
            FindAndSetupCanvas();
        }
        
        [ContextMenu("调整正交大小")]
        private void AdjustSize()
        {
            AdjustOrthographicSize();
        }
        
        [ContextMenu("重新设置UI摄像机")]
        private void ResetupUICamera()
        {
            SetupUICamera();
        }
        
        #endregion
    }
}