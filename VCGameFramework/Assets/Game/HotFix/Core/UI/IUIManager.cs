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
        /// <typeparam name="TPanel">UI面板类型</typeparam>
        /// <typeparam name="TParams">UI面板参数类型</typeparam>
        /// <param name="params">传入参数</param>
        /// <returns>UI面板实例</returns>
        UniTask<TPanel> ShowAsync<TPanel, TParams>(TParams @params) 
            where TPanel : class, IUIPanel
            where TParams : struct, IUIParams;

        /// <summary>
        /// 异步显示UI面板
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        /// <param name="params">传入参数</param>
        /// <returns>UI面板实例</returns>
        //UniTask<IUIPanel> ShowAsync(string assetKey, params object[] args);
        UniTask<T> ShowAsync<T>() where T : class, IUIPanel, IUIPanel<EmptyUIParams>;
        
        /// <summary>
        /// 异步隐藏UI面板
        /// </summary>
        /// <typeparam name="TPanel">UI面板类型</typeparam>
        UniTask HideAsync<TPanel>() where TPanel : class, IUIPanel;
        
        /// <summary>
        /// 异步隐藏所有UI面板
        /// </summary>
        UniTask HideAllAsync();
        
        /// <summary>
        /// 获取UI面板实例
        /// </summary>
        /// <typeparam name="TPanel">UI面板类型</typeparam>
        /// <returns>UI面板实例，如果不存在返回null</returns>
        TPanel GetPanel<TPanel>() where TPanel : class, IUIPanel;
        
        /// <summary>
        /// 检查UI面板是否正在显示
        /// </summary>
        /// <typeparam name="TPanel">UI面板类型</typeparam>
        /// <returns>是否正在显示</returns>
        bool IsShowing<TPanel>() where TPanel : class, IUIPanel;
        
        /// <summary>
        /// 异步切换UI面板显示状态
        /// </summary>
        /// <typeparam name="TPanel">UI面板类型</typeparam>
        /// <param name="args">传入参数</param>
        UniTask ToggleAsync<TPanel, TParams>(TParams args) where TPanel : class, IUIPanel<TParams> where TParams : struct, IUIParams;

        /// <summary>
        /// 异步切换UI面板显示状态
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        /// <param name="assetKey">资源键名</param>
        /// <param name="args">传入参数</param>
        UniTask ToggleAsync<T>() where T : class, IUIPanel<EmptyUIParams>;

    }
}