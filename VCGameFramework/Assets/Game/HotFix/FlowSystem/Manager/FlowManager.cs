using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.HotFix.FlowSystem.BaseClass;
using Game.HotFix.FlowSystem.Interface;
using UnityEngine;
using VContainer;

namespace Game.HotFix.FlowSystem.Manager
{
    /// <summary>
    /// 主流程管理器实现
    /// </summary>
    public class FlowManager : IFlowManager, IDisposable
    {
        private readonly Dictionary<Type, IMainFlow> _flowRegistry = new();
        private readonly List<Type> _flowHistory = new();
        private readonly IFlowEventPublisher _eventPublisher;
        private readonly IObjectResolver _container;
        
        private IMainFlow _currentFlow;
        private bool _disposed = false;
        
        /// <summary>
        /// 当前活跃的主流程
        /// </summary>
        public IMainFlow CurrentFlow => _currentFlow;
        
        /// <summary>
        /// 流程切换历史
        /// </summary>
        public IReadOnlyList<Type> FlowHistory => _flowHistory.AsReadOnly();
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="eventPublisher">事件发布器</param>
        /// <param name="container">依赖注入容器</param>
        public FlowManager(IFlowEventPublisher eventPublisher, IObjectResolver container)
        {
            _eventPublisher = eventPublisher;
            _container = container;
        }
        
        /// <summary>
        /// 切换到指定类型的流程
        /// </summary>
        /// <typeparam name="T">流程类型</typeparam>
        /// <param name="context">流程上下文</param>
        /// <returns>异步任务</returns>
        public async UniTask SwitchToFlow<T>(FlowContext context = null) where T : class, IMainFlow
        {
            await SwitchToFlow(typeof(T), context);
        }
        
        /// <summary>
        /// 切换到指定类型的流程
        /// </summary>
        /// <param name="flowType">流程类型</param>
        /// <param name="context">流程上下文</param>
        /// <returns>异步任务</returns>
        public async UniTask SwitchToFlow(Type flowType, FlowContext context = null)
        {
            if (_disposed)
            {
                Debug.LogError("FlowManager has been disposed, cannot switch flows");
                return;
            }
            
            // 验证流程类型
            if (!typeof(IMainFlow).IsAssignableFrom(flowType))
            {
                Debug.LogError($"Type {flowType.Name} is not a valid main flow type");
                return;
            }
            
            // 检查是否可以切换到目标流程
            if (_currentFlow != null && !_currentFlow.CanSwitchTo(flowType))
            {
                Debug.LogWarning($"Cannot switch from {_currentFlow.FlowName} to {flowType.Name}");
                return;
            }
            
            // 如果目标流程就是当前流程，跳过切换
            if (_currentFlow?.GetType() == flowType)
            {
                Debug.LogWarning($"Target flow {flowType.Name} is already active");
                return;
            }
            
            var targetFlow = GetOrCreateFlow(flowType);
            if (targetFlow == null)
            {
                Debug.LogError($"Failed to get or create flow of type {flowType.Name}");
                return;
            }
            
            var oldFlow = _currentFlow;
            
            try
            {
                // 退出当前流程
                if (_currentFlow != null)
                {
                    Debug.Log($"Exiting current flow: {_currentFlow.FlowName}");
                    await _currentFlow.OnExit();
                }
                
                // 进入新流程
                Debug.Log($"Entering new flow: {targetFlow.FlowName}");
                _currentFlow = targetFlow;
                await _currentFlow.OnEnter(context);
                
                // 记录历史
                _flowHistory.Add(flowType);
                
                // 发布切换事件
                _eventPublisher?.PublishMainFlowSwitched(oldFlow, targetFlow, context);
                
                Debug.Log($"Successfully switched to flow: {targetFlow.FlowName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error switching to flow {flowType.Name}: {ex.Message}");
                
                // 发布错误事件
                _eventPublisher?.PublishError(targetFlow, ex, context);
                
                // 如果新流程进入失败，尝试恢复旧流程
                if (oldFlow != null && _currentFlow == targetFlow)
                {
                    Debug.LogWarning("Attempting to restore previous flow due to error");
                    _currentFlow = oldFlow;
                    try
                    {
                        await _currentFlow.OnEnter(context);
                    }
                    catch (Exception restoreEx)
                    {
                        Debug.LogError($"Failed to restore previous flow: {restoreEx.Message}");
                        _currentFlow = null;
                    }
                }
                
                throw;
            }
        }
        
        /// <summary>
        /// 检查指定类型的流程是否处于活跃状态
        /// </summary>
        /// <typeparam name="T">流程类型</typeparam>
        /// <returns>如果活跃返回true，否则返回false</returns>
        public bool IsFlowActive<T>() where T : class, IMainFlow
        {
            return IsFlowActive(typeof(T));
        }
        
        /// <summary>
        /// 检查指定类型的流程是否处于活跃状态
        /// </summary>
        /// <param name="flowType">流程类型</param>
        /// <returns>如果活跃返回true，否则返回false</returns>
        public bool IsFlowActive(Type flowType)
        {
            return _currentFlow?.GetType() == flowType && _currentFlow.IsActive;
        }
        
        /// <summary>
        /// 注册流程实例
        /// </summary>
        /// <typeparam name="T">流程类型</typeparam>
        /// <param name="flow">流程实例</param>
        public void RegisterFlow<T>(T flow) where T : class, IMainFlow
        {
            if (flow == null)
            {
                Debug.LogError("Cannot register null flow");
                return;
            }
            
            var flowType = typeof(T);
            _flowRegistry[flowType] = flow;
            
            // 设置事件发布器
            if (flow is BaseFlow baseFlow)
            {
                baseFlow.SetEventPublisher(_eventPublisher);
            }
            
            Debug.Log($"Registered flow: {flow.FlowName} ({flowType.Name})");
        }
        
        /// <summary>
        /// 获取指定类型的流程实例
        /// </summary>
        /// <typeparam name="T">流程类型</typeparam>
        /// <returns>流程实例，如果不存在返回null</returns>
        public T GetFlow<T>() where T : class, IMainFlow
        {
            return GetFlow(typeof(T)) as T;
        }
        
        /// <summary>
        /// 获取指定类型的流程实例
        /// </summary>
        /// <param name="flowType">流程类型</param>
        /// <returns>流程实例，如果不存在返回null</returns>
        public IMainFlow GetFlow(Type flowType)
        {
            _flowRegistry.TryGetValue(flowType, out var flow);
            return flow;
        }
        
        /// <summary>
        /// 停止所有流程并清理
        /// </summary>
        /// <returns>异步任务</returns>
        public async UniTask Shutdown()
        {
            if (_disposed) return;
            
            Debug.Log("Shutting down FlowManager");
            
            try
            {
                // 停止当前流程
                if (_currentFlow != null)
                {
                    await _currentFlow.OnExit();
                    _currentFlow = null;
                }
                
                // 清理所有流程
                foreach (var flow in _flowRegistry.Values)
                {
                    if (flow.IsActive)
                    {
                        try
                        {
                            await flow.OnExit();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error shutting down flow {flow.FlowName}: {ex.Message}");
                        }
                    }
                }
                
                _flowRegistry.Clear();
                _flowHistory.Clear();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during FlowManager shutdown: {ex.Message}");
            }
            
            Debug.Log("FlowManager shutdown completed");
        }
        
        /// <summary>
        /// 获取或创建流程实例
        /// </summary>
        /// <param name="flowType">流程类型</param>
        /// <returns>流程实例</returns>
        private IMainFlow GetOrCreateFlow(Type flowType)
        {
            // 首先尝试从注册表中获取
            if (_flowRegistry.TryGetValue(flowType, out var existingFlow))
            {
                return existingFlow;
            }
            
            // 尝试从容器中解析
            try
            {
                var flow = _container.Resolve(flowType) as IMainFlow;
                if (flow != null)
                {
                    _flowRegistry[flowType] = flow;
                    
                    // 设置事件发布器
                    if (flow is BaseFlow baseFlow)
                    {
                        baseFlow.SetEventPublisher(_eventPublisher);
                    }
                    
                    Debug.Log($"Created and registered flow from container: {flow.FlowName} ({flowType.Name})");
                    return flow;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to resolve flow {flowType.Name} from container: {ex.Message}");
            }
            
            // 尝试直接创建实例
            try
            {
                var flow = Activator.CreateInstance(flowType) as IMainFlow;
                if (flow != null)
                {
                    _flowRegistry[flowType] = flow;
                    
                    // 设置事件发布器
                    if (flow is BaseFlow baseFlow)
                    {
                        baseFlow.SetEventPublisher(_eventPublisher);
                    }
                    
                    Debug.LogWarning($"Created flow using Activator (no DI): {flow.FlowName} ({flowType.Name})");
                    return flow;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create flow {flowType.Name} using Activator: {ex.Message}");
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
                Shutdown().Forget();
                _disposed = true;
            }
        }
    }
}