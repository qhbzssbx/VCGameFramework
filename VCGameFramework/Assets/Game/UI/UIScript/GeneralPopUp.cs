using Cysharp.Threading.Tasks;
using Game.UI.Core;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class GeneralPopUp : UIPanel
    {
        [SerializeField] private TMP_Text content;
        [SerializeField] private Button btnOk;
        [SerializeField] private Button btnCancel;

        private Action OnClickBtnOkCallBack;
        private Action OnClickBtnCancelCallback;

        protected override void Initialize()
        {
            base.Initialize();

            btnOk.onClick.AddListener(OnClickBtnOK);
            btnCancel.onClick.AddListener(OnClickBtnCancel);
        }
        protected override UniTask OnBeforeShow(params object[] args)
        {
            content.text = args[0] as string;
            //OnClickBtnOkCallBack = args[1] as Action;
            //OnClickBtnCancelCallback = args[2] as Action;

            return base.OnBeforeShow(args);
        }

        private void OnClickBtnOK()
        {
            OnClickBtnOkCallBack?.Invoke();
            handle.Close();
        }

        private void OnClickBtnCancel()
        {
            OnClickBtnCancelCallback?.Invoke();
            handle.Close();
        }

        protected override UniTask OnBeforeHide()
        {
            OnClickBtnOkCallBack = null;
            OnClickBtnCancelCallback = null;
            return base.OnBeforeHide();
        }


        public void SetContent(string content)
        {
            this.content.text = content;
        }

        public void SetOkCallBack(Action action)
        {
            OnClickBtnOkCallBack = action;
        }

        public void SetCancelCallBack(Action action)
        {
            OnClickBtnCancelCallback = action;
        }

    }
}


