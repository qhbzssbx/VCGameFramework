namespace Game.Modules.Global.Domain
{
    using Cysharp.Threading.Tasks;

    public interface IAccountService
    {
        UniTask LoginAsync(string username, string password);
        void Logout();
    }
}
