using System;
using System.Threading;
using Game.AOT.AssetSystem.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.AOT.AssetSystem.Utilities
{
    /// <summary>
    /// Scope管理辅助工具，简化资源管理相关的Scope创建和配置
    /// </summary>
    public static class ScopeHelper
    {
        /// <summary>
        /// 配置资源管理相关的服务到IContainerBuilder
        /// </summary>
        /// <param name="builder">容器构建器</param>
        /// <param name="cancellationToken">取消令牌，如果为null则创建新的</param>
        /// <param name="scopeName">Scope名称（用于调试）</param>
        public static void ConfigureAssetServices(IContainerBuilder builder, CancellationToken? cancellationToken = null, string scopeName = "AssetScope")
        {
            CancellationTokenSource cts = null;
            
            // 如果没有提供CancellationToken，创建一个新的
            if (cancellationToken == null)
            {
                cts = new CancellationTokenSource();
                cancellationToken = cts.Token;
            }
            
            // 注册CancellationToken
            builder.RegisterInstance(cancellationToken.Value);

            // 注册AssetScope
            builder.Register<IAssetScope>(provider =>
            {
                var registry = provider.Resolve<IAssetRegistry>();
                var token = provider.Resolve<CancellationToken>();
                return new AssetScope.AssetScope(registry, token);
            }, Lifetime.Scoped)
            .AsImplementedInterfaces()
            .As<IAsyncStartable>();

            Debug.Log($"[ScopeHelper] Configured asset services for scope: {scopeName}");
        }

        /// <summary>
        /// 创建一个配置了资源管理的子Scope
        /// </summary>
        /// <param name="parent">父Scope</param>
        /// <param name="cancellationToken">取消令牌，如果为null则创建新的</param>
        /// <param name="scopeName">Scope名称</param>
        /// <param name="onDestroy">Scope销毁时的回调</param>
        /// <returns>新的子Scope包装器，包含清理逻辑</returns>
        public static ScopeWrapper CreateAssetScope(LifetimeScope parent, CancellationToken? cancellationToken = null, 
            string scopeName = "AssetScope", Action onDestroy = null)
        {
            CancellationTokenSource cts = null;
            
            var childScope = parent.CreateChild(builder =>
            {
                if (cancellationToken == null)
                {
                    cts = new CancellationTokenSource();
                    cancellationToken = cts.Token;
                }

                ConfigureAssetServices(builder, cancellationToken, scopeName);
            });

            childScope.name = scopeName;

            return new ScopeWrapper(childScope, cts, onDestroy);
        }

        /// <summary>
        /// 配置场景级别的资源Scope
        /// </summary>
        /// <param name="parent">父Scope</param>
        /// <param name="sceneName">场景名称</param>
        /// <returns>场景资源Scope包装器</returns>
        public static ScopeWrapper CreateSceneAssetScope(LifetimeScope parent, string sceneName)
        {
            return CreateAssetScope(parent, null, $"Scene_{sceneName}", () =>
            {
                Debug.Log($"[ScopeHelper] Scene scope destroyed: {sceneName}");
            });
        }

        /// <summary>
        /// 配置UI窗口级别的资源Scope
        /// </summary>
        /// <param name="parent">父Scope</param>
        /// <param name="windowName">窗口名称</param>
        /// <returns>窗口资源Scope包装器</returns>
        public static ScopeWrapper CreateWindowAssetScope(LifetimeScope parent, string windowName)
        {
            return CreateAssetScope(parent, null, $"Window_{windowName}", () =>
            {
                Debug.Log($"[ScopeHelper] Window scope destroyed: {windowName}");
            });
        }
    }

    /// <summary>
    /// LifetimeScope包装器，提供安全的清理逻辑
    /// </summary>
    public class ScopeWrapper : IDisposable
    {
        private readonly LifetimeScope _scope;
        private readonly CancellationTokenSource _cts;
        private readonly Action _onDestroy;
        private bool _disposed;

        public LifetimeScope Scope => _scope;

        internal ScopeWrapper(LifetimeScope scope, CancellationTokenSource cts, Action onDestroy)
        {
            _scope = scope;
            _cts = cts;
            _onDestroy = onDestroy;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                _onDestroy?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ScopeWrapper] Error in onDestroy callback: {ex.Message}");
            }

            try
            {
                _cts?.Cancel();
                _cts?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ScopeWrapper] Error disposing CancellationTokenSource: {ex.Message}");
            }

            try
            {
                _scope?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ScopeWrapper] Error disposing LifetimeScope: {ex.Message}");
            }
        }
    }
}