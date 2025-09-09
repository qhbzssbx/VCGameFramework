using Cysharp.Threading.Tasks;
using Game.HotFix.FlowSystem.Interface;
using UnityEngine;

namespace Game.HotFix.FlowSystem.BaseClass
{
    /// <summary>
    /// 流程基础抽象类，提供通用的流程实现
    /// </summary>
    public abstract class BaseFlow : IFlow
    {
        /// <summary>
        /// 流程名称，默认使用类名
        /// </summary>
        public virtual string FlowName => GetType().Name;
        
        /// <summary>
        /// 流程是否处于活跃状态
        /// </summary>
        public bool IsActive { get; private set; }
        
        /// <summary>
        /// 当前流程上下文
        /// </summary>
        protected FlowContext CurrentContext { get; private set; }
        
        /// <summary>
        /// 事件发布器，用于发布流程事件
        /// </summary>
        protected IFlowEventPublisher EventPublisher { get; private set; }
        
        /// <summary>
        /// 设置事件发布器
        /// </summary>
        /// <param name="eventPublisher">事件发布器</param>
        public void SetEventPublisher(IFlowEventPublisher eventPublisher)
        {
            EventPublisher = eventPublisher;
        }
        
        /// <summary>
        /// 进入流程
        /// </summary>
        /// <param name="context">流程上下文</param>
        /// <returns>异步任务</returns>
        public async UniTask OnEnter(FlowContext context = null)
        {
            if (IsActive)
            {
                Debug.LogWarning($"Flow '{FlowName}' is already active, skipping OnEnter");
                return;
            }
            
            CurrentContext = context;
            IsActive = true;
            
            Debug.Log($"Entering flow: {FlowName}");
            
            try
            {
                await OnEnterInternal(context);
                
                // 发布进入事件
                EventPublisher?.PublishEntered(this, context);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in flow '{FlowName}' OnEnter: {ex.Message}");
                IsActive = false;
                
                // 发布错误事件
                EventPublisher?.PublishError(this, ex, context);
                throw;
            }
        }
        
        /// <summary>
        /// 流程更新循环，默认为空实现
        /// </summary>
        /// <returns>异步任务</returns>
        public virtual async UniTask OnUpdate()
        {
            if (!IsActive) return;
            
            try
            {
                await OnUpdateInternal();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in flow '{FlowName}' OnUpdate: {ex.Message}");
                
                // 发布错误事件
                EventPublisher?.PublishError(this, ex, CurrentContext);
            }
        }
        
        /// <summary>
        /// 退出流程
        /// </summary>
        /// <returns>异步任务</returns>
        public async UniTask OnExit()
        {
            if (!IsActive)
            {
                Debug.LogWarning($"Flow '{FlowName}' is not active, skipping OnExit");
                return;
            }
            
            Debug.Log($"Exiting flow: {FlowName}");
            
            try
            {
                await OnExitInternal();
                
                // 发布退出事件
                EventPublisher?.PublishExited(this, CurrentContext);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in flow '{FlowName}' OnExit: {ex.Message}");
                
                // 发布错误事件
                EventPublisher?.PublishError(this, ex, CurrentContext);
            }
            finally
            {
                IsActive = false;
                CurrentContext = null;
            }
        }
        
        /// <summary>
        /// 内部进入流程实现，子类需要重写此方法
        /// </summary>
        /// <param name="context">流程上下文</param>
        /// <returns>异步任务</returns>
        protected abstract UniTask OnEnterInternal(FlowContext context);
        
        /// <summary>
        /// 内部更新循环实现，子类可以重写此方法
        /// </summary>
        /// <returns>异步任务</returns>
        protected virtual UniTask OnUpdateInternal()
        {
            return UniTask.CompletedTask;
        }
        
        /// <summary>
        /// 内部退出流程实现，子类可以重写此方法
        /// </summary>
        /// <returns>异步任务</returns>
        protected virtual UniTask OnExitInternal()
        {
            return UniTask.CompletedTask;
        }
    }
}