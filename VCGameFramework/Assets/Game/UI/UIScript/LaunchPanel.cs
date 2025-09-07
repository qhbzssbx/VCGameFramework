using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.UI.Core;
using System.Collections;
using System.Collections.Generic;
using Game.Core.UI;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class LaunchPanel : UIPanelBase<EmptyUIParams>
    {
        public TMP_Text tMP_Text;

        public UniTask PlayAnim()
        {
            var tcs = new UniTaskCompletionSource();
            
            tMP_Text.DOText("GameStart", 0.2f)
                .SetRelative()
                .SetEase(Ease.Linear)
                .SetAutoKill(true)  // 自动清理Tween
                .OnComplete(() => tcs.TrySetResult());
            
            return tcs.Task;
        }

        protected override UniTask OnShowAsync(in EmptyUIParams args)
        {
            return UniTask.CompletedTask;
        }

        protected override UniTask OnOpenAsync(in EmptyUIParams args)
        {
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHideAsync()
        {
            return UniTask.CompletedTask;
        }
    }
}

