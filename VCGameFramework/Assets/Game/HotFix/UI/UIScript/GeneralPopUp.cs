using Cysharp.Threading.Tasks;
using Game.UI.Core;
using System;
using Game.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public struct GeneralPopUpParams : IUIParams
    {
        public string content;
        public Action onClickOk;
        public Action onClickCancel;

        public GeneralPopUpParams(string content, Action onClickOk = null, Action onClickCancel = null)
        {
            this.content = content;
            this.onClickOk = onClickOk;
            this.onClickCancel = onClickCancel;
        }
    }
    
    public class GeneralPopUp : UIPanelBase<GeneralPopUpParams>
    {
        [SerializeField] private TMP_Text content;
        [SerializeField] private Button btnOk;
        [SerializeField] private Button btnCancel;

        private Action OnClickBtnOkCallBack;
        private Action OnClickBtnCancelCallback;


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

        protected override UniTask OnShowAsync(in GeneralPopUpParams args)
        {
            
            content.text = args.content;
            OnClickBtnOkCallBack = args.onClickCancel;
            OnClickBtnOkCallBack = args.onClickOk;

            btnOk.onClick.AddListener(OnClickBtnOK);
            btnCancel.onClick.AddListener(OnClickBtnCancel);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnOpenAsync(in GeneralPopUpParams args)
        {
            content.text = args.content;
            OnClickBtnOkCallBack = args.onClickCancel;
            OnClickBtnOkCallBack = args.onClickOk;
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHideAsync()
        {
            OnClickBtnOkCallBack = null;
            OnClickBtnCancelCallback = null;
            
            return UniTask.CompletedTask;
        }
    }
}


