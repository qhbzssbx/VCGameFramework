namespace Game.Modules.Global.Domain
{
    using Cysharp.Threading.Tasks;

    public interface INetworkService
    {
        UniTask ConnectAsync();
        UniTask SendAsync(string message);
    }
}
