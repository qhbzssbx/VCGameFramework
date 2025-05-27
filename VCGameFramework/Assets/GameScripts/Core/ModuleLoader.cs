using System;
using System.Collections.Generic;
using System.Linq;
using VContainer;

namespace Game.Core
{
    public static class ModuleLoader
    {
        public static void RegisterAllModules(IContainerBuilder builder)
        {
            var modules = DiscoverModules();
            foreach (var module in modules)
            {
                module.Configure(builder);
            }
        }

        private static List<IModule> DiscoverModules()
        {
            var result = new List<IModule>();
            var moduleType = typeof(IModule);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var type in assemblies.SelectMany(a => a.GetTypes()))
            {
                if (moduleType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                {
                    if (Activator.CreateInstance(type) is IModule module)
                    {
                        result.Add(module);
                    }
                }
            }

            // 按优先级排序（默认 0）
            return result.OrderBy(m =>
            {
                if (m is IModuleWithOrder ordered)
                    return ordered.Order;
                return 0;
            }).ToList();
        }
    }
}