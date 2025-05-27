using System;
using System.Linq;
using System.Reflection;
using VContainer;

namespace Game.Modules.Log.Application
{
    public static class GameModuleLoader
    {
        public static void RegisterAllModules(IContainerBuilder builder)
        {
            var moduleType = typeof(IGameModule);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var type in assemblies.SelectMany(a => a.GetTypes()))
            {
                if (moduleType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                {
                    if (Activator.CreateInstance(type) is IGameModule module)
                    {
                        module.Register(builder);
                    }
                }
            }
        }
    }
}