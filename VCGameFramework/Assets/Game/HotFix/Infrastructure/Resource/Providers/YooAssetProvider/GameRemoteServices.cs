using UnityEngine;
using YooAsset;

namespace Game.Infrastructure.Resource.Providers.YooAssetProvider
{
    /// <summary>
    /// 游戏远程服务，用于YooAsset的远程资源获取
    /// </summary>
    public class GameRemoteServices : IRemoteServices
    {
        private readonly string _mainURL;
        private readonly string _fallbackURL;

        /// <summary>
        /// 构造函数（单服务器）
        /// </summary>
        /// <param name="remoteURL">远程URL根地址</param>
        public GameRemoteServices(string remoteURL)
        {
            _mainURL = remoteURL?.TrimEnd('/') ?? "";
            _fallbackURL = _mainURL; // 备用URL与主URL相同
        }

        /// <summary>
        /// 构造函数（主服务器和备用服务器）
        /// </summary>
        /// <param name="mainURL">主服务器URL</param>
        /// <param name="fallbackURL">备用服务器URL</param>
        public GameRemoteServices(string mainURL, string fallbackURL)
        {
            _mainURL = mainURL?.TrimEnd('/') ?? "";
            _fallbackURL = fallbackURL?.TrimEnd('/') ?? _mainURL;
        }

        /// <summary>
        /// 获取远程文件的完整URL
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>完整的URL</returns>
        public string GetRemoteMainURL(string fileName)
        {
            if (string.IsNullOrEmpty(_mainURL))
            {
                Debug.LogWarning("Main remote URL is not configured");
                return fileName;
            }

            var url = $"{_mainURL}/{fileName}";
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
            if (string.IsNullOrEmpty(_fallbackURL))
            {
                Debug.LogWarning("Fallback remote URL is not configured, using main URL");
                return GetRemoteMainURL(fileName);
            }

            var url = $"{_fallbackURL}/{fileName}";
            Debug.Log($"Remote fallback URL: {url}");
            return url;
        }
    }
}