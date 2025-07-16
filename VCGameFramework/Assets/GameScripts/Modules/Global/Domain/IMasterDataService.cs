namespace Game.Modules.Global.Domain
{
    using Cysharp.Threading.Tasks;

    public interface IMasterDataService
    {
        UniTask LoadAsync();
    }
}
