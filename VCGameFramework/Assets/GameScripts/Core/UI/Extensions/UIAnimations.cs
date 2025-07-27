using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GameScript.Core.UI.Extensions
{
    /// <summary>
    /// UI动画扩展
    /// 提供常用的UI显示/隐藏动画效果
    /// </summary>
    public static class UIAnimations
    {
        /// <summary>
        /// 默认动画持续时间
        /// </summary>
        public const float DefaultDuration = 0.3f;
        
        #region 淡入淡出动画
        
        /// <summary>
        /// 淡入动画
        /// </summary>
        /// <param name="target">目标物体</param>
        /// <param name="duration">动画时长</param>
        /// <param name="fromAlpha">起始透明度</param>
        /// <param name="toAlpha">结束透明度</param>
        public static async UniTask FadeIn(GameObject target, float duration = DefaultDuration, float fromAlpha = 0f, float toAlpha = 1f)
        {
            var canvasGroup = GetOrAddCanvasGroup(target);
            canvasGroup.alpha = fromAlpha;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            await canvasGroup.DOFade(toAlpha, duration)
                .SetEase(Ease.OutQuart)
                .ToUniTask();
                
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        /// <summary>
        /// 淡出动画
        /// </summary>
        /// <param name="target">目标物体</param>
        /// <param name="duration">动画时长</param>
        /// <param name="fromAlpha">起始透明度</param>
        /// <param name="toAlpha">结束透明度</param>
        public static async UniTask FadeOut(GameObject target, float duration = DefaultDuration, float fromAlpha = 1f, float toAlpha = 0f)
        {
            var canvasGroup = GetOrAddCanvasGroup(target);
            canvasGroup.alpha = fromAlpha;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            await canvasGroup.DOFade(toAlpha, duration)
                .SetEase(Ease.InQuart)
                .ToUniTask();
        }
        
        #endregion
        
        #region 缩放动画
        
        /// <summary>
        /// 缩放进入动画
        /// </summary>
        /// <param name="target">目标物体</param>
        /// <param name="duration">动画时长</param>
        /// <param name="fromScale">起始缩放</param>
        /// <param name="toScale">结束缩放</param>
        public static async UniTask ScaleIn(GameObject target, float duration = DefaultDuration, float fromScale = 0.8f, float toScale = 1f)
        {
            var transform = target.transform;
            transform.localScale = Vector3.one * fromScale;
            
            var canvasGroup = GetOrAddCanvasGroup(target);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            await transform.DOScale(toScale, duration)
                .SetEase(Ease.OutBack)
                .ToUniTask();
                
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        /// <summary>
        /// 缩放退出动画
        /// </summary>
        /// <param name="target">目标物体</param>
        /// <param name="duration">动画时长</param>
        /// <param name="fromScale">起始缩放</param>
        /// <param name="toScale">结束缩放</param>
        public static async UniTask ScaleOut(GameObject target, float duration = DefaultDuration, float fromScale = 1f, float toScale = 0.8f)
        {
            var transform = target.transform;
            transform.localScale = Vector3.one * fromScale;
            
            var canvasGroup = GetOrAddCanvasGroup(target);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            await transform.DOScale(toScale, duration)
                .SetEase(Ease.InBack)
                .ToUniTask();
        }
        
        #endregion
        
        #region 滑动动画
        
        /// <summary>
        /// 从左侧滑入
        /// </summary>
        /// <param name="target">目标物体</param>
        /// <param name="duration">动画时长</param>
        /// <param name="offset">偏移距离（负数表示从屏幕外进入）</param>
        public static async UniTask SlideInFromLeft(GameObject target, float duration = DefaultDuration, float offset = -1000f)
        {
            var rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null) return;
            
            var originalPosition = rectTransform.anchoredPosition;
            var startPosition = originalPosition + Vector2.left * Mathf.Abs(offset);
            
            rectTransform.anchoredPosition = startPosition;
            
            var canvasGroup = GetOrAddCanvasGroup(target);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            await rectTransform.DOAnchorPos(originalPosition, duration)
                .SetEase(Ease.OutQuart)
                .ToUniTask();
                
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        /// <summary>
        /// 向右侧滑出
        /// </summary>
        /// <param name="target">目标物体</param>
        /// <param name="duration">动画时长</param>
        /// <param name="offset">偏移距离</param>
        public static async UniTask SlideOutToRight(GameObject target, float duration = DefaultDuration, float offset = 1000f)
        {
            var rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null) return;
            
            var originalPosition = rectTransform.anchoredPosition;
            var endPosition = originalPosition + Vector2.right * Mathf.Abs(offset);
            
            var canvasGroup = GetOrAddCanvasGroup(target);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            await rectTransform.DOAnchorPos(endPosition, duration)
                .SetEase(Ease.InQuart)
                .ToUniTask();
        }
        
        /// <summary>
        /// 从上方滑入
        /// </summary>
        /// <param name="target">目标物体</param>
        /// <param name="duration">动画时长</param>
        /// <param name="offset">偏移距离</param>
        public static async UniTask SlideInFromTop(GameObject target, float duration = DefaultDuration, float offset = 1000f)
        {
            var rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null) return;
            
            var originalPosition = rectTransform.anchoredPosition;
            var startPosition = originalPosition + Vector2.up * Mathf.Abs(offset);
            
            rectTransform.anchoredPosition = startPosition;
            
            var canvasGroup = GetOrAddCanvasGroup(target);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            await rectTransform.DOAnchorPos(originalPosition, duration)
                .SetEase(Ease.OutQuart)
                .ToUniTask();
                
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        /// <summary>
        /// 向下方滑出
        /// </summary>
        /// <param name="target">目标物体</param>
        /// <param name="duration">动画时长</param>
        /// <param name="offset">偏移距离</param>
        public static async UniTask SlideOutToBottom(GameObject target, float duration = DefaultDuration, float offset = -1000f)
        {
            var rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null) return;
            
            var originalPosition = rectTransform.anchoredPosition;
            var endPosition = originalPosition + Vector2.down * Mathf.Abs(offset);
            
            var canvasGroup = GetOrAddCanvasGroup(target);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            await rectTransform.DOAnchorPos(endPosition, duration)
                .SetEase(Ease.InQuart)
                .ToUniTask();
        }
        
        #endregion
        
        #region 组合动画
        
        /// <summary>
        /// 弹出显示动画（缩放+淡入）
        /// </summary>
        /// <param name="target">目标物体</param>
        /// <param name="duration">动画时长</param>
        public static async UniTask PopIn(GameObject target, float duration = DefaultDuration)
        {
            var transform = target.transform;
            var canvasGroup = GetOrAddCanvasGroup(target);
            
            // 设置初始状态
            transform.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            // 同时执行缩放和淡入动画
            var scaleTask = transform.DOScale(1f, duration).SetEase(Ease.OutBack).ToUniTask();
            var fadeTask = canvasGroup.DOFade(1f, duration).SetEase(Ease.OutQuart).ToUniTask();
            
            await UniTask.WhenAll(scaleTask, fadeTask);
            
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        /// <summary>
        /// 弹出隐藏动画（缩放+淡出）
        /// </summary>
        /// <param name="target">目标物体</param>
        /// <param name="duration">动画时长</param>
        public static async UniTask PopOut(GameObject target, float duration = DefaultDuration)
        {
            var transform = target.transform;
            var canvasGroup = GetOrAddCanvasGroup(target);
            
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            // 同时执行缩放和淡出动画
            var scaleTask = transform.DOScale(0f, duration).SetEase(Ease.InBack).ToUniTask();
            var fadeTask = canvasGroup.DOFade(0f, duration).SetEase(Ease.InQuart).ToUniTask();
            
            await UniTask.WhenAll(scaleTask, fadeTask);
        }
        
        #endregion
        
        #region 工具方法
        
        /// <summary>
        /// 获取或添加CanvasGroup组件
        /// </summary>
        /// <param name="target">目标物体</param>
        /// <returns>CanvasGroup组件</returns>
        private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
        {
            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = target.AddComponent<CanvasGroup>();
            }
            return canvasGroup;
        }
        
        /// <summary>
        /// 停止目标物体的所有DOTween动画
        /// </summary>
        /// <param name="target">目标物体</param>
        public static void StopAllAnimations(GameObject target)
        {
            target.transform.DOKill();
            
            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
            }
            
            var rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.DOKill();
            }
        }
        
        /// <summary>
        /// 立即完成目标物体的所有DOTween动画
        /// </summary>
        /// <param name="target">目标物体</param>
        public static void CompleteAllAnimations(GameObject target)
        {
            target.transform.DOComplete();
            
            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.DOComplete();
            }
            
            var rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.DOComplete();
            }
        }
        
        #endregion
    }
}