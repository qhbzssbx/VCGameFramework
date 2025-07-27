using Cysharp.Threading.Tasks;
using VContainer;

namespace Game.Core
{
    public interface IAsyncModule : IModule
    {
        UniTask InitializeAsync(IObjectResolver resolver);
    }
}
