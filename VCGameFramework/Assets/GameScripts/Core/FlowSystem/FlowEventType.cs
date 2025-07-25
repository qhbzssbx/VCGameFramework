namespace Game.Core.FlowSystem
{
    /// <summary>
    /// 流程事件类型枚举
    /// </summary>
    public enum FlowEventType
    {
        /// <summary>
        /// 流程进入事件
        /// </summary>
        FlowEntered,
        
        /// <summary>
        /// 流程退出事件
        /// </summary>
        FlowExited,
        
        /// <summary>
        /// 主流程切换事件
        /// </summary>
        MainFlowSwitched,
        
        /// <summary>
        /// 子流程压入事件
        /// </summary>
        SubFlowPushed,
        
        /// <summary>
        /// 子流程弹出事件
        /// </summary>
        SubFlowPopped,
        
        /// <summary>
        /// 流程错误事件
        /// </summary>
        FlowError,
        
        /// <summary>
        /// 流程暂停事件
        /// </summary>
        FlowPaused,
        
        /// <summary>
        /// 流程恢复事件
        /// </summary>
        FlowResumed
    }
}