using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.UI.Core;
using UnityEngine;

namespace Game.UI
{
    public class HotFixPanel : UIPanel
    {
        [SerializeField] private RectTransform progress;
        protected override UniTask OnShow(params object[] args)
        {
            progress.sizeDelta = new Vector2(0, 25);
            return base.OnShow(args);
        }

        public void UpdateProgress(float value)
        {
            progress.DOSizeDelta(new Vector2(value * 1600, 25), 0.25f);
        }
    }
}

