using Game.HotFix.FlowSystem.Interface;

namespace Game.HotFix.FlowSystem.BaseClass
{
    /// <summary>
    /// 子流程基础抽象类
    /// </summary>
    public abstract class BaseSubFlow : BaseFlow, ISubFlow
    {
        /// <summary>
        /// 父流程引用
        /// </summary>
        public IMainFlow ParentFlow { get; set; }
        
        /// <summary>
        /// 指示进入此子流程时是否应该暂停父流程
        /// 子类需要重写此属性来指定具体的行为
        /// </summary>
        public abstract bool ShouldPauseParent { get; }
    }
}