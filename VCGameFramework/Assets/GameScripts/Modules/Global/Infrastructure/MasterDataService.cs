using Cysharp.Threading.Tasks;
using Game.Modules.Global.Domain;

namespace Game.Modules.Global.Infrastructure
{
    public class MasterDataService : IMasterDataService
    {
        public async UniTask LoadAsync()
        {
            await UniTask.Delay(100);
        }
    }
}
