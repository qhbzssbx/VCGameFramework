using VContainer;

namespace Game.Core
{
    public interface IModule
    {
        void Configure(IContainerBuilder builder);
    }
}