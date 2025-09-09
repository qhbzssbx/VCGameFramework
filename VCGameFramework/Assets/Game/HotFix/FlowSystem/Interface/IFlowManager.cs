using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Game.HotFix.FlowSystem.Interface
{
    /// <summary>
    /// 主流程管理器接口
    /// </summary>
    public interface IFlowManager : IDisposable
    {
        /// <summary>
        /// 当前活跃的主流程
        /// </summary>
        IMainFlow CurrentFlow { get; }
        
        /// <summary>
        /// 流程切换历史
        /// </summary>
        IReadOnlyList<Type> FlowHistory { get; }
        
        /// <summary>
        /// 切换到指定类型的流程
        /// </summary>
        /// <typeparam name="T">流程类型</typeparam>
        /// <param name="context">流程上下文</param>
        /// <returns>异步任务</returns>
        UniTask SwitchToFlow<T>(FlowContext context = null) where T : class, IMainFlow;
        
        /// <summary>
        /// 切换到指定类型的流程
        /// </summary>
        /// <param name="flowType">流程类型</param>
        /// <param name="context">流程上下文</param>
        /// <returns>异步任务</returns>
        UniTask SwitchToFlow(Type flowType, FlowContext context = null);
        
        /// <summary>
        /// 检查指定类型的流程是否处于活跃状态
        /// </summary>
        /// <typeparam name="T">流程类型</typeparam>
        /// <returns>如果活跃返回true，否则返回false</returns>
        bool IsFlowActive<T>() where T : class, IMainFlow;
        
        /// <summary>
        /// 检查指定类型的流程是否处于活跃状态
        /// </summary>
        /// <param name="flowType">流程类型</param>
        /// <returns>如果活跃返回true，否则返回false</returns>
        bool IsFlowActive(Type flowType);
        
        /// <summary>
        /// 注册流程实例
        /// </summary>
        /// <typeparam name="T">流程类型</typeparam>
        /// <param name="flow">流程实例</param>
        void RegisterFlow<T>(T flow) where T : class, IMainFlow;
        
        /// <summary>
        /// 获取指定类型的流程实例
        /// </summary>
        /// <typeparam name="T">流程类型</typeparam>
        /// <returns>流程实例，如果不存在返回null</returns>
        T GetFlow<T>() where T : class, IMainFlow;
        
        /// <summary>
        /// 获取指定类型的流程实例
        /// </summary>
        /// <param name="flowType">流程类型</param>
        /// <returns>流程实例，如果不存在返回null</returns>
        IMainFlow GetFlow(Type flowType);
        
        /// <summary>
        /// 停止所有流程并清理
        /// </summary>
        /// <returns>异步任务</returns>
        UniTask Shutdown();
    }
}