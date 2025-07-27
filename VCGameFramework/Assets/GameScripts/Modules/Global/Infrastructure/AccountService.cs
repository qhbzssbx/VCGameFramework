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
        public async UniTask<bool> LoginAsync(string username, string password)
        {
            try
            {
                // 首先尝试连接网络
                Debug.Log($"[Account] Attempting login for {username}");
                var connected = await _network.ConnectAsync();
                
                if (!connected)
                {
                    Debug.LogError("[Account] Network connection failed, cannot login");
                    return false;
                }
                
                // 模拟登录验证逻辑
                await UniTask.Delay(1000); // 模拟验证时间
                
                // 这里应该实现真正的登录验证逻辑
                // 现在先简单验证用户名和密码不为空
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    Debug.LogWarning("[Account] Username or password is empty");
                    return false;
                }
                
                Debug.Log($"[Account] Login successful for {username}");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Account] Login failed: {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc />
        public void Logout()
        {
            Debug.Log("[Account] Logout");
        }
    }
}
