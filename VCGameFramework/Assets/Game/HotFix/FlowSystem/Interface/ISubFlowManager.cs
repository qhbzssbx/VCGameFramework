using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Game.HotFix.FlowSystem.Interface
{
    /// <summary>
    /// 子流程管理器接口
    /// </summary>
    public interface ISubFlowManager
    {
        /// <summary>
        /// 当前活跃的子流程
        /// </summary>
        ISubFlow CurrentSubFlow { get; }
        
        /// <summary>
        /// 父主流程引用
        /// </summary>
        IMainFlow ParentMainFlow { get; set; }
        
        /// <summary>
        /// 子流程栈，只读访问
        /// </summary>
        IReadOnlyList<ISubFlow> SubFlowStack { get; }
        
        /// <summary>
        /// 子流程栈深度
        /// </summary>
        int StackDepth { get; }
        
        /// <summary>
        /// 压入子流程到栈顶
        /// </summary>
        /// <typeparam name="T">子流程类型</typeparam>
        /// <param name="context">流程上下文</param>
        /// <returns>异步任务</returns>
        UniTask PushSubFlow<T>(FlowContext context = null) where T : class, ISubFlow;
        
        /// <summary>
        /// 压入子流程到栈顶
        /// </summary>
        /// <param name="subFlowType">子流程类型</param>
        /// <param name="context">流程上下文</param>
        /// <returns>异步任务</returns>
        UniTask PushSubFlow(Type subFlowType, FlowContext context = null);
        
        /// <summary>
        /// 弹出栈顶子流程
        /// </summary>
        /// <returns>异步任务</returns>
        UniTask PopSubFlow();
        
        /// <summary>
        /// 弹出所有子流程，返回到根状态
        /// </summary>
        /// <returns>异步任务</returns>
        UniTask PopToRoot();
        
        /// <summary>
        /// 弹出到指定类型的子流程
        /// </summary>
        /// <typeparam name="T">目标子流程类型</typeparam>
        /// <returns>异步任务</returns>
        UniTask PopToSubFlow<T>() where T : class, ISubFlow;
        
        /// <summary>
        /// 检查指定类型的子流程是否在栈中
        /// </summary>
        /// <typeparam name="T">子流程类型</typeparam>
        /// <returns>如果在栈中返回true，否则返回false</returns>
        bool IsSubFlowInStack<T>() where T : class, ISubFlow;
        
        /// <summary>
        /// 检查指定类型的子流程是否为当前活跃流程
        /// </summary>
        /// <typeparam name="T">子流程类型</typeparam>
        /// <returns>如果是当前活跃流程返回true，否则返回false</returns>
        bool IsCurrentSubFlow<T>() where T : class, ISubFlow;
        
        /// <summary>
        /// 注册子流程实例
        /// </summary>
        /// <typeparam name="T">子流程类型</typeparam>
        /// <param name="subFlow">子流程实例</param>
        void RegisterSubFlow<T>(T subFlow) where T : class, ISubFlow;
        
        /// <summary>
        /// 获取指定类型的子流程实例
        /// </summary>
        /// <typeparam name="T">子流程类型</typeparam>
        /// <returns>子流程实例，如果不存在返回null</returns>
        T GetSubFlow<T>() where T : class, ISubFlow;
        
        /// <summary>
        /// 清理所有子流程
        /// </summary>
        /// <returns>异步任务</returns>
        UniTask Clear();
    }
}