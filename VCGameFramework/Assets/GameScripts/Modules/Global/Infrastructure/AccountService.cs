using Cysharp.Threading.Tasks;
using Game.Modules.Global.Domain;
using UnityEngine;

namespace Game.Modules.Global.Infrastructure
{
    /// <summary>
    /// 账户服务的具体实现
    /// </summary>
    public class AccountService : IAccountService
    {
        private readonly INetworkService _network;

        /// <summary>
        /// 构造函数，通过依赖注入获得网络服务
        /// </summary>
        public AccountService(INetworkService network)
        {
            _network = network;
        }

        /// <inheritdoc />
        public async UniTask LoginAsync(string username, string password)
        {
            await _network.ConnectAsync();
            Debug.Log($"[Account] Login as {username}");
        }

        /// <inheritdoc />
        public void Logout()
        {
            Debug.Log("[Account] Logout");
        }
    }
}
