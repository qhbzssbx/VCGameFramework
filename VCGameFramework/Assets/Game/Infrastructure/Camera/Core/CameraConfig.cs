using UnityEngine;

namespace Game.Infrastructure.Camera.Core
{
    /// <summary>
    /// 摄像机配置数据
    /// </summary>
    [System.Serializable]
    public class CameraConfig
    {
        [Header("基本设置")]
        public CameraType cameraType = CameraType.Main;
        public int priority = 0;
        public bool isPersistent = false;
        
        [Header("渲染设置")]
        public CameraClearFlags clearFlags = CameraClearFlags.Skybox;
        public Color backgroundColor = Color.black;
        public LayerMask cullingMask = -1;
        public RenderingPath renderingPath = RenderingPath.UsePlayerSettings;
        
        [Header("投影设置")]
        public bool orthographic = false;
        public float fieldOfView = 60f;
        public float orthographicSize = 5f;
        public float nearClipPlane = 0.3f;
        public float farClipPlane = 1000f;
        
        [Header("视口设置")]
        public Rect viewportRect = new Rect(0, 0, 1, 1);
        public float depth = -1;
        
        [Header("位置设置")]
        public Vector3 position = Vector3.zero;
        public Vector3 rotation = Vector3.zero;
        
        public CameraConfig()
        {
        }
        
        public CameraConfig(CameraType type)
        {
            cameraType = type;
            SetDefaultsByType(type);
        }
        
        /// <summary>
        /// 根据摄像机类型设置默认值
        /// </summary>
        private void SetDefaultsByType(CameraType type)
        {
            switch (type)
            {
                case CameraType.Main:
                    priority = 0;
                    depth = -1;
                    clearFlags = CameraClearFlags.Skybox;
                    cullingMask = -1;
                    break;
                    
                case CameraType.UI:
                    priority = 100;
                    depth = 10;
                    clearFlags = CameraClearFlags.Depth;
                    cullingMask = 1 << LayerMask.NameToLayer("UI");
                    orthographic = true;
                    break;
                    
                case CameraType.Effect:
                    priority = 50;
                    depth = 5;
                    clearFlags = CameraClearFlags.Depth;
                    break;
                    
                case CameraType.Minimap:
                    priority = 20;
                    depth = 2;
                    clearFlags = CameraClearFlags.Skybox;
                    orthographic = true;
                    viewportRect = new Rect(0.7f, 0.7f, 0.3f, 0.3f);
                    break;
                    
                case CameraType.Cinematic:
                    priority = 200;
                    depth = 20;
                    clearFlags = CameraClearFlags.Skybox;
                    break;
            }
        }
        
        /// <summary>
        /// 应用配置到摄像机
        /// </summary>
        public void ApplyTo(UnityEngine.Camera camera)
        {
            if (camera == null) return;
            
            camera.clearFlags = clearFlags;
            camera.backgroundColor = backgroundColor;
            camera.cullingMask = cullingMask;
            camera.renderingPath = renderingPath;
            
            camera.orthographic = orthographic;
            camera.fieldOfView = fieldOfView;
            camera.orthographicSize = orthographicSize;
            camera.nearClipPlane = nearClipPlane;
            camera.farClipPlane = farClipPlane;
            
            camera.rect = viewportRect;
            camera.depth = depth;
            
            var transform = camera.transform;
            transform.position = position;
            transform.eulerAngles = rotation;
        }
    }
}