using System;
using Game.HotFix.FlowSystem.Interface;
using Game.HotFix.FlowSystem.Manager;

namespace Game.HotFix.FlowSystem.Event
{
    /// <summary>
    /// 流程事件数据结构
    /// </summary>
    public readonly struct FlowEvent
    {
        /// <summary>
        /// 事件类型
        /// </summary>
        public readonly FlowEventType EventType;
        
        /// <summary>
        /// 流程类型
        /// </summary>
        public readonly Type FlowType;
        
        /// <summary>
        /// 流程名称
        /// </summary>
        public readonly string FlowName;
        
        /// <summary>
        /// 流程上下文数据
        /// </summary>
        public readonly FlowContext Context;
        
        /// <summary>
        /// 事件时间戳
        /// </summary>
        public readonly DateTime Timestamp;
        
        /// <summary>
        /// 错误信息（仅在FlowError事件时有效）
        /// </summary>
        public readonly Exception Error;
        
        /// <summary>
        /// 创建流程事件
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="flowType">流程类型</param>
        /// <param name="flowName">流程名称</param>
        /// <param name="context">流程上下文</param>
        /// <param name="error">错误信息</param>
        public FlowEvent(FlowEventType eventType, Type flowType, string flowName, FlowContext context = null, Exception error = null)
        {
            EventType = eventType;
            FlowType = flowType;
            FlowName = flowName;
            Context = context;
            Timestamp = DateTime.Now;
            Error = error;
        }
        
        /// <summary>
        /// 创建流程进入事件
        /// </summary>
        /// <param name="flow">流程实例</param>
        /// <param name="context">流程上下文</param>
        /// <returns>流程事件</returns>
        public static FlowEvent CreateEntered(IFlow flow, FlowContext context = null)
        {
            return new FlowEvent(FlowEventType.FlowEntered, flow.GetType(), flow.FlowName, context);
        }
        
        /// <summary>
        /// 创建流程退出事件
        /// </summary>
        /// <param name="flow">流程实例</param>
        /// <param name="context">流程上下文</param>
        /// <returns>流程事件</returns>
        public static FlowEvent CreateExited(IFlow flow, FlowContext context = null)
        {
            return new FlowEvent(FlowEventType.FlowExited, flow.GetType(), flow.FlowName, context);
        }
        
        /// <summary>
        /// 创建主流程切换事件
        /// </summary>
        /// <param name="fromFlow">源流程</param>
        /// <param name="toFlow">目标流程</param>
        /// <param name="context">流程上下文</param>
        /// <returns>流程事件</returns>
        public static FlowEvent CreateMainFlowSwitched(IMainFlow fromFlow, IMainFlow toFlow, FlowContext context = null)
        {
            var eventContext = FlowContextBuilder.Create()
                .WithTypedData(fromFlow)
                .WithTypedData(toFlow)
                .WithData("FromFlowName", fromFlow?.FlowName ?? "None")
                .WithData("ToFlowName", toFlow?.FlowName ?? "None")
                .Build();
                
            if (context != null)
            {
                // 合并传入的上下文数据
                var mergedContext = context.CreateChild();
                mergedContext.SetTyped(fromFlow);
                mergedContext.SetTyped(toFlow);
                mergedContext.Set("FromFlowName", fromFlow?.FlowName ?? "None");
                mergedContext.Set("ToFlowName", toFlow?.FlowName ?? "None");
                eventContext = mergedContext;
            }
            
            return new FlowEvent(FlowEventType.MainFlowSwitched, toFlow?.GetType(), toFlow?.FlowName ?? "Unknown", eventContext);
        }
        
        /// <summary>
        /// 创建子流程压入事件
        /// </summary>
        /// <param name="subFlow">子流程实例</param>
        /// <param name="context">流程上下文</param>
        /// <returns>流程事件</returns>
        public static FlowEvent CreateSubFlowPushed(ISubFlow subFlow, FlowContext context = null)
        {
            return new FlowEvent(FlowEventType.SubFlowPushed, subFlow.GetType(), subFlow.FlowName, context);
        }
        
        /// <summary>
        /// 创建子流程弹出事件
        /// </summary>
        /// <param name="subFlow">子流程实例</param>
        /// <param name="context">流程上下文</param>
        /// <returns>流程事件</returns>
        public static FlowEvent CreateSubFlowPopped(ISubFlow subFlow, FlowContext context = null)
        {
            return new FlowEvent(FlowEventType.SubFlowPopped, subFlow.GetType(), subFlow.FlowName, context);
        }
        
        /// <summary>
        /// 创建流程错误事件
        /// </summary>
        /// <param name="flow">流程实例</param>
        /// <param name="error">错误信息</param>
        /// <param name="context">流程上下文</param>
        /// <returns>流程事件</returns>
        public static FlowEvent CreateError(IFlow flow, Exception error, FlowContext context = null)
        {
            return new FlowEvent(FlowEventType.FlowError, flow.GetType(), flow.FlowName, context, error);
        }
        
        /// <summary>
        /// 创建流程暂停事件
        /// </summary>
        /// <param name="flow">流程实例</param>
        /// <param name="context">流程上下文</param>
        /// <returns>流程事件</returns>
        public static FlowEvent CreatePaused(IFlow flow, FlowContext context = null)
        {
            return new FlowEvent(FlowEventType.FlowPaused, flow.GetType(), flow.FlowName, context);
        }
        
        /// <summary>
        /// 创建流程恢复事件
        /// </summary>
        /// <param name="flow">流程实例</param>
        /// <param name="context">流程上下文</param>
        /// <returns>流程事件</returns>
        public static FlowEvent CreateResumed(IFlow flow, FlowContext context = null)
        {
            return new FlowEvent(FlowEventType.FlowResumed, flow.GetType(), flow.FlowName, context);
        }
        
        /// <summary>
        /// 转换为字符串表示
        /// </summary>
        /// <returns>字符串表示</returns>
        public override string ToString()
        {
            var errorInfo = Error != null ? $" Error: {Error.Message}" : "";
            return $"[{Timestamp:HH:mm:ss.fff}] {EventType}: {FlowName} ({FlowType?.Name}){errorInfo}";
        }
    }
}