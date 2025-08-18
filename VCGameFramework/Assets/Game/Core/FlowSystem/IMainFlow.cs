using System;

namespace Game.Core.FlowSystem
{
    /// <summary>
    /// 主流程接口，用于管理游戏的主要生命周期阶段
    /// 主流程之间是互斥的，同时只能有一个主流程处于活跃状态
    /// </summary>
    public interface IMainFlow : IFlow
    {
        /// <summary>
        /// 流程优先级，数值越小优先级越高
        /// 用于确定流程的启动顺序和重要程度
        /// </summary>
        int Priority { get; }
        
        /// <summary>
        /// 检查是否可以切换到指定的目标流程
        /// </summary>
        /// <param name="targetFlowType">目标流程类型</param>
        /// <returns>如果可以切换返回true，否则返回false</returns>
        bool CanSwitchTo(Type targetFlowType);
    }
}