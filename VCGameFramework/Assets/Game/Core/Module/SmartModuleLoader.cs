using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Core
{
    /// <summary>
    /// 智能模块加载器
    /// 保留反射自动发现的优势，但限制扫描范围，提升性能
    /// </summary>
    public static class SmartModuleLoader
    {
        static readonly List<IAsyncModule> asyncModules = new();
        
        /// <summary>
        /// 模块扫描配置
        /// </summary>
        public static class ScanConfig
        {
            /// <summary>
            /// 允许扫描的命名空间前缀
            /// </summary>
            public static readonly string[] AllowedNamespaces = 
            {
                "Game.Core",
                "Game.Modules",
                "Game.Infrastructure", 
                "Game.Flows",
                "Game.UI",
                // 可以继续添加你的命名空间
            };
            
            /// <summary>
            /// 允许扫描的程序集名称
            /// </summary>
            public static readonly string[] AllowedAssemblies = 
            {
                "Assembly-CSharp",
                "Game.Core",
                // 你的自定义程序集
            };
            
            /// <summary>
            /// 跳过的程序集名称（性能优化）
            /// </summary>
            public static readonly string[] SkippedAssemblies = 
            {
                "Unity",
                "UnityEngine",
                "UnityEditor",
                "System",
                "Microsoft",
                "Mono",
                "mscorlib",
                "netstandard",
                "DOTween",
                "YooAsset",
                "MessagePipe",
                "VContainer",
                "Cysharp",
            };
        }

        /// <summary>
        /// 智能注册所有模块
        /// 只扫描指定命名空间，避免全程序集扫描的性能问题
        /// </summary>
        public static void RegisterAllModules(IContainerBuilder builder)
        {
            
            asyncModules.Clear();
            var modules = DiscoverModulesIntelligently();
            
            UnityEngine.Debug.Log($"Smart scan found {modules.Count} modules in allowed namespaces");
            
            foreach (var module in modules)
            {
                try
                {
                    module.Configure(builder);
                    if (module is IAsyncModule asyncModule)
                        asyncModules.Add(asyncModule);
                        
                    UnityEngine.Debug.Log($"✓ Module registered: {module.GetType().Name}");
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"✗ Failed to register module {module.GetType().Name}: {ex.Message}");
                    throw;
                }
            }

            if (asyncModules.Count > 0)
            {
                builder.RegisterInstance<IReadOnlyList<IAsyncModule>>(asyncModules);
                builder.RegisterEntryPoint<ModuleInitializer>(Lifetime.Singleton);
                UnityEngine.Debug.Log($"Registered {asyncModules.Count} async modules for initialization");
            }
            
            UnityEngine.Debug.Log($"✅ Smart module registration completed!");
        }

        /// <summary>
        /// 智能发现模块：只扫描允许的程序集和命名空间
        /// </summary>
        private static List<IModule> DiscoverModulesIntelligently()
        {
            
            var result = new List<IModule>();
            var moduleType = typeof(IModule);
            var scannedTypes = 0;
            var scannedAssemblies = 0;
            
            // 获取所有已加载的程序集
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            UnityEngine.Debug.Log($"Total loaded assemblies: {allAssemblies.Length}");
            
            foreach (var assembly in allAssemblies)
            {
                // 跳过不需要扫描的程序集
                if (ShouldSkipAssembly(assembly))
                {
                    continue;
                }
                
                scannedAssemblies++;
                UnityEngine.Debug.Log($"Scanning assembly: {assembly.GetName().Name}");
                
                try
                {
                    var types = assembly.GetTypes();
                    
                    foreach (var type in types)
                    {
                        scannedTypes++;
                        
                        // 检查是否在允许的命名空间中
                        if (!IsInAllowedNamespace(type))
                        {
                            continue;
                        }
                        
                        // 检查是否实现了IModule接口
                        if (moduleType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                        {
                            try
                            {
                                if (Activator.CreateInstance(type) is IModule module)
                                {
                                    result.Add(module);
                                    UnityEngine.Debug.Log($"Found module: {type.FullName}");
                                }
                            }
                            catch (Exception ex)
                            {
                                UnityEngine.Debug.LogError($"Failed to create module instance {type.FullName}: {ex.Message}");
                            }
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    UnityEngine.Debug.LogWarning($"Could not load all types from assembly {assembly.GetName().Name}: {ex.Message}");
                    
                    // 尝试加载成功的类型
                    foreach (var type in ex.Types.Where(t => t != null))
                    {
                        scannedTypes++;
                        
                        if (!IsInAllowedNamespace(type)) continue;
                        
                        if (moduleType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                        {
                            try
                            {
                                if (Activator.CreateInstance(type) is IModule module)
                                {
                                    result.Add(module);
                                    UnityEngine.Debug.Log($"Found module: {type.FullName}");
                                }
                            }
                            catch (Exception createEx)
                            {
                                UnityEngine.Debug.LogError($"Failed to create module instance {type.FullName}: {createEx.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Error scanning assembly {assembly.GetName().Name}: {ex.Message}");
                }
            }
            
            // 按优先级排序
            var sortedResult = result.OrderBy(m =>
            {
                if (m is IModuleWithOrder ordered)
                    return ordered.Order;
                return 0;
            }).ToList();
            
            UnityEngine.Debug.Log($"📊 Scan statistics: {scannedAssemblies} assemblies, {scannedTypes} types, {result.Count} modules found");
            
            return sortedResult;
        }

        /// <summary>
        /// 检查程序集是否应该跳过
        /// </summary>
        private static bool ShouldSkipAssembly(Assembly assembly)
        {
            var assemblyName = assembly.GetName().Name;
            
            // 检查是否在允许列表中
            if (ScanConfig.AllowedAssemblies.Any(allowed => assemblyName.StartsWith(allowed)))
            {
                return false; // 在允许列表中，不跳过
            }
            
            // 检查是否在跳过列表中
            if (ScanConfig.SkippedAssemblies.Any(skipped => assemblyName.StartsWith(skipped)))
            {
                return true; // 在跳过列表中，跳过
            }
            
            // 默认不跳过（保守策略）
            return true;
        }

        /// <summary>
        /// 检查类型是否在允许的命名空间中
        /// </summary>
        private static bool IsInAllowedNamespace(Type type)
        {
            if (string.IsNullOrEmpty(type.Namespace))
            {
                return false; // 没有命名空间的类型通常不是我们的模块
            }
            
            return ScanConfig.AllowedNamespaces.Any(ns => type.Namespace.StartsWith(ns));
        }

        /// <summary>
        /// 获取扫描统计信息（调试用）
        /// </summary>
        public static string GetScanStatistics()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var allowedAssemblies = allAssemblies.Where(a => !ShouldSkipAssembly(a)).ToList();
            var skippedAssemblies = allAssemblies.Length - allowedAssemblies.Count;
            
            return $"Assembly scan scope: {allowedAssemblies.Count} allowed, {skippedAssemblies} skipped, " +
                   $"namespaces: [{string.Join(", ", ScanConfig.AllowedNamespaces)}]";
        }

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器工具：显示扫描统计
        /// </summary>
        [UnityEditor.MenuItem("VCFramework/Module Scan Statistics")]
        public static void ShowScanStatistics()
        {
            UnityEngine.Debug.Log("📊 === Module Scan Statistics ===");
            UnityEngine.Debug.Log(GetScanStatistics());
            
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            UnityEngine.Debug.Log($"Allowed assemblies:");
            foreach (var assembly in allAssemblies.Where(a => !ShouldSkipAssembly(a)))
            {
                UnityEngine.Debug.Log($"  ✓ {assembly.GetName().Name}");
            }
            
            UnityEngine.Debug.Log($"Skipped assemblies (first 10):");
            foreach (var assembly in allAssemblies.Where(ShouldSkipAssembly).Take(10))
            {
                UnityEngine.Debug.Log($"  ⏭ {assembly.GetName().Name}");
            }
        }
#endif
    }
}