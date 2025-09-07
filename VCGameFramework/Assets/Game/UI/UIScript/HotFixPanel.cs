using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Core.UI;
using Game.UI.Core;
using UnityEngine;

namespace Game.UI
{
    public class HotFixPanel : UIPanelBase<EmptyUIParams>
    {
        [SerializeField] private RectTransform progress;

        public void UpdateProgress(float value)
        {
            progress.DOSizeDelta(new Vector2(value * 1600, 25), 0.25f);
        }

        protected override UniTask OnShowAsync(in EmptyUIParams args)
        {
            progress.sizeDelta = new Vector2(0, 25);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnOpenAsync(in EmptyUIParams args)
        {
            progress.sizeDelta = new Vector2(0, 25);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHideAsync()
        {
            return UniTask.CompletedTask;
        }
    }
}

