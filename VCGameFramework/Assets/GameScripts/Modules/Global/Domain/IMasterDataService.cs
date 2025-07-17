namespace Game.Modules.Global.Domain
{
    using Cysharp.Threading.Tasks;

    /// <summary>
    /// 静态数据加载服务接口
    /// </summary>
    public interface IMasterDataService
    {
        /// <summary>
        /// 异步加载配置数据
        /// </summary>
        UniTask LoadAsync();
    }
}
