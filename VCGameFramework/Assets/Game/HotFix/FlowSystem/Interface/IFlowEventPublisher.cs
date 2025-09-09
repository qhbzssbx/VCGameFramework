using Game.HotFix.FlowSystem.Event;

namespace Game.HotFix.FlowSystem.Interface
{
    /// <summary>
    /// 流程事件发布器接口
    /// </summary>
    public interface IFlowEventPublisher
    {
        /// <summary>
        /// 发布流程事件
        /// </summary>
        /// <param name="flowEvent">流程事件</param>
        void Publish(FlowEvent flowEvent);
        
        /// <summary>
        /// 发布流程进入事件
        /// </summary>
        /// <param name="flow">流程实例</param>
        /// <param name="context">流程上下文</param>
        void PublishEntered(IFlow flow, FlowContext context = null);
        
        /// <summary>
        /// 发布流程退出事件
        /// </summary>
        /// <param name="flow">流程实例</param>
        /// <param name="context">流程上下文</param>
        void PublishExited(IFlow flow, FlowContext context = null);
        
        /// <summary>
        /// 发布主流程切换事件
        /// </summary>
        /// <param name="fromFlow">源流程</param>
        /// <param name="toFlow">目标流程</param>
        /// <param name="context">流程上下文</param>
        void PublishMainFlowSwitched(IMainFlow fromFlow, IMainFlow toFlow, FlowContext context = null);
        
        /// <summary>
        /// 发布子流程压入事件
        /// </summary>
        /// <param name="subFlow">子流程实例</param>
        /// <param name="context">流程上下文</param>
        void PublishSubFlowPushed(ISubFlow subFlow, FlowContext context = null);
        
        /// <summary>
        /// 发布子流程弹出事件
        /// </summary>
        /// <param name="subFlow">子流程实例</param>
        /// <param name="context">流程上下文</param>
        void PublishSubFlowPopped(ISubFlow subFlow, FlowContext context = null);
        
        /// <summary>
        /// 发布流程错误事件
        /// </summary>
        /// <param name="flow">流程实例</param>
        /// <param name="error">错误信息</param>
        /// <param name="context">流程上下文</param>
        void PublishError(IFlow flow, System.Exception error, FlowContext context = null);
        
        /// <summary>
        /// 发布流程暂停事件
        /// </summary>
        /// <param name="flow">流程实例</param>
        /// <param name="context">流程上下文</param>
        void PublishPaused(IFlow flow, FlowContext context = null);
        
        /// <summary>
        /// 发布流程恢复事件
        /// </summary>
        /// <param name="flow">流程实例</param>
        /// <param name="context">流程上下文</param>
        void PublishResumed(IFlow flow, FlowContext context = null);
    }
}