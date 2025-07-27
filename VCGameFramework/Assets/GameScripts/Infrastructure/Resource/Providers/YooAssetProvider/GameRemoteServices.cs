using UnityEngine;
using YooAsset;

namespace Game.Infrastructure.Resource.Providers.YooAssetProvider
{
    /// <summary>
    /// 游戏远程服务，用于YooAsset的远程资源获取
    /// </summary>
    public class GameRemoteServices : IRemoteServices
    {
        private readonly string _remoteURL;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="remoteURL">远程URL根地址</param>
        public GameRemoteServices(string remoteURL)
        {
            _remoteURL = remoteURL?.TrimEnd('/') ?? "";
        }

        /// <summary>
        /// 获取远程文件的完整URL
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>完整的URL</returns>
        public string GetRemoteMainURL(string fileName)
        {
            if (string.IsNullOrEmpty(_remoteURL))
            {
                Debug.LogWarning("Remote URL is not configured");
                return fileName;
            }

            var url = $"{_remoteURL}/{fileName}";
            Debug.Log($"Remote main URL: {url}");
            return url;
        }

        /// <summary>
        /// 获取远程文件的备用URL
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>备用URL</returns>
        public string GetRemoteFallbackURL(string fileName)
        {
            // 简单实现：备用URL与主URL相同
            // 在实际项目中，可以配置多个CDN地址作为备用
            return GetRemoteMainURL(fileName);
        }
    }
}