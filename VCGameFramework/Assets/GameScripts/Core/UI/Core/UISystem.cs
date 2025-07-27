using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using YooAsset;
using Game.Infrastructure.Resource.Core;

namespace GameScript.Core.UI.Core
{
    /// <summary>
    /// UI系统管理器
    /// 负责UI的生命周期管理、层级控制和资源加载
    /// </summary>
    public class UISystem : MonoBehaviour
    {
        public static UISystem Instance { get; private set; }
        
        [Header("UI系统配置")]
        [SerializeField] private Transform uiRoot;
        [SerializeField] private Camera uiCamera;
        
        /// <summary>
        /// 各层级的父节点
        /// </summary>
        private readonly Dictionary<UILayer, Transform> layerParents = new();
        
        /// <summary>
        /// 当前显示的UI面板
        /// </summary>
        private readonly Dictionary<Type, UIPanel> activePanels = new();
        
        /// <summary>
        /// UI预制体缓存
        /// </summary>
        private readonly Dictionary<Type, GameObject> prefabCache = new();
        
        /// <summary>
        /// 资源加载器
        /// </summary>
        private readonly ResourceLoader resourceLoader = new();
        
        #region 初始化
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeUISystem();
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
            // 创建UI根节点
            if (uiRoot == null)
            {
                var rootGO = new GameObject("UIRoot");
                rootGO.transform.SetParent(transform);
                uiRoot = rootGO.transform;
                
                // 添加Canvas组件
                var canvas = rootGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.pixelPerfect = false;
                
                // 添加CanvasScaler
                var scaler = rootGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                
                // 添加GraphicRaycaster
                rootGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
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
                layerGO.transform.SetParent(uiRoot, false);
                
                // 设置Canvas组件用于层级控制
                var canvas = layerGO.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = (int)layer;
                
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
        public async UniTask<T> Show<T>(string assetKey, params object[] args) where T : UIPanel
        {
            var panelType = typeof(T);
            
            // 如果UI已经存在且正在显示，直接返回
            if (activePanels.TryGetValue(panelType, out var existingPanel) && existingPanel.IsShowing)
            {
                return existingPanel as T;
            }
            
            // 加载UI预制体
            var prefab = await LoadUIPrefab<T>(assetKey);
            if (prefab == null)
            {
                Debug.LogError($"UI预制体加载失败: {assetKey}");
                return null;
            }
            
            // 创建UI实例
            var panelInstance = await CreateUIInstance<T>(prefab);
            if (panelInstance == null)
            {
                Debug.LogError($"UI实例创建失败: {panelType.Name}");
                return null;
            }
            
            // 设置UI层级
            SetupUILayer(panelInstance);
            
            // 注册到管理器
            activePanels[panelType] = panelInstance;
            
            // 显示UI
            await panelInstance.Show(args);
            
            return panelInstance;
        }
        
        /// <summary>
        /// 隐藏UI面板
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        public async UniTask Hide<T>() where T : UIPanel
        {
            var panelType = typeof(T);
            
            if (activePanels.TryGetValue(panelType, out var panel))
            {
                await panel.Hide();
                
                // 如果设置为自动销毁，从管理器中移除
                if (panel.AutoDestroy)
                {
                    activePanels.Remove(panelType);
                }
            }
        }
        
        /// <summary>
        /// 获取UI面板实例
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        /// <returns>UI面板实例，如果不存在返回null</returns>
        public T Get<T>() where T : UIPanel
        {
            var panelType = typeof(T);
            return activePanels.TryGetValue(panelType, out var panel) ? panel as T : null;
        }
        
        /// <summary>
        /// 检查UI是否正在显示
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        /// <returns>是否正在显示</returns>
        public bool IsShowing<T>() where T : UIPanel
        {
            var panel = Get<T>();
            return panel != null && panel.IsShowing;
        }
        
        /// <summary>
        /// 切换UI显示状态
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        /// <param name="assetKey">资源键名</param>
        /// <param name="args">传入参数</param>
        public async UniTask Toggle<T>(string assetKey, params object[] args) where T : UIPanel
        {
            if (IsShowing<T>())
            {
                await Hide<T>();
            }
            else
            {
                await Show<T>(assetKey, args);
            }
        }
        
        /// <summary>
        /// 关闭所有UI面板
        /// </summary>
        public async UniTask HideAll()
        {
            var hideList = new List<UniTask>();
            
            foreach (var panel in activePanels.Values)
            {
                if (panel.IsShowing)
                {
                    hideList.Add(panel.Hide());
                }
            }
            
            await UniTask.WhenAll(hideList);
            activePanels.Clear();
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 加载UI预制体
        /// </summary>
        private async UniTask<GameObject> LoadUIPrefab<T>(string assetKey) where T : UIPanel
        {
            var panelType = typeof(T);
            
            // 检查缓存
            if (prefabCache.TryGetValue(panelType, out var cachedPrefab))
            {
                return cachedPrefab;
            }
            
            // 加载资源
            var handle = await resourceLoader.LoadAssetAsync<GameObject>(assetKey);
            if (handle.IsValid && handle.AssetObject != null)
            {
                var prefab = handle.AssetObject as GameObject;
                prefabCache[panelType] = prefab;
                return prefab;
            }
            
            return null;
        }
        
        /// <summary>
        /// 创建UI实例
        /// </summary>
        private async UniTask<T> CreateUIInstance<T>(GameObject prefab) where T : UIPanel
        {
            var instance = Instantiate(prefab);
            
            // 尝试获取或添加UI组件
            var panelComponent = instance.GetComponent<T>();
            if (panelComponent == null)
            {
                panelComponent = instance.AddComponent<T>();
            }
            
            await UniTask.CompletedTask;
            return panelComponent;
        }
        
        /// <summary>
        /// 设置UI层级
        /// </summary>
        private void SetupUILayer(UIPanel panel)
        {
            var layer = panel.Layer;
            
            // 设置父节点
            if (layerParents.TryGetValue(layer, out var parent))
            {
                panel.transform.SetParent(parent, false);
            }
            
            // 设置Canvas排序
            var canvas = panel.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = panel.gameObject.AddComponent<Canvas>();
                panel.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            
            canvas.overrideSorting = true;
            canvas.sortingOrder = (int)layer;
        }
        
        #endregion
        
        #region Unity生命周期
        
        private void OnDestroy()
        {
            // 清理资源
            resourceLoader?.Dispose();
            
            // 清理缓存
            prefabCache.Clear();
            activePanels.Clear();
            layerParents.Clear();
        }
        
        #endregion
    }
}