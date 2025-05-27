using VContainer;

namespace Game.Modules.Log.Application
{
    public interface IGameModule
    {
        void Register(IContainerBuilder builder);
    }
}