using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace Game.Core.Resource
{
    public interface IAssetRegistry
    {
        UniTask<AssetHandle> AcquireAsync<T>(string package, string address, CancellationToken ct)
            where T : UnityEngine.Object;
        void Release(string package, string address);
        AssetStats Snapshot(); // 指标快照
    }
}
