using Cysharp.Threading.Tasks;

namespace Game.Infrastructure.Resource.Core
{
    /// <summary>
    /// 简化的资源服务接口
    /// 专注于YooAsset初始化，实际资源加载通过ResourceLoader完成
    /// </summary>
    public interface IResourceService
    {
        /// <summary>
        /// 初始化资源服务（YooAsset初始化）
        /// </summary>
        UniTask InitializeAsync();
    }
}