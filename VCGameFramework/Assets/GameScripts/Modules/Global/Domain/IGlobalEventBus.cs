using System;

namespace Game.Modules.Global.Domain
{
    public interface IGlobalEventBus
    {
        void Publish<T>(T message);
        void Subscribe<T>(Action<T> handler);
        void Unsubscribe<T>(Action<T> handler);
    }
}
