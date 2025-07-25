using System;

namespace Game.Core.FlowSystem
{
    /// <summary>
    /// 主流程基础抽象类
    /// </summary>
    public abstract class BaseMainFlow : BaseFlow, IMainFlow
    {
        /// <summary>
        /// 流程优先级，子类可以重写
        /// </summary>
        public virtual int Priority => 0;
        
        /// <summary>
        /// 检查是否可以切换到指定的目标流程
        /// 默认实现允许切换到任何流程，子类可以重写以添加特定的限制
        /// </summary>
        /// <param name="targetFlowType">目标流程类型</param>
        /// <returns>如果可以切换返回true，否则返回false</returns>
        public virtual bool CanSwitchTo(Type targetFlowType)
        {
            // 默认实现：不能切换到自己
            if (targetFlowType == GetType())
            {
                return false;
            }
            
            // 检查目标类型是否是有效的主流程
            if (!typeof(IMainFlow).IsAssignableFrom(targetFlowType))
            {
                return false;
            }
            
            return true;
        }
    }
}