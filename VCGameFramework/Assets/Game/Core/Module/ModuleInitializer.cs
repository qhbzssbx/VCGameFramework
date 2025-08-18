using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace Game.Core
{
    public class ModuleInitializer : IStartable
    {
        readonly IReadOnlyList<IAsyncModule> modules;
        readonly IObjectResolver resolver;

        public ModuleInitializer(IReadOnlyList<IAsyncModule> modules, IObjectResolver resolver)
        {
            this.modules = modules;
            this.resolver = resolver;
        }

        public void Start()
        {
            Initialize().Forget();
        }

        private async UniTaskVoid Initialize()
        {
            foreach (var module in modules)
            {
                await module.InitializeAsync(resolver);
            }
        }
    }
}
