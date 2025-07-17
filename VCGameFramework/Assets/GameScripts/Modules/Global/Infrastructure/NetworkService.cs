using Cysharp.Threading.Tasks;
using Game.Modules.Global.Domain;
using UnityEngine;

namespace Game.Modules.Global.Infrastructure
{
    /// <summary>
    /// 网络通信服务实现
    /// </summary>
    public class NetworkService : INetworkService
    {
        /// <inheritdoc />
        public async UniTask ConnectAsync()
        {
            Debug.Log("[Network] Connect");
            await UniTask.Delay(500);
        }

        /// <inheritdoc />
        public async UniTask SendAsync(string message)
        {
            Debug.Log("[Network] Send: " + message);
            await UniTask.Delay(10);
        }
    }
}
