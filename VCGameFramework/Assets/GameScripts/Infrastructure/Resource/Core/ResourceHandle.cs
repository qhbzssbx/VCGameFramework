using System;
using UnityEngine;
using YooAsset;

namespace Game.Infrastructure.Resource.Core
{
    /// <summary>
    /// 资源Handle包装器，提供智能资源管理
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    public class ResourceHandle<T> : IResourceHandle where T : UnityEngine.Object
    {
        private AssetHandle _yooHandle;
        private bool _disposed = false;

        /// <summary>
        /// 内部构造函数，只能通过ResourceService创建
        /// </summary>
        internal ResourceHandle(AssetHandle yooHandle)
        {
            _yooHandle = yooHandle ?? throw new ArgumentNullException(nameof(yooHandle));
        }

        /// <summary>
        /// 获取资源对象
        /// </summary>
        public T Asset
        {
            get
            {
                if (_disposed)
                {
                    Debug.LogWarning("Attempting to access disposed ResourceHandle");
                    return null;
                }

                return _yooHandle?.AssetObject as T;
            }
        }

        /// <summary>
        /// Handle是否有效
        /// </summary>
        public bool IsValid => !_disposed && _yooHandle != null && _yooHandle.IsValid;

        /// <summary>
        /// 非泛型版本的Asset属性
        /// </summary>
        UnityEngine.Object IResourceHandle.Asset => Asset;

        /// <summary>
        /// 手动释放资源
        /// </summary>
        public virtual void Dispose()
        {
            if (!_disposed && _yooHandle != null)
            {
                _yooHandle.Release();
                _disposed = true;
            }
        }

        /// <summary>
        /// 隐式转换到资源类型，便于使用
        /// </summary>
        public static implicit operator T(ResourceHandle<T> handle)
        {
            return handle?.Asset;
        }

        /// <summary>
        /// 隐式转换到bool，便于null检查
        /// </summary>
        public static implicit operator bool(ResourceHandle<T> handle)
        {
            return handle?.IsValid == true;
        }

        /// <summary>
        /// 析构函数确保资源释放
        /// </summary>
        ~ResourceHandle()
        {
            if (!_disposed)
            {
                Debug.LogWarning($"ResourceHandle<{typeof(T).Name}> was not properly disposed. This may cause memory leaks.");
                Dispose();
            }
        }
    }
}