namespace Game.Modules.Global.Domain
{
    using Cysharp.Threading.Tasks;

    /// <summary>
    /// 网络通信服务接口
    /// </summary>
    public interface INetworkService
    {
        /// <summary>
        /// 建立连接
        /// </summary>
        UniTask ConnectAsync();

        /// <summary>
        /// 发送消息
        /// </summary>
        UniTask SendAsync(string message);
    }
}
