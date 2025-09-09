using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.HotFix.FlowSystem.BaseClass;
using Game.HotFix.FlowSystem.Interface;
using UnityEngine;
using VContainer;

namespace Game.HotFix.FlowSystem.Manager
{
    /// <summary>
    /// 子流程管理器实现
    /// </summary>
    public class SubFlowManager : ISubFlowManager, IDisposable
    {
        private readonly Stack<ISubFlow> _subFlowStack = new();
        private readonly Dictionary<Type, ISubFlow> _subFlowRegistry = new();
        private readonly IFlowEventPublisher _eventPublisher;
        private readonly IObjectResolver _container;
        
        private IMainFlow _parentMainFlow;
        private bool _disposed = false;
        private bool _parentPaused = false;
        
        /// <summary>
        /// 当前活跃的子流程
        /// </summary>
        public ISubFlow CurrentSubFlow => _subFlowStack.Count > 0 ? _subFlowStack.Peek() : null;
        
        /// <summary>
        /// 父主流程引用
        /// </summary>
        public IMainFlow ParentMainFlow 
        { 
            get => _parentMainFlow;
            set => _parentMainFlow = value;
        }
        
        /// <summary>
        /// 子流程栈，只读访问
        /// </summary>
        public IReadOnlyList<ISubFlow> SubFlowStack => _subFlowStack.Reverse().ToList().AsReadOnly();
        
        /// <summary>
        /// 子流程栈深度
        /// </summary>
        public int StackDepth => _subFlowStack.Count;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="eventPublisher">事件发布器</param>
        /// <param name="container">依赖注入容器</param>
        public SubFlowManager(IFlowEventPublisher eventPublisher, IObjectResolver container)
        {
            _eventPublisher = eventPublisher;
            _container = container;
        }
        
        /// <summary>
        /// 压入子流程到栈顶
        /// </summary>
        /// <typeparam name="T">子流程类型</typeparam>
        /// <param name="context">流程上下文</param>
        /// <returns>异步任务</returns>
        public async UniTask PushSubFlow<T>(FlowContext context = null) where T : class, ISubFlow
        {
            await PushSubFlow(typeof(T), context);
        }
        
        /// <summary>
        /// 压入子流程到栈顶
        /// </summary>
        /// <param name="subFlowType">子流程类型</param>
        /// <param name="context">流程上下文</param>
        /// <returns>异步任务</returns>
        public async UniTask PushSubFlow(Type subFlowType, FlowContext context = null)
        {
            if (_disposed)
            {
                Debug.LogError("SubFlowManager has been disposed, cannot push sub flows");
                return;
            }
            
            // 验证子流程类型
            if (!typeof(ISubFlow).IsAssignableFrom(subFlowType))
            {
                Debug.LogError($"Type {subFlowType.Name} is not a valid sub flow type");
                return;
            }
            
            var newSubFlow = GetOrCreateSubFlow(subFlowType);
            if (newSubFlow == null)
            {
                Debug.LogError($"Failed to get or create sub flow of type {subFlowType.Name}");
                return;
            }
            
            // 设置父流程引用
            newSubFlow.ParentFlow = _parentMainFlow;
            
            try
            {
                var previousSubFlow = CurrentSubFlow;
                
                // 如果有当前子流程，先暂停它（不退出）
                if (previousSubFlow != null)
                {
                    Debug.Log($"Pausing current sub flow: {previousSubFlow.FlowName}");
                    _eventPublisher?.PublishPaused(previousSubFlow, context);
                }
                
                // 如果新子流程要求暂停父流程，并且父流程还没有被暂停
                if (newSubFlow.ShouldPauseParent && !_parentPaused && _parentMainFlow != null)
                {
                    Debug.Log($"Pausing parent main flow: {_parentMainFlow.FlowName}");
                    _parentPaused = true;
                    _eventPublisher?.PublishPaused(_parentMainFlow, context);
                }
                
                // 压入新子流程到栈
                _subFlowStack.Push(newSubFlow);
                
                // 进入新子流程
                Debug.Log($"Entering sub flow: {newSubFlow.FlowName}");
                await newSubFlow.OnEnter(context);
                
                // 发布子流程压入事件
                _eventPublisher?.PublishSubFlowPushed(newSubFlow, context);
                
                Debug.Log($"Successfully pushed sub flow: {newSubFlow.FlowName} (Stack depth: {StackDepth})");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error pushing sub flow {subFlowType.Name}: {ex.Message}");
                
                // 发布错误事件
                _eventPublisher?.PublishError(newSubFlow, ex, context);
                
                // 如果压入失败，从栈中移除
                if (_subFlowStack.Count > 0 && _subFlowStack.Peek() == newSubFlow)
                {
                    _subFlowStack.Pop();
                }
                
                throw;
            }
        }
        
        /// <summary>
        /// 弹出栈顶子流程
        /// </summary>
        /// <returns>异步任务</returns>
        public async UniTask PopSubFlow()
        {
            if (_disposed)
            {
                Debug.LogError("SubFlowManager has been disposed, cannot pop sub flows");
                return;
            }
            
            if (_subFlowStack.Count == 0)
            {
                Debug.LogWarning("No sub flows to pop");
                return;
            }
            
            var currentSubFlow = _subFlowStack.Pop();
            
            try
            {
                // 退出当前子流程
                Debug.Log($"Exiting sub flow: {currentSubFlow.FlowName}");
                await currentSubFlow.OnExit();
                
                // 发布子流程弹出事件
                _eventPublisher?.PublishSubFlowPopped(currentSubFlow);
                
                // 检查是否需要恢复父流程
                var newCurrentSubFlow = CurrentSubFlow;
                bool shouldResumeParent = true;
                
                if (newCurrentSubFlow != null)
                {
                    // 如果新的当前子流程也需要暂停父流程，则不恢复
                    if (newCurrentSubFlow.ShouldPauseParent)
                    {
                        shouldResumeParent = false;
                    }
                    
                    // 恢复之前暂停的子流程
                    Debug.Log($"Resuming sub flow: {newCurrentSubFlow.FlowName}");
                    _eventPublisher?.PublishResumed(newCurrentSubFlow);
                }
                
                // 如果需要且可能，恢复父流程
                if (shouldResumeParent && _parentPaused && _parentMainFlow != null)
                {
                    Debug.Log($"Resuming parent main flow: {_parentMainFlow.FlowName}");
                    _parentPaused = false;
                    _eventPublisher?.PublishResumed(_parentMainFlow);
                }
                
                Debug.Log($"Successfully popped sub flow: {currentSubFlow.FlowName} (Stack depth: {StackDepth})");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error popping sub flow {currentSubFlow.FlowName}: {ex.Message}");
                
                // 发布错误事件
                _eventPublisher?.PublishError(currentSubFlow, ex);
                
                throw;
            }
        }
        
        /// <summary>
        /// 弹出所有子流程，返回到根状态
        /// </summary>
        /// <returns>异步任务</returns>
        public async UniTask PopToRoot()
        {
            Debug.Log($"Popping all sub flows to root (Current stack depth: {StackDepth})");
            
            while (_subFlowStack.Count > 0)
            {
                await PopSubFlow();
            }
            
            Debug.Log("Successfully popped to root");
        }
        
        /// <summary>
        /// 弹出到指定类型的子流程
        /// </summary>
        /// <typeparam name="T">目标子流程类型</typeparam>
        /// <returns>异步任务</returns>
        public async UniTask PopToSubFlow<T>() where T : class, ISubFlow
        {
            var targetType = typeof(T);
            Debug.Log($"Popping to sub flow: {targetType.Name}");
            
            // 找到目标子流程在栈中的位置
            var stackArray = _subFlowStack.ToArray();
            int targetIndex = -1;
            
            for (int i = 0; i < stackArray.Length; i++)
            {
                if (stackArray[i].GetType() == targetType)
                {
                    targetIndex = i;
                    break;
                }
            }
            
            if (targetIndex == -1)
            {
                Debug.LogWarning($"Sub flow {targetType.Name} not found in stack");
                return;
            }
            
            // 弹出到目标子流程
            for (int i = 0; i < targetIndex; i++)
            {
                await PopSubFlow();
            }
            
            Debug.Log($"Successfully popped to sub flow: {targetType.Name}");
        }
        
        /// <summary>
        /// 检查指定类型的子流程是否在栈中
        /// </summary>
        /// <typeparam name="T">子流程类型</typeparam>
        /// <returns>如果在栈中返回true，否则返回false</returns>
        public bool IsSubFlowInStack<T>() where T : class, ISubFlow
        {
            var targetType = typeof(T);
            return _subFlowStack.Any(subFlow => subFlow.GetType() == targetType);
        }
        
        /// <summary>
        /// 检查指定类型的子流程是否为当前活跃流程
        /// </summary>
        /// <typeparam name="T">子流程类型</typeparam>
        /// <returns>如果是当前活跃流程返回true，否则返回false</returns>
        public bool IsCurrentSubFlow<T>() where T : class, ISubFlow
        {
            var targetType = typeof(T);
            return CurrentSubFlow?.GetType() == targetType && CurrentSubFlow.IsActive;
        }
        
        /// <summary>
        /// 注册子流程实例
        /// </summary>
        /// <typeparam name="T">子流程类型</typeparam>
        /// <param name="subFlow">子流程实例</param>
        public void RegisterSubFlow<T>(T subFlow) where T : class, ISubFlow
        {
            if (subFlow == null)
            {
                Debug.LogError("Cannot register null sub flow");
                return;
            }
            
            var subFlowType = typeof(T);
            _subFlowRegistry[subFlowType] = subFlow;
            
            // 设置父流程引用
            subFlow.ParentFlow = _parentMainFlow;
            
            // 设置事件发布器
            if (subFlow is BaseFlow baseFlow)
            {
                baseFlow.SetEventPublisher(_eventPublisher);
            }
            
            Debug.Log($"Registered sub flow: {subFlow.FlowName} ({subFlowType.Name})");
        }
        
        /// <summary>
        /// 获取指定类型的子流程实例
        /// </summary>
        /// <typeparam name="T">子流程类型</typeparam>
        /// <returns>子流程实例，如果不存在返回null</returns>
        public T GetSubFlow<T>() where T : class, ISubFlow
        {
            var subFlowType = typeof(T);
            _subFlowRegistry.TryGetValue(subFlowType, out var subFlow);
            return subFlow as T;
        }
        
        /// <summary>
        /// 清理所有子流程
        /// </summary>
        /// <returns>异步任务</returns>
        public async UniTask Clear()
        {
            if (_disposed) return;
            
            Debug.Log("Clearing all sub flows");
            
            try
            {
                // 弹出所有子流程
                await PopToRoot();
                
                // 清理注册表
                foreach (var subFlow in _subFlowRegistry.Values)
                {
                    if (subFlow.IsActive)
                    {
                        try
                        {
                            await subFlow.OnExit();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error clearing sub flow {subFlow.FlowName}: {ex.Message}");
                        }
                    }
                }
                
                _subFlowRegistry.Clear();
                _parentPaused = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during SubFlowManager clear: {ex.Message}");
            }
            
            Debug.Log("SubFlowManager cleared");
        }
        
        /// <summary>
        /// 获取或创建子流程实例
        /// </summary>
        /// <param name="subFlowType">子流程类型</param>
        /// <returns>子流程实例</returns>
        private ISubFlow GetOrCreateSubFlow(Type subFlowType)
        {
            // 首先尝试从注册表中获取
            if (_subFlowRegistry.TryGetValue(subFlowType, out var existingSubFlow))
            {
                return existingSubFlow;
            }
            
            // 尝试从容器中解析
            try
            {
                var subFlow = _container.Resolve(subFlowType) as ISubFlow;
                if (subFlow != null)
                {
                    _subFlowRegistry[subFlowType] = subFlow;
                    subFlow.ParentFlow = _parentMainFlow;
                    
                    // 设置事件发布器
                    if (subFlow is BaseFlow baseFlow)
                    {
                        baseFlow.SetEventPublisher(_eventPublisher);
                    }
                    
                    Debug.Log($"Created and registered sub flow from container: {subFlow.FlowName} ({subFlowType.Name})");
                    return subFlow;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to resolve sub flow {subFlowType.Name} from container: {ex.Message}");
            }
            
            // 尝试直接创建实例
            try
            {
                var subFlow = Activator.CreateInstance(subFlowType) as ISubFlow;
                if (subFlow != null)
                {
                    _subFlowRegistry[subFlowType] = subFlow;
                    subFlow.ParentFlow = _parentMainFlow;
                    
                    // 设置事件发布器
                    if (subFlow is BaseFlow baseFlow)
                    {
                        baseFlow.SetEventPublisher(_eventPublisher);
                    }
                    
                    Debug.LogWarning($"Created sub flow using Activator (no DI): {subFlow.FlowName} ({subFlowType.Name})");
                    return subFlow;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create sub flow {subFlowType.Name} using Activator: {ex.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Clear().Forget();
                _disposed = true;
            }
        }
    }
}