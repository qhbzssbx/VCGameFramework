using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace GameScripts.Modules.Scene.Runtime
{
    /// <summary>
    /// 自定义场景管理器：用于异步加载和激活场景
    /// </summary>
    public class CustomSceneManager
    {
        private AsyncOperation preloadOperation;
        private string preloadSceneName;

        public CustomSceneManager()
        {

        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="onComplete">完成后回调</param>
        public async UniTaskVoid LoadSceneAsync(string sceneName, Action onComplete = null)
        {
            Debug.Log($"开始加载场景: {sceneName}");

            preloadSceneName = sceneName;

            preloadOperation = SceneManager.LoadSceneAsync(sceneName);
            preloadOperation.allowSceneActivation = true;

            while (!preloadOperation.isDone)
            {
                Debug.Log($"加载进度: {preloadOperation.progress * 100f:0.0}%");
                await UniTask.Yield(); // 每帧等待
            }

            Debug.Log("场景加载完成");

            onComplete?.Invoke();
        }

        /// <summary>
        /// 手动激活已加载场景
        /// </summary>
        public async UniTask ActivateLoadedScene()
        {
            if (preloadOperation == null)
            {
                Debug.LogWarning("当前没有已预加载的场景");
                return;
            }

            preloadOperation.allowSceneActivation = true;

            // 等待切换完成
            while (!preloadOperation.isDone)
            {
                await UniTask.Yield();
            }

            Debug.Log($"场景 [{preloadSceneName}] 激活完成");
            preloadOperation = null;
            preloadSceneName = null;
        }
    }
}

