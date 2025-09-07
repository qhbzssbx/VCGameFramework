using System;

namespace Game.Modules.Global.Domain
{
    /// <summary>
    /// 全局事件总线接口，用于跨模块消息通信
    /// </summary>
    public interface IGlobalEventBus
    {
        /// <summary>
        /// 发布消息
        /// </summary>
        void Publish<T>(T message);

        /// <summary>
        /// 订阅消息
        /// </summary>
        void Subscribe<T>(Action<T> handler);

        /// <summary>
        /// 取消订阅
        /// </summary>
        void Unsubscribe<T>(Action<T> handler);
    }
}
