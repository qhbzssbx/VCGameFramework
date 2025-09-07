using System;
using UnityEngine;

namespace Game.Infrastructure.Resource.Core
{
    /// <summary>
    /// 资源Handle接口，支持统一的资源管理
    /// </summary>
    public interface IResourceHandle : IDisposable
    {
        /// <summary>
        /// Handle是否有效
        /// </summary>
        bool IsValid { get; }
        
        /// <summary>
        /// 获取资源对象（非泛型版本）
        /// </summary>
        UnityEngine.Object Asset { get; }
    }
}