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
    /// UI系统管理器：负责UI的生命周期管理、层级控制和资源加载
    /// </summary>
    public class UISystem : MonoBehaviour, IUIManager, IUIContainer
    {
        public static UISystem Instance { get; private set; }

        [Header("UI系统配置")]
        private Transform uiRoot;
        private Camera uiCamera;

        private readonly Dictionary<UILayer, Transform> layerParents = new();
        private readonly Dictionary<string, IUIPanel> activePanels = new();
        private readonly Dictionary<string, GameObject> prefabCache = new();

        [Inject] private IUIResourceLoader uiResourceLoader;
        [Inject] private ICameraManager cameraManager;

        #region 初始化
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeUISystem();
                UIManagerService.RegisterManager(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeUISystem()
        {
            InitializeUICamera();

            if (uiRoot == null)
            {
                var rootGO = new GameObject("UIRoot");
                rootGO.transform.SetParent(transform);
                uiRoot = rootGO.transform;
            }

            CreateLayerParents();
        }

        private void CreateLayerParents()
        {
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                var layerGO = new GameObject($"{layer}Layer");
                DontDestroyOnLoad(layerGO);
                layerGO.transform.SetParent(uiRoot, false);

                var canvas = layerGO.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = (int)layer;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = uiCamera;
                canvas.pixelPerfect = false;

                var scaler = layerGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                layerGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

                layerParents[layer] = layerGO.transform;
            }
        }
        #endregion

        #region IUIManager 实现（按三个接口文件修正）

        /// <summary>显示（或复用并再次显示）UI面板（带参数版）</summary>
        public async UniTask<TPanel> ShowAsync<TPanel, TParams>(TParams @params)
            where TPanel : class, IUIPanel
            where TParams : struct, IUIParams
        {
            string assetKey = typeof(TPanel).Name;

            // 已创建面板：复用实例并直接分发到 IUIPanel<TParams>
            if (activePanels.TryGetValue(assetKey, out var existing))
            {
                if (existing is IUIPanel<TParams> typedExisting)
                {
                    if (!existing.IsShowing)
                        await typedExisting.ShowAsync(in @params);
                    return existing as TPanel;
                }

                Debug.LogError($"[UISystem] 面板 {assetKey} 已存在，但不支持参数类型 {typeof(TParams).Name}");
                return existing as TPanel; // 退让返回，避免空引用
            }

            // 加载与创建
            var prefab = await LoadUIPrefab(assetKey);
            if (prefab == null)
            {
                Debug.LogError($"[UISystem] UI预制体加载失败: {assetKey}");
                return null;
            }

            var panelInstance = await CreateUIInstance(prefab, typeof(TPanel));
            if (panelInstance == null)
            {
                Debug.LogError($"[UISystem] UI实例创建失败: {assetKey}");
                return null;
            }

            SetupUILayer(panelInstance);

            var handle = new DefaultUIHandle(panelInstance, this, assetKey);
            panelInstance.SetHandle(handle);

            RegisterPanel(assetKey, panelInstance);

            // 按参数类型调用
            if (panelInstance is IUIPanel<TParams> typed)
            {
                await typed.ShowAsync(in @params);
            }
            else
            {
                Debug.LogError($"[UISystem] 面板 {assetKey} 不支持参数类型 {typeof(TParams).Name}");
            }

            return panelInstance as TPanel;
        }

        /// <summary>显示（或复用并再次显示）UI面板（无参版，使用 EmptyUIParams）</summary>
        public async UniTask<T> ShowAsync<T>()
            where T : class, IUIPanel, IUIPanel<EmptyUIParams>
        {
            string assetKey = typeof(T).Name;

            if (activePanels.TryGetValue(assetKey, out var existing))
            {
                if (existing is IUIPanel<EmptyUIParams> typedExisting)
                {
                    if (!existing.IsShowing)
                        await typedExisting.ShowAsync(in EmptyUIParams.Instance);
                    return existing as T;
                }

                Debug.LogError($"[UISystem] 面板 {assetKey} 已存在，但未实现 IUIPanel<EmptyUIParams>。");
                return existing as T;
            }

            var prefab = await LoadUIPrefab(assetKey);
            if (prefab == null)
            {
                Debug.LogError($"[UISystem] UI预制体加载失败: {assetKey}");
                return null;
            }

            var panelInstance = await CreateUIInstance(prefab, typeof(T));
            if (panelInstance == null)
            {
                Debug.LogError($"[UISystem] UI实例创建失败: {assetKey}");
                return null;
            }

            SetupUILayer(panelInstance);

            var handle = new DefaultUIHandle(panelInstance, this, assetKey);
            panelInstance.SetHandle(handle);

            RegisterPanel(assetKey, panelInstance);

            if (panelInstance is IUIPanel<EmptyUIParams> typed)
            {
                await typed.ShowAsync(in EmptyUIParams.Instance);
            }
            else
            {
                Debug.LogError($"[UISystem] 面板 {assetKey} 未实现 IUIPanel<EmptyUIParams>。");
            }

            return panelInstance as T;
        }

        /// <summary>隐藏指定 UI 面板</summary>
        public async UniTask HideAsync<TPanel>() where TPanel : class, IUIPanel
        {
            var panelName = typeof(TPanel).Name;

            if (activePanels.TryGetValue(panelName, out var panel))
            {
                await panel.HideAsync();
                // 按接口定义，暂不做 AutoDestroy 之类的项目内扩展处理
            }
        }

        /// <summary>隐藏所有 UI 面板</summary>
        public async UniTask HideAllAsync()
        {
            var tasks = new List<UniTask>();
            foreach (var panel in activePanels.Values)
                if (panel.IsShowing) tasks.Add(panel.HideAsync());

            await UniTask.WhenAll(tasks);
            Clear();
        }

        /// <summary>获取 UI 面板实例</summary>
        public TPanel GetPanel<TPanel>() where TPanel : class, IUIPanel
        {
            var key = typeof(TPanel).Name;
            return activePanels.TryGetValue(key, out var panel) ? panel as TPanel : null;
        }

        /// <summary>检查 UI 面板是否正在显示</summary>
        public bool IsShowing<TPanel>() where TPanel : class, IUIPanel
        {
            var panel = GetPanel<TPanel>();
            return panel != null && panel.IsShowing;
        }

        /// <summary>切换显示状态（带参数版）</summary>
        public async UniTask ToggleAsync<TPanel, TParams>(TParams args)
            where TPanel : class, IUIPanel<TParams>
            where TParams : struct, IUIParams
        {
            if (IsShowing<TPanel>())
                await HideAsync<TPanel>();
            else
                await ShowAsync<TPanel, TParams>(args);
        }

        /// <summary>切换显示状态（无参版）</summary>
        public async UniTask ToggleAsync<T>() where T : class, IUIPanel<EmptyUIParams>
        {
            if (IsShowing<T>())
                await HideAsync<T>();
            else
                await ShowAsync<T>();
        }

        #endregion

        #region 私有方法（未改动或小幅调整）

        private async UniTask<GameObject> LoadUIPrefab(string uiPanelName)
        {
            if (prefabCache.TryGetValue(uiPanelName, out var cached)) return cached;

            if (uiResourceLoader == null)
            {
                Debug.LogError("UIResourceLoader 未注入，无法加载UI资源");
                return null;
            }

            var prefab = await uiResourceLoader.LoadUIPrefabAsync(uiPanelName);
            if (prefab != null) prefabCache[uiPanelName] = prefab;
            return prefab;
        }

        private async UniTask<IUIPanel> CreateUIInstance(GameObject prefab, Type type)
        {
            var instance = Instantiate(prefab);
            var comp = instance.GetComponent(type) ?? instance.AddComponent(type);
            await UniTask.CompletedTask;
            return comp as IUIPanel;
        }

        private void SetupUILayer(IUIPanel panel)
        {
            var layer = panel.Layer;

            if (panel is MonoBehaviour mb)
            {
                if (layerParents.TryGetValue(layer, out var parent))
                    mb.transform.SetParent(parent, false);

                var canvas = mb.GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = mb.gameObject.AddComponent<Canvas>();
                    mb.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }

                canvas.overrideSorting = true;
                canvas.sortingOrder = (int)layer;
            }
        }
        #endregion

        #region Unity生命周期 & 摄像机（原样保留）
        private void OnDestroy()
        {
            UIManagerService.UnregisterManager(this);
            uiResourceLoader?.Dispose();
            prefabCache.Clear();
            activePanels.Clear();
            layerParents.Clear();
        }

        private void InitializeUICamera()
        {
            if (uiCamera != null) { RegisterUICamera(uiCamera); return; }
            if (cameraManager == null) { Debug.LogWarning("[UISystem] 摄像机管理器未注入，延后初始化UI摄像机"); return; }

            var existing = cameraManager.GetCamera(CameraType.UI);
            if (existing != null)
            {
                uiCamera = existing;
                SetupUICanvas(uiCamera);
                Debug.Log("[UISystem] 使用现有UI摄像机");
                return;
            }

            CreateUICamera();
        }

        private void CreateUICamera()
        {
            if (cameraManager == null) return;

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

        private void RegisterUICamera(Camera camera)
        {
            if (camera == null || cameraManager == null) return;

            var config = new CameraConfig(CameraType.UI);
            config.ApplyTo(camera);

            cameraManager.RegisterCamera(camera, config);
            SetupUICanvas(camera);

            Debug.Log($"[UISystem] 注册UI摄像机: {camera.name}");
        }

        private void SetupUICanvas(Camera camera)
        {
            if (uiRoot == null || camera == null) return;

            var canvas = uiRoot.GetComponent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceCamera)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 100f;
                Debug.Log("[UISystem] UI Canvas切换到Camera渲染模式");
            }
        }

        public Camera GetUICamera() => uiCamera;

        public void SetUICamera(Camera camera)
        {
            if (camera == null) return;
            uiCamera = camera;
            RegisterUICamera(camera);
        }

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
                    if (uiCamera != null) canvas.worldCamera = uiCamera;
                    Debug.Log("[UISystem] 切换到WorldSpace渲染模式");
                    break;
            }
        }
        #endregion

        #region IUIContainer 实现（原样保留）
        public void RegisterPanel(string panelName, IUIPanel panel)
        {
            if (panelName == null) throw new ArgumentNullException(nameof(panelName));
            if (panel == null) throw new ArgumentNullException(nameof(panel));
            activePanels[panelName] = panel;
            Debug.Log($"[UISystem] 注册UI面板: {panelName}");
        }

        public void UnregisterPanel(string panelName)
        {
            if (panelName == null) return;
            if (activePanels.Remove(panelName))
                Debug.Log($"[UISystem] 注销UI面板: {panelName}");
        }

        public IUIPanel GetPanel(string panelName)
        {
            if (panelName == null) return null;
            return activePanels.TryGetValue(panelName, out var panel) ? panel : null;
        }

        public IEnumerable<IUIPanel> GetAllPanels() => activePanels.Values;

        public bool HasPanel(string panelName)
        {
            if (panelName == null) return false;
            return activePanels.ContainsKey(panelName);
        }

        public void Clear()
        {
            activePanels.Clear();
            Debug.Log("[UISystem] 清空所有UI面板注册");
        }
        #endregion
    }
}
