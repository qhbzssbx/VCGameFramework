using System;
using System.Collections.Generic;
using Game.Modules.Global.Domain;

namespace Game.Modules.Global.Infrastructure
{
    public class GlobalEventBus : IGlobalEventBus
    {
        private readonly Dictionary<Type, Delegate> _listeners = new();

        public void Publish<T>(T message)
        {
            if (_listeners.TryGetValue(typeof(T), out var d))
            {
                var callback = d as Action<T>;
                callback?.Invoke(message);
            }
        }

        public void Subscribe<T>(Action<T> handler)
        {
            if (_listeners.TryGetValue(typeof(T), out var d))
            {
                _listeners[typeof(T)] = Delegate.Combine(d, handler);
            }
            else
            {
                _listeners[typeof(T)] = handler;
            }
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            if (_listeners.TryGetValue(typeof(T), out var d))
            {
                var current = Delegate.Remove(d, handler);
                if (current == null)
                    _listeners.Remove(typeof(T));
                else
                    _listeners[typeof(T)] = current;
            }
        }
    }
}
