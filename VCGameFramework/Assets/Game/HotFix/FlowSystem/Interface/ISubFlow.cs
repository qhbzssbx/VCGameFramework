namespace Game.HotFix.FlowSystem.Interface
{
    /// <summary>
    /// 子流程接口，用于管理主流程内部的子状态
    /// 子流程可以嵌套，支持栈式管理和返回机制
    /// </summary>
    public interface ISubFlow : IFlow
    {
        /// <summary>
        /// 父流程引用，指向当前子流程所属的主流程
        /// </summary>
        IMainFlow ParentFlow { get; set; }
        
        /// <summary>
        /// 指示进入此子流程时是否应该暂停父流程
        /// 例如暂停菜单需要暂停游戏，而HUD界面则不需要
        /// </summary>
        bool ShouldPauseParent { get; }
    }
}