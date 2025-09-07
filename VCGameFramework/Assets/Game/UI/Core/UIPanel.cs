// using System;
// using UnityEngine;
// using Cysharp.Threading.Tasks;
// using Game.Infrastructure.Resource.Core;
// using GameScript.Core.UI.Extensions;
// using Game.Core.UI;
//
// namespace Game.UI.Core
// {
//     /// <summary>
//     /// UI动画类型
//     /// </summary>
//     public enum UIAnimationType
//     {
//         None,           // 无动画
//         Fade,           // 淡入淡出
//         Scale,          // 缩放
//         SlideFromLeft,  // 从左滑入
//         SlideFromRight, // 从右滑入
//         SlideFromTop,   // 从上滑入
//         SlideFromBottom,// 从下滑入
//         PopInOut        // 弹出效果
//     }
//
//     /// <summary>
//     /// UI面板基类
//     /// 为所有UI面板提供基础功能和生命周期管理
//     /// </summary>
//     public abstract class UIPanel : MonoBehaviour, IUIPanel
//     {
//         [Header("UI配置")]
//         [SerializeField] protected Game.Core.UI.UILayer uiLayer = Game.Core.UI.UILayer.Window;
//         [SerializeField] protected bool isModal = false; // 是否模态显示
//         [SerializeField] protected bool autoDestroy = true; // 关闭时是否自动销毁
//         
//         [Header("动画配置")]
//         [SerializeField] protected UIAnimationType showAnimation = UIAnimationType.Fade;
//         [SerializeField] protected UIAnimationType hideAnimation = UIAnimationType.Fade;
//         [SerializeField] protected float animationDuration = 0.3f;
//         
//         /// <summary>
//         /// UI所属层级
//         /// </summary>
//         public Game.Core.UI.UILayer Layer => uiLayer;
//         
//         /// <summary>
//         /// 是否模态显示（阻止后面UI的交互）
//         /// </summary>
//         public bool IsModal => isModal;
//         
//         /// <summary>
//         /// 是否自动销毁
//         /// </summary>
//         public bool AutoDestroy => autoDestroy;
//         
//         /// <summary>
//         /// UI是否正在显示
//         /// </summary>
//         public bool IsShowing { get; private set; }
//         
//         /// <summary>
//         /// 资源加载器，用于UI相关资源管理
//         /// </summary>
//         protected ResourceLoader resourceLoader = new();
//         
//         /// <summary>
//         /// UI控制句柄
//         /// </summary>
//         protected IUIHandle handle;
//         
//         /// <summary>
//         /// UI显示事件
//         /// </summary>
//         public event Action<UIPanel> OnShowCallBack;
//         
//         /// <summary>
//         /// UI隐藏事件
//         /// </summary>
//         public event Action<UIPanel> OnHideCallBack;
//         
//         #region 生命周期方法
//         
//         /// <summary>
//         /// 显示UI
//         /// </summary>
//         /// <param name="args">传入参数</param>
//         public async UniTask ShowAsync(params object[] args)
//         {
//             if (IsShowing) return;
//             
//             gameObject.SetActive(true);
//             IsShowing = true;
//             
//             // 执行显示前的逻辑
//             await OnBeforeShow(args);
//             
//             // 播放显示动画
//             await PlayShowAnimation();
//             
//             // 执行显示后的逻辑
//             await OnShow(args);
//             
//             // 触发事件
//             this.OnShowCallBack?.Invoke(this);
//         }
//         
//         /// <summary>
//         /// 显示UI
//         /// </summary>
//         /// <param name="args">传入参数</param>
//         public async UniTask ShowAsync<T>(T? args = null) where T : struct, IUIParams
//         {
//             if (IsShowing) return;
//             
//             gameObject.SetActive(true);
//             IsShowing = true;
//             
//             // 执行显示前的逻辑
//             await OnBeforeShow(args);
//             
//             // 播放显示动画
//             await PlayShowAnimation();
//             
//             // 执行显示后的逻辑
//             await OnShow(args);
//             
//             // 触发事件
//             this.OnShowCallBack?.Invoke(this);
//         }
//         
//         /// <summary>
//         /// 隐藏UI
//         /// </summary>
//         public async UniTask HideAsync()
//         {
//             if (!IsShowing) return;
//             
//             IsShowing = false;
//             
//             // 执行隐藏前的逻辑
//             await OnBeforeHide();
//             
//             // 播放隐藏动画
//             await PlayHideAnimation();
//             
//             // 执行隐藏后的逻辑
//             await OnHide();
//             
//             // 触发事件
//             this.OnHideCallBack?.Invoke(this);
//             
//             // 根据配置决定是否销毁
//             if (autoDestroy)
//             {
//                 Destroy(gameObject);
//             }
//             else
//             {
//                 gameObject.SetActive(false);
//             }
//         }
//         
//         /// <summary>
//         /// 设置UI控制句柄
//         /// </summary>
//         /// <param name="handle">UI控制句柄</param>
//         public void SetHandle(IUIHandle handle)
//         {
//             this.handle = handle;
//             Debug.Log($"[UIPanel] {GetType().Name} 设置Handle: {handle?.GetType().Name}");
//         }
//         
//         /// <summary>
//         /// 请求关闭UI（使用Handle）
//         /// </summary>
//         protected void RequestClose()
//         {
//             if (handle != null)
//             {
//                 handle.Close();
//             }
//             else
//             {
//                 // 降级到直接隐藏
//                 Debug.LogWarning($"[UIPanel] {GetType().Name} 没有Handle，使用直接隐藏");
//                 HideAsync().Forget();
//             }
//         }
//         
//         #endregion
//         
//         #region 虚方法 - 子类重写
//         
//         /// <summary>
//         /// UI显示前调用 - 子类重写
//         /// 在动画播放前执行，用于设置初始状态
//         /// </summary>
//         /// <param name="args">传入参数</param>
//         protected virtual async UniTask OnBeforeShow(params object[] args)
//         {
//             await UniTask.CompletedTask;
//         }
//         
//         /// <summary>
//         /// UI显示时调用 - 子类重写
//         /// 在动画播放后执行，用于最终的逻辑处理
//         /// </summary>
//         /// <param name="args">传入参数</param>
//         protected virtual async UniTask OnShow(params object[] args)
//         {
//             await UniTask.CompletedTask;
//         }
//         
//         /// <summary>
//         /// UI隐藏前调用 - 子类重写
//         /// 在动画播放前执行
//         /// </summary>
//         protected virtual async UniTask OnBeforeHide()
//         {
//             await UniTask.CompletedTask;
//         }
//         
//         /// <summary>
//         /// UI隐藏时调用 - 子类重写
//         /// 在动画播放后执行
//         /// </summary>
//         protected virtual async UniTask OnHide()
//         {
//             await UniTask.CompletedTask;
//         }
//         
//         /// <summary>
//         /// UI初始化 - 子类重写
//         /// 用于设置UI组件引用、事件绑定等
//         /// </summary>
//         protected virtual void Initialize()
//         {
//         }
//         
//         #endregion
//         
//         #region Unity生命周期
//         
//         protected virtual void Awake()
//         {
//             Initialize();
//         }
//         
//         protected virtual void OnDestroy()
//         {
//             // 清理资源
//             resourceLoader?.Dispose();
//             
//             // 清理事件
//             OnShowCallBack = null;
//             OnHideCallBack = null;
//         }
//         
//         #endregion
//         
//         #region 工具方法
//         
//         /// <summary>
//         /// 设置UI层级
//         /// </summary>
//         /// <param name="layer">目标层级</param>
//         public void SetLayer(Game.Core.UI.UILayer layer)
//         {
//             uiLayer = layer;
//             
//             // 如果UI已经显示，立即更新Canvas层级
//             if (IsShowing)
//             {
//                 var canvas = GetComponent<Canvas>();
//                 if (canvas != null)
//                 {
//                     canvas.sortingOrder = (int)layer;
//                 }
//             }
//         }
//         
//         /// <summary>
//         /// 切换UI显示状态
//         /// </summary>
//         public async UniTask Toggle(params object[] args)
//         {
//             if (IsShowing)
//             {
//                 await HideAsync();
//             }
//             else
//             {
//                 await ShowAsync(args);
//             }
//         }
//         
//         /// <summary>
//         /// 播放显示动画
//         /// </summary>
//         protected virtual async UniTask PlayShowAnimation()
//         {
//             switch (showAnimation)
//             {
//                 case UIAnimationType.None:
//                     break;
//                 case UIAnimationType.Fade:
//                     await UIAnimations.FadeIn(gameObject, animationDuration);
//                     break;
//                 case UIAnimationType.Scale:
//                     await UIAnimations.ScaleIn(gameObject, animationDuration);
//                     break;
//                 case UIAnimationType.SlideFromLeft:
//                     await UIAnimations.SlideInFromLeft(gameObject, animationDuration);
//                     break;
//                 case UIAnimationType.SlideFromTop:
//                     await UIAnimations.SlideInFromTop(gameObject, animationDuration);
//                     break;
//                 case UIAnimationType.PopInOut:
//                     await UIAnimations.PopIn(gameObject, animationDuration);
//                     break;
//             }
//         }
//         
//         /// <summary>
//         /// 播放隐藏动画
//         /// </summary>
//         protected virtual async UniTask PlayHideAnimation()
//         {
//             switch (hideAnimation)
//             {
//                 case UIAnimationType.None:
//                     break;
//                 case UIAnimationType.Fade:
//                     await UIAnimations.FadeOut(gameObject, animationDuration);
//                     break;
//                 case UIAnimationType.Scale:
//                     await UIAnimations.ScaleOut(gameObject, animationDuration);
//                     break;
//                 case UIAnimationType.SlideFromLeft:
//                     await UIAnimations.SlideOutToRight(gameObject, animationDuration);
//                     break;
//                 case UIAnimationType.SlideFromTop:
//                     await UIAnimations.SlideOutToBottom(gameObject, animationDuration);
//                     break;
//                 case UIAnimationType.PopInOut:
//                     await UIAnimations.PopOut(gameObject, animationDuration);
//                     break;
//             }
//         }
//         
//         /// <summary>
//         /// 停止所有动画
//         /// </summary>
//         public void StopAllAnimations()
//         {
//             UIAnimations.StopAllAnimations(gameObject);
//         }
//         
//         /// <summary>
//         /// 完成所有动画
//         /// </summary>
//         public void CompleteAllAnimations()
//         {
//             UIAnimations.CompleteAllAnimations(gameObject);
//         }
//         
//         #endregion
//     }
// }