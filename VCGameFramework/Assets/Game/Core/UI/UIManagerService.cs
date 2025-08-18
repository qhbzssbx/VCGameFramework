using UnityEngine;

namespace Game.Core.UI
{
    /// <summary>
    /// UI管理器服务定位器
    /// 提供全局访问UI管理器的统一入口，支持运行时实现切换
    /// </summary>
    public static class UIManagerService
    {
        private static IUIManager _instance;
        
        /// <summary>
        /// 当前UI管理器实例
        /// </summary>
        public static IUIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogWarning("[UIManagerService] UI管理器未注册，请确保在使用前调用 RegisterManager 进行注册");
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// 检查是否已注册UI管理器
        /// </summary>
        public static bool IsRegistered => _instance != null;
        
        /// <summary>
        /// 注册UI管理器实现
        /// </summary>
        /// <param name="manager">UI管理器实例</param>
        public static void RegisterManager(IUIManager manager)
        {
            if (_instance != null && _instance != manager)
            {
                Debug.LogWarning("[UIManagerService] 正在替换现有的UI管理器实现");
            }
            
            _instance = manager;
            Debug.Log($"[UIManagerService] UI管理器已注册: {manager?.GetType().Name}");
        }
        
        /// <summary>
        /// 注销UI管理器
        /// </summary>
        public static void UnregisterManager()
        {
            if (_instance != null)
            {
                Debug.Log($"[UIManagerService] UI管理器已注销: {_instance.GetType().Name}");
                _instance = null;
            }
        }
        
        /// <summary>
        /// 注销指定的UI管理器（安全检查）
        /// </summary>
        /// <param name="manager">要注销的管理器实例</param>
        public static void UnregisterManager(IUIManager manager)
        {
            if (_instance == manager)
            {
                UnregisterManager();
            }
        }
    }
}