using Cysharp.Threading.Tasks;
using Game.UI.Core;

namespace Game.Core.UI
{
    /// <summary>
    /// UI面板接口
    /// 定义UI面板的基础行为和属性
    /// </summary>
    public interface IUIPanel
    {
        /// <summary>
        /// UI是否正在显示
        /// </summary>
        bool IsShowing { get; }
        
        /// <summary>
        /// UI所属层级
        /// </summary>
        UILayer Layer { get; }
        
        /// <summary>
        /// 异步显示UI
        /// </summary>
        /// <param name="args">传入参数</param>
        UniTask ShowAsync(params object[] args);
        
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
}