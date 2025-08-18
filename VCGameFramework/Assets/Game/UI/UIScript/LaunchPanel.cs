using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.UI.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class LaunchPanel : UIPanel
    {
        public TMP_Text tMP_Text;

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override UniTask OnShow(params object[] args)
        {

            return base.OnShow(args);
        }

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
    }
}

