using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Core.UI
{
    /// <summary>
    /// 默认UI控制句柄实现
    /// 提供标准的UI面板关闭控制功能
    /// </summary>
    public class DefaultUIHandle : IUIHandle
    {
        private readonly IUIPanel _panel;
        private readonly IUIManager _uiManager;
        private readonly Type _panelType;
        private bool _canClose = true;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="panel">关联的UI面板</param>
        /// <param name="uiManager">UI管理器</param>
        /// <param name="panelType">面板类型</param>
        public DefaultUIHandle(IUIPanel panel, IUIManager uiManager, Type panelType)
        {
            _panel = panel ?? throw new ArgumentNullException(nameof(panel));
            _uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
            _panelType = panelType ?? throw new ArgumentNullException(nameof(panelType));
        }
        
        /// <summary>
        /// 是否可以关闭
        /// </summary>
        public bool CanClose
        {
            get => _canClose;
            set
            {
                _canClose = value;
                Debug.Log($"[DefaultUIHandle] {_panelType.Name} CanClose设置为: {value}");
            }
        }
        
        /// <summary>
        /// 关闭请求事件
        /// </summary>
        public event Action<IUIHandle> OnCloseRequested;
        
        /// <summary>
        /// 同步关闭UI
        /// </summary>
        public void Close()
        {
            if (!CanClose)
            {
                Debug.LogWarning($"[DefaultUIHandle] {_panelType.Name} 当前不允许关闭");
                return;
            }
            
            OnCloseRequested?.Invoke(this);
            CloseAsync().Forget();
        }
        
        /// <summary>
        /// 异步关闭UI
        /// </summary>
        public async UniTask CloseAsync()
        {
            if (!CanClose)
            {
                Debug.LogWarning($"[DefaultUIHandle] {_panelType.Name} 当前不允许关闭");
                return;
            }
            
            try
            {
                OnCloseRequested?.Invoke(this);
                
                if (_panel != null && _panel.IsShowing)
                {
                    await _panel.HideAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DefaultUIHandle] 关闭UI时发生错误 {_panelType.Name}: {ex}");
            }
        }
        
        /// <summary>
        /// 带返回值的异步关闭UI
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="result">返回值</param>
        public async UniTask<TResult> CloseWithResultAsync<TResult>(TResult result)
        {
            await CloseAsync();
            return result;
        }
        
        /// <summary>
        /// 设置关闭守卫
        /// </summary>
        /// <param name="guard">守卫函数，返回true表示可以关闭</param>
        public void SetCloseGuard(Func<bool> guard)
        {
            if (guard != null)
            {
                var originalCanClose = _canClose;
                _canClose = originalCanClose && guard();
            }
        }
    }
}