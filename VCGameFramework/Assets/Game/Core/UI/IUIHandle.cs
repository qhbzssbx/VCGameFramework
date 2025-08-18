using System;
using Cysharp.Threading.Tasks;

namespace Game.Core.UI
{
    /// <summary>
    /// UI控制句柄接口
    /// 提供UI面板的关闭控制能力，实现解耦的关闭逻辑
    /// </summary>
    public interface IUIHandle
    {
        /// <summary>
        /// 是否可以关闭
        /// </summary>
        bool CanClose { get; }
        
        /// <summary>
        /// 同步关闭UI
        /// </summary>
        void Close();
        
        /// <summary>
        /// 异步关闭UI
        /// </summary>
        UniTask CloseAsync();
        
        /// <summary>
        /// 带返回值的异步关闭UI
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="result">返回值</param>
        UniTask<TResult> CloseWithResultAsync<TResult>(TResult result);
        
        /// <summary>
        /// 关闭请求事件
        /// </summary>
        event Action<IUIHandle> OnCloseRequested;
    }
}