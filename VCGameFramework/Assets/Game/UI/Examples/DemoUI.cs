// using UnityEngine;
// using UnityEngine.UI;
// using Cysharp.Threading.Tasks;
// using Game.UI.Core;
// using Game.Core.UI;
// using TMPro;
//
// namespace Game.UI.Examples
// {
//     /// <summary>
//     /// Demo UI面板
//     /// 展示新UI系统的基本使用方法
//     /// </summary>
//     public class DemoUI : UIPanel
//     {
//         [Header("Demo UI组件")]
//         [SerializeField] private Button closeButton;
//         [SerializeField] private Button openPopupButton;
//         [SerializeField] private TMP_Text titleText;
//         [SerializeField] private TMP_Text descriptionText;
//         
//         private string demoMessage;
//         
//         protected override void Initialize()
//         {
//             base.Initialize();
//             
//             // 绑定按钮事件
//             if (closeButton != null)
//             {
//                 closeButton.onClick.AddListener(OnCloseButtonClicked);
//             }
//             
//             if (openPopupButton != null)
//             {
//                 openPopupButton.onClick.AddListener(OnOpenPopupButtonClicked);
//             }
//         }
//         
//         protected override async UniTask OnBeforeShow(params object[] args)
//         {
//             await base.OnBeforeShow(args);
//             
//             // 处理传入的参数
//             if (args.Length > 0 && args[0] is string message)
//             {
//                 demoMessage = message;
//             }
//             else
//             {
//                 demoMessage = "这是一个Demo UI面板";
//             }
//             
//             // 设置UI内容
//             if (titleText != null)
//                 titleText.text = "Demo UI";
//                 
//             if (descriptionText != null)
//                 descriptionText.text = demoMessage;
//             
//             Debug.Log($"DemoUI 准备显示: {demoMessage}");
//         }
//         
//         protected override async UniTask OnShow(params object[] args)
//         {
//             await base.OnShow(args);
//             Debug.Log("DemoUI 显示完成");
//         }
//         
//         protected override async UniTask OnBeforeHide()
//         {
//             await base.OnBeforeHide();
//             Debug.Log("DemoUI 准备隐藏");
//         }
//         
//         protected override async UniTask OnHide()
//         {
//             await base.OnHide();
//             Debug.Log("DemoUI 隐藏完成");
//         }
//         
//         private void OnCloseButtonClicked()
//         {
//             Debug.Log("关闭按钮被点击");
//             RequestClose();
//         }
//         
//         private void OnOpenPopupButtonClicked()
//         {
//             Debug.Log("打开弹窗按钮被点击");
//             // 这里可以打开另一个UI作为弹窗
//             UIManagerService.Instance.ShowAsync<DemoPopupUI>("这是一个弹窗消息").Forget();
//         }
//         
//         protected override void OnDestroy()
//         {
//             // 清理按钮事件
//             if (closeButton != null)
//             {
//                 closeButton.onClick.RemoveAllListeners();
//             }
//             
//             if (openPopupButton != null)
//             {
//                 openPopupButton.onClick.RemoveAllListeners();
//             }
//             
//             base.OnDestroy();
//         }
//     }
//     
//     /// <summary>
//     /// Demo弹窗UI
//     /// 展示弹窗层的使用
//     /// </summary>
//     public class DemoPopupUI : UIPanel
//     {
//         [Header("Popup组件")]
//         [SerializeField] private Button confirmButton;
//         [SerializeField] private Button cancelButton;
//         [SerializeField] private Text messageText;
//         
//         protected override void Initialize()
//         {
//             base.Initialize();
//             
//             // 设置为弹窗层级
//             uiLayer = Game.Core.UI.UILayer.Popup;
//             isModal = true; // 模态显示
//             
//             // 绑定按钮事件
//             if (confirmButton != null)
//             {
//                 confirmButton.onClick.AddListener(OnConfirmButtonClicked);
//             }
//             
//             if (cancelButton != null)
//             {
//                 cancelButton.onClick.AddListener(OnCancelButtonClicked);
//             }
//         }
//         
//         protected override async UniTask OnBeforeShow(params object[] args)
//         {
//             await base.OnBeforeShow(args);
//             
//             // 设置弹窗消息
//             string message = args.Length > 0 && args[0] is string msg ? msg : "确认执行操作？";
//             
//             if (messageText != null)
//                 messageText.text = message;
//                 
//             Debug.Log($"DemoPopupUI 准备显示: {message}");
//         }
//         
//         protected override async UniTask OnShow(params object[] args)
//         {
//             await base.OnShow(args);
//             Debug.Log("DemoPopupUI 显示完成");
//         }
//         
//         private void OnConfirmButtonClicked()
//         {
//             Debug.Log("确认按钮被点击");
//             RequestClose();
//         }
//         
//         private void OnCancelButtonClicked()
//         {
//             Debug.Log("取消按钮被点击");
//             RequestClose();
//         }
//         
//         protected override void OnDestroy()
//         {
//             // 清理按钮事件
//             if (confirmButton != null)
//             {
//                 confirmButton.onClick.RemoveAllListeners();
//             }
//             
//             if (cancelButton != null)
//             {
//                 cancelButton.onClick.RemoveAllListeners();
//             }
//             
//             base.OnDestroy();
//         }
//     }
// }