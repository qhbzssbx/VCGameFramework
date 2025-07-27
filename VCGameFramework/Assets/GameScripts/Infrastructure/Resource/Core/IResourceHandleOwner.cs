namespace Game.Infrastructure.Resource.Core
{
    /// <summary>
    /// 资源Handle持有者接口（简化版本）
    /// 实现此接口可获得更高性能的资源自动释放
    /// </summary>
    public interface IResourceHandleOwner
    {
        /// <summary>
        /// 注册需要自动释放的资源Handle
        /// 当持有者销毁时，应该调用所有注册Handle的Dispose方法
        /// </summary>
        /// <param name="handle">需要自动释放的资源Handle</param>
        void RegisterHandleForAutoRelease(IResourceHandle handle);
    }
}