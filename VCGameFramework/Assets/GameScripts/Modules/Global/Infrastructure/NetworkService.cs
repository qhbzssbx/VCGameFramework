using Cysharp.Threading.Tasks;
using Game.Modules.Global.Domain;
using UnityEngine;

namespace Game.Modules.Global.Infrastructure
{
    public class NetworkService : INetworkService
    {
        public async UniTask ConnectAsync()
        {
            Debug.Log("[Network] Connect");
            await UniTask.Delay(500);
        }

        public async UniTask SendAsync(string message)
        {
            Debug.Log("[Network] Send: " + message);
            await UniTask.Delay(10);
        }
    }
}
