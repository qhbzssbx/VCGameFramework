using Cysharp.Threading.Tasks;
using Game.Core.UI;
using UnityEngine;

namespace Game.UI.Core
{
    public abstract class UIPanelBase<TParams> : MonoBehaviour, IUIPanel<TParams>
        where TParams : struct, IUIParams
    {
        public UILayer Layer { get; protected set; }
        protected IUIHandle handle;
        public bool IsShowing { get; protected set; }
        public UniTask ShowAsync(in TParams args)
        {
            IsShowing = true;
            return OnShowAsync(args);
        }
        public UniTask OpenAsync(in TParams args)
        {
            IsShowing = true;
            return OnOpenAsync(args);
        }
        public UniTask HideAsync()
        {
            IsShowing = false;
            return OnHideAsync();
        }

        public void SetHandle(IUIHandle handle)
        {
            this.handle = handle;
        }

        protected abstract UniTask OnShowAsync(in TParams args);
        protected abstract UniTask OnOpenAsync(in TParams args);
        protected abstract UniTask OnHideAsync();
    }
}
