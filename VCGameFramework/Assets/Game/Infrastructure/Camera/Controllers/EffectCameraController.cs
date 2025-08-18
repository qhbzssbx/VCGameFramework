using UnityEngine;
using Game.Infrastructure.Camera.Core;
using CameraType = Game.Infrastructure.Camera.Core.CameraType;

namespace Game.Infrastructure.Camera.Controllers
{
    /// <summary>
    /// 特效摄像机控制器 - 用于渲染特殊效果和后处理
    /// </summary>
    public class EffectCameraController : BaseCameraController
    {
        [Header("特效摄像机设置")]
        [SerializeField] private LayerMask effectLayers = -1;
        [SerializeField] private RenderTexture renderTexture;
        [SerializeField] private bool createRenderTexture = true;
        [SerializeField] private Vector2Int renderTextureSize = new Vector2Int(1920, 1080);
        
        [Header("后处理设置")]
        [SerializeField] private bool enablePostProcessing = true;
        [SerializeField] private Material postProcessMaterial;
        
        private RenderTexture originalTargetTexture;
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            cameraType = CameraType.Effect;
            
            // 设置特效摄像机配置
            if (cameraConfig == null)
            {
                cameraConfig = new CameraConfig(CameraType.Effect);
            }
            
            cameraConfig.clearFlags = CameraClearFlags.Depth;
            cameraConfig.depth = 5; // 在主摄像机之后，UI摄像机之前
            cameraConfig.cullingMask = effectLayers;
            
            SetupEffectCamera();
            CreateRenderTextureIfNeeded();
        }
        
        /// <summary>
        /// 设置特效摄像机
        /// </summary>
        private void SetupEffectCamera()
        {
            if (controlledCamera == null) return;
            
            controlledCamera.clearFlags = CameraClearFlags.Depth;
            controlledCamera.cullingMask = effectLayers;
            controlledCamera.depth = 5;
            
            // 保存原始目标纹理
            originalTargetTexture = controlledCamera.targetTexture;
            
            Debug.Log("[EffectCameraController] 特效摄像机设置完成");
        }
        
        /// <summary>
        /// 创建渲染纹理
        /// </summary>
        private void CreateRenderTextureIfNeeded()
        {
            if (!createRenderTexture || renderTexture != null) return;
            
            renderTexture = new RenderTexture(
                renderTextureSize.x, 
                renderTextureSize.y, 
                24, 
                RenderTextureFormat.ARGB32
            );
            
            renderTexture.name = $"EffectCamera_RT_{GetInstanceID()}";
            renderTexture.Create();
            
            if (controlledCamera != null)
            {
                controlledCamera.targetTexture = renderTexture;
            }
            
            Debug.Log($"[EffectCameraController] 创建渲染纹理: {renderTextureSize.x}x{renderTextureSize.y}");
        }
        
        /// <summary>
        /// 设置特效层遮罩
        /// </summary>
        /// <param name="layerMask">层遮罩</param>
        public void SetEffectLayers(LayerMask layerMask)
        {
            effectLayers = layerMask;
            
            if (controlledCamera != null)
            {
                controlledCamera.cullingMask = layerMask;
                cameraConfig.cullingMask = layerMask;
            }
        }
        
        /// <summary>
        /// 设置渲染纹理
        /// </summary>
        /// <param name="texture">目标渲染纹理</param>
        public void SetRenderTexture(RenderTexture texture)
        {
            // 释放旧的渲染纹理
            if (renderTexture != null && createRenderTexture)
            {
                renderTexture.Release();
                DestroyImmediate(renderTexture);
            }
            
            renderTexture = texture;
            
            if (controlledCamera != null)
            {
                controlledCamera.targetTexture = texture;
            }
        }
        
        /// <summary>
        /// 获取渲染纹理
        /// </summary>
        /// <returns>当前使用的渲染纹理</returns>
        public RenderTexture GetRenderTexture()
        {
            return renderTexture;
        }
        
        /// <summary>
        /// 设置后处理材质
        /// </summary>
        /// <param name="material">后处理材质</param>
        public void SetPostProcessMaterial(Material material)
        {
            postProcessMaterial = material;
        }
        
        /// <summary>
        /// 启用/禁用后处理
        /// </summary>
        /// <param name="enabled">是否启用</param>
        public void SetPostProcessingEnabled(bool enabled)
        {
            enablePostProcessing = enabled;
        }
        
        /// <summary>
        /// 渲染到指定的渲染纹理
        /// </summary>
        /// <param name="targetTexture">目标纹理</param>
        public void RenderToTexture(RenderTexture targetTexture)
        {
            if (controlledCamera == null || targetTexture == null) return;
            
            var originalTarget = controlledCamera.targetTexture;
            controlledCamera.targetTexture = targetTexture;
            controlledCamera.Render();
            controlledCamera.targetTexture = originalTarget;
        }
        
        /// <summary>
        /// 应用后处理效果
        /// </summary>
        /// <param name="source">源纹理</param>
        /// <param name="destination">目标纹理</param>
        public void ApplyPostProcessing(RenderTexture source, RenderTexture destination)
        {
            if (!enablePostProcessing || postProcessMaterial == null)
            {
                Graphics.Blit(source, destination);
                return;
            }
            
            Graphics.Blit(source, destination, postProcessMaterial);
        }
        
        /// <summary>
        /// 捕获屏幕截图
        /// </summary>
        /// <returns>截图纹理</returns>
        public Texture2D CaptureScreenshot()
        {
            if (controlledCamera == null || renderTexture == null) return null;
            
            var screenshot = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            
            RenderTexture.active = renderTexture;
            controlledCamera.Render();
            screenshot.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            screenshot.Apply();
            RenderTexture.active = null;
            
            return screenshot;
        }
        
        protected override void OnActivate()
        {
            base.OnActivate();
            
            if (controlledCamera != null && renderTexture != null)
            {
                controlledCamera.targetTexture = renderTexture;
            }
            
            Debug.Log("[EffectCameraController] 特效摄像机激活");
        }
        
        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            
            if (controlledCamera != null)
            {
                controlledCamera.targetTexture = originalTargetTexture;
            }
            
            Debug.Log("[EffectCameraController] 特效摄像机停用");
        }
        
        protected override void OnCleanup()
        {
            base.OnCleanup();
            
            // 释放渲染纹理
            if (renderTexture != null && createRenderTexture)
            {
                renderTexture.Release();
                DestroyImmediate(renderTexture);
                renderTexture = null;
            }
        }
        
        /// <summary>
        /// 渲染后处理
        /// </summary>
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (enablePostProcessing && postProcessMaterial != null)
            {
                Graphics.Blit(source, destination, postProcessMaterial);
            }
            else
            {
                Graphics.Blit(source, destination);
            }
        }
        
        #region Inspector工具方法
        
        [ContextMenu("创建渲染纹理")]
        private void CreateRT()
        {
            CreateRenderTextureIfNeeded();
        }
        
        [ContextMenu("释放渲染纹理")]
        private void ReleaseRT()
        {
            if (renderTexture != null)
            {
                renderTexture.Release();
                if (createRenderTexture)
                {
                    DestroyImmediate(renderTexture);
                    renderTexture = null;
                }
            }
        }
        
        [ContextMenu("捕获截图")]
        private void CaptureScreenshotTest()
        {
            var screenshot = CaptureScreenshot();
            if (screenshot != null)
            {
                var bytes = screenshot.EncodeToPNG();
                System.IO.File.WriteAllBytes($"Screenshot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png", bytes);
                DestroyImmediate(screenshot);
                Debug.Log("截图已保存");
            }
        }
        
        #endregion
    }
}