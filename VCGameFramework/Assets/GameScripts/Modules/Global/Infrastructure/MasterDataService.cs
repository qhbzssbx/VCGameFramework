using Cysharp.Threading.Tasks;
using Game.Modules.Global.Domain;

namespace Game.Modules.Global.Infrastructure
{
    /// <summary>
    /// 静态数据服务实现，用于加载配置表
    /// </summary>
    public class MasterDataService : IMasterDataService
    {
        /// <inheritdoc />
        public async UniTask LoadAsync()
        {
            // 此处仅模拟加载延迟
            await UniTask.Delay(100);
        }
    }
}
