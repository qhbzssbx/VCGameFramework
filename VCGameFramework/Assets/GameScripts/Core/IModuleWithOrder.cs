namespace Game.Core
{
    public interface IModuleWithOrder : IModule
    {
        int Order { get; }
    }
}