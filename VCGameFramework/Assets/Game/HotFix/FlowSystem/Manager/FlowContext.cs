using System;
using System.Collections.Generic;

namespace Game.HotFix.FlowSystem
{
    /// <summary>
    /// 流程上下文，用于在流程间传递数据
    /// 支持强类型数据存储和检索，以及父子关系管理
    /// </summary>
    public class FlowContext : IDisposable
    {
        private readonly Dictionary<string, object> _data = new();
        private readonly Dictionary<Type, object> _typedData = new();
        private FlowContext _parent;
        private bool _disposed = false;
        
        /// <summary>
        /// 父上下文引用
        /// </summary>
        public FlowContext Parent => _parent;
        
        /// <summary>
        /// 设置字符串键值对数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void Set<T>(string key, T value)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(FlowContext));
            _data[key] = value;
        }
        
        /// <summary>
        /// 获取字符串键对应的数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>对应的值，如果不存在则返回默认值</returns>
        public T Get<T>(string key)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(FlowContext));
            
            // 首先在当前上下文中查找
            if (_data.TryGetValue(key, out var value) && value is T)
            {
                return (T)value;
            }
            
            // 如果没找到且有父上下文，则在父上下文中查找
            if (_parent != null)
            {
                return _parent.Get<T>(key);
            }
            
            return default(T);
        }
        
        /// <summary>
        /// 设置类型化数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="value">值</param>
        public void SetTyped<T>(T value)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(FlowContext));
            _typedData[typeof(T)] = value;
        }
        
        /// <summary>
        /// 获取类型化数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns>对应的值，如果不存在则返回默认值</returns>
        public T GetTyped<T>()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(FlowContext));
            
            // 首先在当前上下文中查找
            if (_typedData.TryGetValue(typeof(T), out var value) && value is T)
            {
                return (T)value;
            }
            
            // 如果没找到且有父上下文，则在父上下文中查找
            if (_parent != null)
            {
                return _parent.GetTyped<T>();
            }
            
            return default(T);
        }
        
        /// <summary>
        /// 检查是否包含指定键的数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>如果包含返回true，否则返回false</returns>
        public bool HasKey(string key)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(FlowContext));
            
            return _data.ContainsKey(key) || (_parent?.HasKey(key) ?? false);
        }
        
        /// <summary>
        /// 创建子上下文
        /// </summary>
        /// <returns>新的子上下文实例</returns>
        public FlowContext CreateChild()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(FlowContext));
            
            return new FlowContext { _parent = this };
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _data.Clear();
                _typedData.Clear();
                _parent = null;
                _disposed = true;
            }
        }
    }
}