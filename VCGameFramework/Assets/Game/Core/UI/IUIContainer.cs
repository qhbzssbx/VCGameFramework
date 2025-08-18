using System;
using System.Collections.Generic;

namespace Game.Core.UI
{
    /// <summary>
    /// UI容器管理接口
    /// 负责UI面板的注册、注销和查询管理
    /// </summary>
    public interface IUIContainer
    {
        /// <summary>
        /// 注册UI面板
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <param name="panel">面板实例</param>
        void RegisterPanel(Type panelType, IUIPanel panel);
        
        /// <summary>
        /// 注销UI面板
        /// </summary>
        /// <param name="panelType">面板类型</param>
        void UnregisterPanel(Type panelType);
        
        /// <summary>
        /// 获取指定类型的UI面板
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <returns>面板实例，如果不存在返回null</returns>
        IUIPanel GetPanel(Type panelType);
        
        /// <summary>
        /// 获取指定类型的UI面板（泛型版本）
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        /// <returns>面板实例，如果不存在返回null</returns>
        T GetPanel<T>() where T : class, IUIPanel;
        
        /// <summary>
        /// 获取所有注册的UI面板
        /// </summary>
        /// <returns>所有面板实例的枚举</returns>
        IEnumerable<IUIPanel> GetAllPanels();
        
        /// <summary>
        /// 检查指定类型的面板是否已注册
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <returns>是否已注册</returns>
        bool HasPanel(Type panelType);
        
        /// <summary>
        /// 清空所有注册的面板
        /// </summary>
        void Clear();
    }
}