using Cysharp.Threading.Tasks;

namespace Game.Core.UI
{
    /// <summary>
    /// UI面板接口
    /// 定义UI面板的基础行为和属性
    /// </summary>
    public interface IUIPanel
    {
        /// <summary>
        /// UI所属层级
        /// </summary>
        UILayer Layer { get; }
        
        /// <summary>
        /// UI是否正在显示
        /// </summary>
        bool IsShowing { get; }
        
        /// <summary>
        /// 异步隐藏UI
        /// </summary>
        UniTask HideAsync();
        
        /// <summary>
        /// 设置UI控制句柄
        /// </summary>
        /// <param name="handle">UI控制句柄</param>
        void SetHandle(IUIHandle handle);
    }

    public interface IUIPanel<TParams> : IUIPanel where TParams : struct, IUIParams
    {
        /// <summary>
        /// 异步显示UI
        /// </summary>
        /// <param name="args">传入参数</param>
        UniTask ShowAsync(in TParams args);
        
        /// <summary>
        /// 异步开启UI, 已存在则刷新参数
        /// </summary>
        /// <param name="args">传入参数</param>
        UniTask OpenAsync(in TParams args);
    }
    

}