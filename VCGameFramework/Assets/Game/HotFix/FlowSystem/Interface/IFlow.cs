using Cysharp.Threading.Tasks;

namespace Game.HotFix.FlowSystem.Interface
{
    /// <summary>
    /// 流程基础接口，定义所有流程的基本生命周期
    /// </summary>
    public interface IFlow
    {
        /// <summary>
        /// 流程名称，用于标识和调试
        /// </summary>
        string FlowName { get; }
        
        /// <summary>
        /// 流程是否处于活跃状态
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// 进入流程时调用，执行初始化逻辑
        /// </summary>
        /// <param name="context">流程上下文数据</param>
        /// <returns>异步任务</returns>
        UniTask OnEnter(FlowContext context = null);
        
        /// <summary>
        /// 流程更新循环，在流程活跃期间持续调用（可选实现）
        /// </summary>
        /// <returns>异步任务</returns>
        UniTask OnUpdate();
        
        /// <summary>
        /// 退出流程时调用，执行清理逻辑
        /// </summary>
        /// <returns>异步任务</returns>
        UniTask OnExit();
    }
}