using MessagePipe;

namespace Game.Core.FlowSystem
{
    /// <summary>
    /// 流程事件发布器实现，集成MessagePipe
    /// </summary>
    public class FlowEventPublisher : IFlowEventPublisher
    {
        private readonly IPublisher<FlowEvent> _publisher;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="publisher">MessagePipe发布器</param>
        public FlowEventPublisher(IPublisher<FlowEvent> publisher)
        {
            _publisher = publisher;
        }
        
        /// <summary>
        /// 发布流程事件
        /// </summary>
        /// <param name="flowEvent">流程事件</param>
        public void Publish(FlowEvent flowEvent)
        {
            _publisher.Publish(flowEvent);
        }
        
        /// <summary>
        /// 发布流程进入事件
        /// </summary>
        /// <param name="flow">流程实例</param>
        /// <param name="context">流程上下文</param>
        public void PublishEntered(IFlow flow, FlowContext context = null)
        {
            var flowEvent = FlowEvent.CreateEntered(flow, context);
            _publisher.Publish(flowEvent);
        }
        
        /// <summary>
        /// 发布流程退出事件
        /// </summary>
        /// <param name="flow">流程实例</param>
        /// <param name="context">流程上下文</param>
        public void PublishExited(IFlow flow, FlowContext context = null)
        {
            var flowEvent = FlowEvent.CreateExited(flow, context);
            _publisher.Publish(flowEvent);
        }
        
        /// <summary>
        /// 发布主流程切换事件
        /// </summary>
        /// <param name="fromFlow">源流程</param>
        /// <param name="toFlow">目标流程</param>
        /// <param name="context">流程上下文</param>
        public void PublishMainFlowSwitched(IMainFlow fromFlow, IMainFlow toFlow, FlowContext context = null)
        {
            var flowEvent = FlowEvent.CreateMainFlowSwitched(fromFlow, toFlow, context);
            _publisher.Publish(flowEvent);
        }
        
        /// <summary>
        /// 发布子流程压入事件
        /// </summary>
        /// <param name="subFlow">子流程实例</param>
        /// <param name="context">流程上下文</param>
        public void PublishSubFlowPushed(ISubFlow subFlow, FlowContext context = null)
        {
            var flowEvent = FlowEvent.CreateSubFlowPushed(subFlow, context);
            _publisher.Publish(flowEvent);
        }
        
        /// <summary>
        /// 发布子流程弹出事件
        /// </summary>
        /// <param name="subFlow">子流程实例</param>
        /// <param name="context">流程上下文</param>
        public void PublishSubFlowPopped(ISubFlow subFlow, FlowContext context = null)
        {
            var flowEvent = FlowEvent.CreateSubFlowPopped(subFlow, context);
            _publisher.Publish(flowEvent);
        }
        
        /// <summary>
        /// 发布流程错误事件
        /// </summary>
        /// <param name="flow">流程实例</param>
        /// <param name="error">错误信息</param>
        /// <param name="context">流程上下文</param>
        public void PublishError(IFlow flow, System.Exception error, FlowContext context = null)
        {
            var flowEvent = FlowEvent.CreateError(flow, error, context);
            _publisher.Publish(flowEvent);
        }
        
        /// <summary>
        /// 发布流程暂停事件
        /// </summary>
        /// <param name="flow">流程实例</param>
        /// <param name="context">流程上下文</param>
        public void PublishPaused(IFlow flow, FlowContext context = null)
        {
            var flowEvent = FlowEvent.CreatePaused(flow, context);
            _publisher.Publish(flowEvent);
        }
        
        /// <summary>
        /// 发布流程恢复事件
        /// </summary>
        /// <param name="flow">流程实例</param>
        /// <param name="context">流程上下文</param>
        public void PublishResumed(IFlow flow, FlowContext context = null)
        {
            var flowEvent = FlowEvent.CreateResumed(flow, context);
            _publisher.Publish(flowEvent);
        }
    }
}