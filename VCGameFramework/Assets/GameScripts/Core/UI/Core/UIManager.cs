using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using Cysharp.Threading.Tasks;

namespace GameScript.Core.UI.Core
{
    /// <summary>
    /// UI 管理器：负责 UI 的加载、缓存、关闭等操作
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private Dictionary<Type, UILogic> uiInstances = new();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public async UniTask<T> OpenUI<T>(string assetKey, params object[] args) where T : UILogic
        {
            return (T)await OpenUI(assetKey, typeof(T), args);
        }

        public async UniTask<UILogic> OpenUI(string assetKey, Type logicType, params object[] args)
        {
            if (uiInstances.TryGetValue(logicType, out var existing))
                return existing;

            var handle = YooAssets.LoadAssetAsync<GameObject>(assetKey);
            await handle.ToUniTask();

            if (handle.AssetObject == null)
            {
                Debug.LogError($"UI资源加载失败: {assetKey}");
                return null;
            }

            GameObject prefab = handle.AssetObject as GameObject;
            GameObject go = Instantiate(prefab);
            UILogic uiLogic = go.GetComponent(logicType) as UILogic ?? (UILogic)go.AddComponent(logicType);

            uiLogic.OnOpen(args);
            uiInstances[logicType] = uiLogic;
            return uiLogic;
        }

        public void CloseUI<T>() where T : UILogic
        {
            var type = typeof(T);
            if (uiInstances.TryGetValue(type, out var ui))
            {
                ui.OnClose();
                uiInstances.Remove(type);
            }
        }

        public bool IsOpen<T>() where T : UILogic => uiInstances.ContainsKey(typeof(T));

        public T GetUI<T>() where T : UILogic => uiInstances.TryGetValue(typeof(T), out var ui) ? ui as T : null;
    }
}
