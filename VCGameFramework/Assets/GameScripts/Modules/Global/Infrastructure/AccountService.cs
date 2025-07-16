using Cysharp.Threading.Tasks;
using Game.Modules.Global.Domain;
using UnityEngine;

namespace Game.Modules.Global.Infrastructure
{
    public class AccountService : IAccountService
    {
        private readonly INetworkService _network;

        public AccountService(INetworkService network)
        {
            _network = network;
        }

        public async UniTask LoginAsync(string username, string password)
        {
            await _network.ConnectAsync();
            Debug.Log($"[Account] Login as {username}");
        }

        public void Logout()
        {
            Debug.Log("[Account] Logout");
        }
    }
}
