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
        /// <returns>连接是否成功</returns>
        UniTask<bool> ConnectAsync();

        /// <summary>
        /// 发送消息
        /// </summary>
        UniTask SendAsync(string message);
    }
}
