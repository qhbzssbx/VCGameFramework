using System;
using Cysharp.Threading.Tasks;

namespace Game.Core.UI
{
    /// <summary>
    /// UI管理器接口
    /// 定义UI系统的核心操作，支持面板的显示、隐藏和状态查询
    /// </summary>
    public interface IUIManager
    {
        /// <summary>
        /// 异步显示UI面板
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        /// <param name="assetKey">资源键名</param>
        /// <param name="args">传入参数</param>
        /// <returns>UI面板实例</returns>
        //UniTask<IUIPanel> ShowAsync(string assetKey, params object[] args);
        UniTask<IUIPanel> ShowAsync(Type type, params object[] args);
        
        /// <summary>
        /// 异步隐藏UI面板
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        UniTask HideAsync<T>() where T : class, IUIPanel;
        
        /// <summary>
        /// 异步隐藏所有UI面板
        /// </summary>
        UniTask HideAllAsync();
        
        /// <summary>
        /// 获取UI面板实例
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        /// <returns>UI面板实例，如果不存在返回null</returns>
        T GetPanel<T>() where T : class, IUIPanel;
        
        /// <summary>
        /// 检查UI面板是否正在显示
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        /// <returns>是否正在显示</returns>
        bool IsShowing<T>() where T : class, IUIPanel;
        
        /// <summary>
        /// 异步切换UI面板显示状态
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        /// <param name="assetKey">资源键名</param>
        /// <param name="args">传入参数</param>
        UniTask ToggleAsync<T>(Type type, params object[] args) where T : class, IUIPanel;
    }
}