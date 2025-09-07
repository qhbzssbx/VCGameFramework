using Game.Infrastructure.Resource.Core;
using UnityEngine;

namespace Game.Infrastructure.Resource.Extensions
{
    /// <summary>
    /// GameObject扩展方法，简化资源管理操作
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// 获取或添加指定类型的组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="gameObject">目标GameObject</param>
        /// <returns>组件实例</returns>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// 获取或添加AutoResourceRelease组件
        /// 便捷方法，用于快速设置资源自动释放
        /// </summary>
        /// <param name="gameObject">目标GameObject</param>
        /// <returns>AutoResourceRelease组件</returns>
        public static AutoResourceRelease GetAutoResourceRelease(this GameObject gameObject)
        {
            return gameObject.GetOrAddComponent<AutoResourceRelease>();
        }

        /// <summary>
        /// 检查GameObject是否有AutoResourceRelease组件
        /// </summary>
        /// <param name="gameObject">目标GameObject</param>
        /// <returns>是否有AutoResourceRelease组件</returns>
        public static bool HasAutoResourceRelease(this GameObject gameObject)
        {
            return gameObject.GetComponent<AutoResourceRelease>() != null;
        }

        /// <summary>
        /// 获取GameObject上AutoResourceRelease组件管理的Handle数量
        /// 用于调试和监控
        /// </summary>
        /// <param name="gameObject">目标GameObject</param>
        /// <returns>Handle数量，如果没有组件返回0</returns>
        public static int GetManagedHandleCount(this GameObject gameObject)
        {
            var autoRelease = gameObject.GetComponent<AutoResourceRelease>();
            return autoRelease?.HandleCount ?? 0;
        }

        /// <summary>
        /// 手动释放GameObject上的所有资源Handle
        /// </summary>
        /// <param name="gameObject">目标GameObject</param>
        /// <returns>是否成功释放（即是否有AutoResourceRelease组件）</returns>
        public static bool ReleaseAllResources(this GameObject gameObject)
        {
            var autoRelease = gameObject.GetComponent<AutoResourceRelease>();
            if (autoRelease != null)
            {
                autoRelease.ReleaseAll();
                return true;
            }
            return false;
        }
    }
}