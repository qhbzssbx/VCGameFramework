using UnityEngine;
using YooAsset;

namespace Game.Infrastructure.Resource.Configuration
{
    /// <summary>
    /// YooAsset配置
    /// </summary>
    [CreateAssetMenu(fileName = "YooAssetConfig", menuName = "Game/Resource/CustomYooAsset Config")]
    public class YooAssetConfig : ScriptableObject
    {
        [Header("运行模式")]
        [Tooltip("编辑器模式：EditorSimulateMode\n离线模式：OfflinePlayMode\n联机模式：HostPlayMode\nWebGL模式：WebPlayMode")]
        public EPlayMode playMode = EPlayMode.EditorSimulateMode;
        
        [Header("包设置")]
        public string packageName = "DefaultPackage";
        
        [Header("服务器配置（联机模式使用）")]
        public string defaultHostServer = "http://127.0.0.1/CDN/Android/v1.0";
        public string fallbackHostServer = "http://127.0.0.1/CDN/Android/v1.0";
        
        [Header("WebGL配置")]
        public string webServerUrl = "http://127.0.0.1/WebGL/v1.0";
        
        [Header("调试设置")]
        public bool enableLog = true;
        public bool simulateLoadDelay = false;
        [Range(0.1f, 5.0f)]
        public float simulateDelayTime = 1.0f;
        
        /// <summary>
        /// 获取当前平台的服务器地址
        /// </summary>
        public string GetPlatformHostServer()
        {
#if UNITY_ANDROID
            return defaultHostServer.Replace("Android", "Android");
#elif UNITY_IOS
            return defaultHostServer.Replace("Android", "iOS");
#elif UNITY_STANDALONE_WIN
            return defaultHostServer.Replace("Android", "StandaloneWindows64");
#elif UNITY_STANDALONE_OSX
            return defaultHostServer.Replace("Android", "StandaloneOSX");
#elif UNITY_WEBGL
            return webServerUrl;
#else
            return defaultHostServer;
#endif
        }
    }
}