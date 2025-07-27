namespace Game.Infrastructure.Resource.Configuration
{
    /// <summary>
    /// 资源系统配置类
    /// </summary>
    public static class ResourceConfig
    {
        /// <summary>
        /// 远程资源服务器URL
        /// </summary>
        public static string RemoteURL = "127.0.0.1:10002";
        
        /// <summary>
        /// YooAsset包名
        /// </summary>
        public static string PackageName = "DefaultPackage";
        
        /// <summary>
        /// 是否启用热更新
        /// </summary>
        public static bool EnableHotUpdate = false;
    }
}