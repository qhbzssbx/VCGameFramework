namespace Game.Modules.Global.Domain
{
    using Cysharp.Threading.Tasks;

    /// <summary>
    /// 玩家账户相关的服务接口
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// 登录账户
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        UniTask LoginAsync(string username, string password);

        /// <summary>
        /// 登出账户
        /// </summary>
        void Logout();
    }
}
