using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;
using Game.Infrastructure.Camera.Core;
using Game.Core.UI;
using CameraType = Game.Infrastructure.Camera.Core.CameraType;
using UILayer = Game.Core.UI.UILayer;

namespace Game.UI.Core
{
    /// <summary>
    /// UI系统管理器
    /// 负责UI的生命周期管理、层级控制和资源加载
    /// </summary>
    public class UISystem : MonoBehaviour, IUIManager, IUIContainer
    {
        public static UISystem Instance { get; private set; }
        
        [Header("UI系统配置")]
        private Transform uiRoot;
        private Camera uiCamera;
        
        /// <summary>
        /// 各层级的父节点
        /// </summary>
        private readonly Dictionary<UILayer, Transform> layerParents = new();
        
        /// <summary>
        /// 当前显示的UI面板
        /// </summary>
        private readonly Dictionary<Type, IUIPanel> activePanels = new();
        
        /// <summary>
        /// UI预制体缓存
        /// </summary>
        private readonly Dictionary<Type, GameObject> prefabCache = new();
        
        /// <summary>
        /// UI资源加载器
        /// </summary>
        [Inject] private IUIResourceLoader uiResourceLoader;
        
        /// <summary>
        /// 摄像机管理器
        /// </summary>
        [Inject] private ICameraManager cameraManager;
        
        #region 初始化
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeUISystem();
                
                // 注册到UI管理器服务
                UIManagerService.RegisterManager(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 初始化UI系统
        /// </summary>
        private void InitializeUISystem()
        {
            // 初始化UI摄像机
            InitializeUICamera();
            
            // 创建UI根节点
            if (uiRoot == null)
            {
                var rootGO = new GameObject("UIRoot");
                rootGO.transform.SetParent(transform);
                uiRoot = rootGO.transform;

                
            }
            
            // 创建各层级的父节点
            CreateLayerParents();
            

        }
        
        /// <summary>
        /// 创建UI层级父节点
        /// </summary>
        private void CreateLayerParents()
        {
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                var layerGO = new GameObject($"{layer}Layer");
                DontDestroyOnLoad(layerGO);
                layerGO.transform.SetParent(uiRoot, false);
                
                // 设置Canvas组件用于层级控制
                var canvas = layerGO.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = (int)layer;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = uiCamera;
                canvas.pixelPerfect = false;

                // 添加CanvasScaler
                var scaler = layerGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                // 添加GraphicRaycaster
                layerGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                
                layerParents[layer] = layerGO.transform;
            }
        }
        
        #endregion
        
        #region 公共API
        
        /// <summary>
        /// 显示UI面板
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        /// <param name="assetKey">资源键名</param>
        /// <param name="args">传入参数</param>
        /// <returns>UI面板实例</returns>
        public async UniTask<IUIPanel> ShowAsync(Type type, params object[] args)
        {
            string assetKey = type.Name;
            
            // 如果UI已经存在且正在显示，直接返回
            if (activePanels.TryGetValue(type, out var existingPanel) && existingPanel.IsShowing)
            {
                return existingPanel;
            }
            
            // 加载UI预制体
            var prefab = await LoadUIPrefab(type);
            if (prefab == null)
            {
                Debug.LogError($"UI预制体加载失败: {assetKey}");
                return null;
            }
            
            // 创建UI实例
            var panelInstance = await CreateUIInstance(prefab, type);
            if (panelInstance == null)
            {
                Debug.LogError($"UI实例创建失败: {assetKey}");
                return null;
            }
            
            // 设置UI层级
            SetupUILayer(panelInstance);
            
            // 创建和设置Handle
            var handle = new DefaultUIHandle(panelInstance, this, type);
            panelInstance.SetHandle(handle);
            
            // 注册到管理器
            RegisterPanel(type, panelInstance);
            
            // 显示UI
            await panelInstance.ShowAsync(args);
            
            return panelInstance;
        }
        
        /// <summary>
        /// 隐藏UI面板
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        public async UniTask HideAsync<T>() where T : class, IUIPanel
        {
            var panelType = typeof(T);
            
            if (activePanels.TryGetValue(panelType, out var panel))
            {
                await panel.HideAsync();
                
                // 如果设置为自动销毁，从管理器中移除
                if (panel is UIPanel uiPanel && uiPanel.AutoDestroy)
                {
                    UnregisterPanel(panelType);
                }
            }
        }
        
        /// <summary>
        /// 获取UI面板实例
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        /// <returns>UI面板实例，如果不存在返回null</returns>
        public T GetPanel<T>() where T : class, IUIPanel
        {
            var panelType = typeof(T);
            return activePanels.TryGetValue(panelType, out var panel) ? panel as T : null;
        }
        
        /// <summary>
        /// 检查UI是否正在显示
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        /// <returns>是否正在显示</returns>
        public bool IsShowing<T>() where T : class, IUIPanel
        {
            var panel = GetPanel<T>();
            return panel != null && panel.IsShowing;
        }
        
        /// <summary>
        /// 切换UI显示状态
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        /// <param name="assetKey">资源键名</param>
        /// <param name="args">传入参数</param>
        public async UniTask ToggleAsync<T>(Type type, params object[] args) where T : class, IUIPanel
        {
            if (IsShowing<T>())
            {
                await HideAsync<T>();
            }
            else
            {
                await ShowAsync(type, args);
            }
        }
        
        /// <summary>
        /// 关闭所有UI面板
        /// </summary>
        public async UniTask HideAllAsync()
        {
            var hideList = new List<UniTask>();
            
            foreach (var panel in activePanels.Values)
            {
                if (panel.IsShowing)
                {
                    hideList.Add(panel.HideAsync());
                }
            }
            
            await UniTask.WhenAll(hideList);
            Clear();
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 加载UI预制体
        /// </summary>
        private async UniTask<GameObject> LoadUIPrefab(Type type)
        {
            // 检查缓存
            if (prefabCache.TryGetValue(type, out var cachedPrefab))
            {
                return cachedPrefab;
            }
            
            // 使用UI资源加载器加载资源
            if (uiResourceLoader == null)
            {
                Debug.LogError("UIResourceLoader 未注入，无法加载UI资源");
                return null;
            }
            
            var prefab = await uiResourceLoader.LoadUIPrefabAsync(type.Name);
            if (prefab != null)
            {
                prefabCache[type] = prefab;
                return prefab;
            }
            
            return null;
        }
        
        /// <summary>
        /// 创建UI实例
        /// </summary>
        private async UniTask<IUIPanel> CreateUIInstance(GameObject prefab, Type type)
        {
            var instance = Instantiate(prefab);
            
            // 尝试获取或添加UI组件
            var panelComponent = instance.GetComponent(type);
            if (panelComponent == null)
            {
                panelComponent = instance.AddComponent(type);
            }
            
            await UniTask.CompletedTask;
            return panelComponent as IUIPanel;
        }
        
        /// <summary>
        /// 设置UI层级
        /// </summary>
        private void SetupUILayer(IUIPanel panel)
        {
            var layer = panel.Layer;
            
            // 确保panel是MonoBehaviour才能访问transform
            if (panel is MonoBehaviour monoBehaviour)
            {
                // 设置父节点
                if (layerParents.TryGetValue(layer, out var parent))
                {
                    monoBehaviour.transform.SetParent(parent, false);
                }
                
                // 设置Canvas排序
                var canvas = monoBehaviour.GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = monoBehaviour.gameObject.AddComponent<Canvas>();
                    monoBehaviour.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }
                
                canvas.overrideSorting = true;
                canvas.sortingOrder = (int)layer;
            }
        }
        
        #endregion
        
        #region Unity生命周期
        
        private void OnDestroy()
        {
            // 从UI管理器服务注销
            UIManagerService.UnregisterManager(this);
            
            // 清理UI资源加载器
            uiResourceLoader?.Dispose();
            
            // 清理缓存
            prefabCache.Clear();
            activePanels.Clear();
            layerParents.Clear();
        }
        
        /// <summary>
        /// 初始化UI摄像机
        /// </summary>
        private void InitializeUICamera()
        {
            // 如果已经有指定的UI摄像机，直接使用
            if (uiCamera != null)
            {
                RegisterUICamera(uiCamera);
                return;
            }
            
            // 如果摄像机管理器不存在，延后初始化
            if (cameraManager == null)
            {
                Debug.LogWarning("[UISystem] 摄像机管理器未注入，延后初始化UI摄像机");
                return;
            }
            
            // 尝试从摄像机管理器获取UI摄像机
            var existingUICamera = cameraManager.GetCamera(CameraType.UI);
            if (existingUICamera != null)
            {
                uiCamera = existingUICamera;
                SetupUICanvas(uiCamera);
                Debug.Log("[UISystem] 使用现有UI摄像机");
                return;
            }
            
            // 创建新的UI摄像机
            CreateUICamera();
        }
        
        /// <summary>
        /// 创建UI摄像机
        /// </summary>
        private void CreateUICamera()
        {
            if (cameraManager == null) return;
            
            // 创建UI摄像机配置
            var uiConfig = new CameraConfig(Infrastructure.Camera.Core.CameraType.UI)
            {
                position = new Vector3(0, 0, -100),
                rotation = Vector3.zero,
                orthographic = true,
                orthographicSize = Screen.height * 0.5f,
                depth = 10,
                clearFlags = CameraClearFlags.Depth,
                cullingMask = 1 << LayerMask.NameToLayer("UI")
            };
            
            // 创建摄像机
            uiCamera = cameraManager.CreateCamera(uiConfig, transform);
            if (uiCamera != null)
            {
                uiCamera.name = "UICamera";
                RegisterUICamera(uiCamera);
                Debug.Log("[UISystem] 创建UI摄像机成功");
            }
            else
            {
                Debug.LogError("[UISystem] 创建UI摄像机失败");
            }
        }
        
        /// <summary>
        /// 注册UI摄像机到管理器
        /// </summary>
        private void RegisterUICamera(Camera camera)
        {
            if (camera == null || cameraManager == null) return;
            
            var config = new CameraConfig(CameraType.UI);
            config.ApplyTo(camera);
            
            cameraManager.RegisterCamera(camera, config);
            SetupUICanvas(camera);
            
            Debug.Log($"[UISystem] 注册UI摄像机: {camera.name}");
        }
        
        /// <summary>
        /// 设置UI Canvas使用指定摄像机
        /// </summary>
        private void SetupUICanvas(Camera camera)
        {
            if (uiRoot == null || camera == null) return;
            
            var canvas = uiRoot.GetComponent<Canvas>();
            if (canvas != null)
            {
                // 根据需要切换到Camera渲染模式
                if (canvas.renderMode != RenderMode.ScreenSpaceCamera)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = camera;
                    canvas.planeDistance = 100f;
                    
                    Debug.Log("[UISystem] UI Canvas切换到Camera渲染模式");
                }
            }
        }
        
        /// <summary>
        /// 获取UI摄像机
        /// </summary>
        public Camera GetUICamera()
        {
            return uiCamera;
        }
        
        /// <summary>
        /// 设置UI摄像机
        /// </summary>
        /// <param name="camera">新的UI摄像机</param>
        public void SetUICamera(Camera camera)
        {
            if (camera == null) return;
            
            uiCamera = camera;
            RegisterUICamera(camera);
        }
        
        /// <summary>
        /// 切换UI渲染模式
        /// </summary>
        /// <param name="renderMode">渲染模式</param>
        public void SwitchUIRenderMode(RenderMode renderMode)
        {
            if (uiRoot == null) return;
            
            var canvas = uiRoot.GetComponent<Canvas>();
            if (canvas == null) return;
            
            canvas.renderMode = renderMode;
            
            switch (renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    canvas.worldCamera = null;
                    Debug.Log("[UISystem] 切换到Overlay渲染模式");
                    break;
                    
                case RenderMode.ScreenSpaceCamera:
                    if (uiCamera != null)
                    {
                        canvas.worldCamera = uiCamera;
                        canvas.planeDistance = 100f;
                    }
                    Debug.Log("[UISystem] 切换到Camera渲染模式");
                    break;
                    
                case RenderMode.WorldSpace:
                    if (uiCamera != null)
                    {
                        canvas.worldCamera = uiCamera;
                    }
                    Debug.Log("[UISystem] 切换到WorldSpace渲染模式");
                    break;
            }
        }
        
        #endregion
        
        #region IUIContainer实现
        
        /// <summary>
        /// 注册UI面板
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <param name="panel">面板实例</param>
        public void RegisterPanel(Type panelType, IUIPanel panel)
        {
            if (panelType == null) throw new ArgumentNullException(nameof(panelType));
            if (panel == null) throw new ArgumentNullException(nameof(panel));
            
            activePanels[panelType] = panel;
            Debug.Log($"[UISystem] 注册UI面板: {panelType.Name}");
        }
        
        /// <summary>
        /// 注销UI面板
        /// </summary>
        /// <param name="panelType">面板类型</param>
        public void UnregisterPanel(Type panelType)
        {
            if (panelType == null) return;
            
            if (activePanels.Remove(panelType))
            {
                Debug.Log($"[UISystem] 注销UI面板: {panelType.Name}");
            }
        }
        
        /// <summary>
        /// 获取指定类型的UI面板
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <returns>面板实例，如果不存在返回null</returns>
        public IUIPanel GetPanel(Type panelType)
        {
            if (panelType == null) return null;
            return activePanels.TryGetValue(panelType, out var panel) ? panel : null;
        }
        
        /// <summary>
        /// 获取所有注册的UI面板
        /// </summary>
        /// <returns>所有面板实例的枚举</returns>
        public IEnumerable<IUIPanel> GetAllPanels()
        {
            return activePanels.Values;
        }
        
        /// <summary>
        /// 检查指定类型的面板是否已注册
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <returns>是否已注册</returns>
        public bool HasPanel(Type panelType)
        {
            if (panelType == null) return false;
            return activePanels.ContainsKey(panelType);
        }
        
        /// <summary>
        /// 清空所有注册的面板
        /// </summary>
        public void Clear()
        {
            activePanels.Clear();
            Debug.Log("[UISystem] 清空所有UI面板注册");
        }
        
        #endregion
    }
}