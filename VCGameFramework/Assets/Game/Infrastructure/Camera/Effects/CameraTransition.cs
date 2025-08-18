using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Game.Infrastructure.Camera.Effects
{
    /// <summary>
    /// 摄像机过渡效果
    /// </summary>
    public class CameraTransition : MonoBehaviour
    {
        [System.Serializable]
        public class TransitionParameters
        {
            [Header("过渡设置")]
            public float duration = 1f;
            public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            public TransitionType transitionType = TransitionType.Smooth;
            
            [Header("位置过渡")]
            public bool transitionPosition = true;
            public Vector3 targetPosition = Vector3.zero;
            
            [Header("旋转过渡")]
            public bool transitionRotation = true;
            public Vector3 targetRotation = Vector3.zero;
            
            [Header("FOV过渡")]
            public bool transitionFOV = false;
            public float targetFOV = 60f;
            
            [Header("正交大小过渡")]
            public bool transitionOrthographicSize = false;
            public float targetOrthographicSize = 5f;
            
            [Header("特殊效果")]
            public bool enableFadeEffect = false;
            public Color fadeColor = Color.black;
            public float fadeInDuration = 0.3f;
            public float fadeOutDuration = 0.3f;
        }
        
        private UnityEngine.Camera targetCamera;
        private Coroutine transitionCoroutine;
        private bool isTransitioning = false;
        
        // 淡入淡出UI
        private GameObject fadeCanvas;
        private UnityEngine.UI.Image fadeImage;
        
        /// <summary>
        /// 当前是否在过渡中
        /// </summary>
        public bool IsTransitioning => isTransitioning;
        
        /// <summary>
        /// 过渡开始事件
        /// </summary>
        public System.Action OnTransitionStart;
        
        /// <summary>
        /// 过渡结束事件
        /// </summary>
        public System.Action OnTransitionEnd;
        
        /// <summary>
        /// 过渡进度事件 (0-1)
        /// </summary>
        public System.Action<float> OnTransitionProgress;
        
        #region Unity生命周期
        
        private void Awake()
        {
            targetCamera = GetComponent<UnityEngine.Camera>();
            CreateFadeCanvas();
        }
        
        private void OnDestroy()
        {
            DestroyFadeCanvas();
        }
        
        #endregion
        
        #region 公共接口
        
        /// <summary>
        /// 开始过渡
        /// </summary>
        /// <param name="parameters">过渡参数</param>
        public async UniTask StartTransition(TransitionParameters parameters)
        {
            if (parameters == null || targetCamera == null) return;
            
            StopTransition(); // 停止之前的过渡
            
            var tcs = new UniTaskCompletionSource();
            transitionCoroutine = StartCoroutine(TransitionCoroutine(parameters, tcs));
            
            await tcs.Task;
        }
        
        /// <summary>
        /// 过渡到指定位置和旋转
        /// </summary>
        /// <param name="position">目标位置</param>
        /// <param name="rotation">目标旋转</param>
        /// <param name="duration">过渡时间</param>
        /// <param name="curve">过渡曲线</param>
        public async UniTask TransitionTo(Vector3 position, Vector3 rotation, float duration = 1f, AnimationCurve curve = null)
        {
            var parameters = new TransitionParameters
            {
                duration = duration,
                easeCurve = curve ?? AnimationCurve.EaseInOut(0, 0, 1, 1),
                targetPosition = position,
                targetRotation = rotation,
                transitionPosition = true,
                transitionRotation = true
            };
            
            await StartTransition(parameters);
        }
        
        /// <summary>
        /// FOV过渡
        /// </summary>
        /// <param name="targetFOV">目标FOV</param>
        /// <param name="duration">过渡时间</param>
        public async UniTask TransitionFOV(float targetFOV, float duration = 1f)
        {
            var parameters = new TransitionParameters
            {
                duration = duration,
                targetFOV = targetFOV,
                transitionFOV = true,
                transitionPosition = false,
                transitionRotation = false
            };
            
            await StartTransition(parameters);
        }
        
        /// <summary>
        /// 带淡入淡出效果的过渡
        /// </summary>
        /// <param name="position">目标位置</param>
        /// <param name="rotation">目标旋转</param>
        /// <param name="duration">过渡时间</param>
        /// <param name="fadeColor">淡入淡出颜色</param>
        public async UniTask TransitionWithFade(Vector3 position, Vector3 rotation, float duration = 1f, Color? fadeColor = null)
        {
            var parameters = new TransitionParameters
            {
                duration = duration,
                targetPosition = position,
                targetRotation = rotation,
                transitionPosition = true,
                transitionRotation = true,
                enableFadeEffect = true,
                fadeColor = fadeColor ?? Color.black,
                fadeInDuration = 0.3f,
                fadeOutDuration = 0.3f
            };
            
            await StartTransition(parameters);
        }
        
        /// <summary>
        /// 停止过渡
        /// </summary>
        public void StopTransition()
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
                transitionCoroutine = null;
            }
            
            if (isTransitioning)
            {
                isTransitioning = false;
                HideFadeEffect();
                OnTransitionEnd?.Invoke();
            }
        }
        
        /// <summary>
        /// 设置目标摄像机
        /// </summary>
        /// <param name="camera">目标摄像机</param>
        public void SetTargetCamera(UnityEngine.Camera camera)
        {
            StopTransition();
            targetCamera = camera;
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 过渡协程
        /// </summary>
        private IEnumerator TransitionCoroutine(TransitionParameters parameters, UniTaskCompletionSource tcs)
        {
            if (targetCamera == null)
            {
                tcs.TrySetResult();
                yield break;
            }
            
            isTransitioning = true;
            OnTransitionStart?.Invoke();
            
            // 记录初始值
            Vector3 startPosition = targetCamera.transform.position;
            Vector3 startRotation = targetCamera.transform.eulerAngles;
            float startFOV = targetCamera.fieldOfView;
            float startOrthographicSize = targetCamera.orthographicSize;
            
            // 淡入效果
            if (parameters.enableFadeEffect)
            {
                yield return StartCoroutine(FadeIn(parameters.fadeColor, parameters.fadeInDuration));
            }
            
            float elapsed = 0f;
            
            while (elapsed < parameters.duration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / parameters.duration;
                float easedTime = parameters.easeCurve.Evaluate(normalizedTime);
                
                // 位置过渡
                if (parameters.transitionPosition)
                {
                    Vector3 currentPosition = Vector3.Lerp(startPosition, parameters.targetPosition, easedTime);
                    targetCamera.transform.position = currentPosition;
                }
                
                // 旋转过渡
                if (parameters.transitionRotation)
                {
                    Vector3 currentRotation = Vector3.Lerp(startRotation, parameters.targetRotation, easedTime);
                    targetCamera.transform.eulerAngles = currentRotation;
                }
                
                // FOV过渡
                if (parameters.transitionFOV && !targetCamera.orthographic)
                {
                    float currentFOV = Mathf.Lerp(startFOV, parameters.targetFOV, easedTime);
                    targetCamera.fieldOfView = currentFOV;
                }
                
                // 正交大小过渡
                if (parameters.transitionOrthographicSize && targetCamera.orthographic)
                {
                    float currentSize = Mathf.Lerp(startOrthographicSize, parameters.targetOrthographicSize, easedTime);
                    targetCamera.orthographicSize = currentSize;
                }
                
                OnTransitionProgress?.Invoke(normalizedTime);
                yield return null;
            }
            
            // 确保达到精确的目标值
            if (parameters.transitionPosition)
            {
                targetCamera.transform.position = parameters.targetPosition;
            }
            if (parameters.transitionRotation)
            {
                targetCamera.transform.eulerAngles = parameters.targetRotation;
            }
            if (parameters.transitionFOV && !targetCamera.orthographic)
            {
                targetCamera.fieldOfView = parameters.targetFOV;
            }
            if (parameters.transitionOrthographicSize && targetCamera.orthographic)
            {
                targetCamera.orthographicSize = parameters.targetOrthographicSize;
            }
            
            // 淡出效果
            if (parameters.enableFadeEffect)
            {
                yield return StartCoroutine(FadeOut(parameters.fadeOutDuration));
            }
            
            isTransitioning = false;
            transitionCoroutine = null;
            
            OnTransitionEnd?.Invoke();
            tcs.TrySetResult();
        }
        
        /// <summary>
        /// 创建淡入淡出Canvas
        /// </summary>
        private void CreateFadeCanvas()
        {
            if (fadeCanvas != null) return;
            
            fadeCanvas = new GameObject("CameraTransitionFade");
            fadeCanvas.SetActive(false);
            
            var canvas = fadeCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            
            fadeCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            fadeCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            var imageGO = new GameObject("FadeImage");
            imageGO.transform.SetParent(fadeCanvas.transform, false);
            
            fadeImage = imageGO.AddComponent<UnityEngine.UI.Image>();
            fadeImage.color = new Color(0, 0, 0, 0);
            
            // 设置为全屏
            var rectTransform = imageGO.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            DontDestroyOnLoad(fadeCanvas);
        }
        
        /// <summary>
        /// 销毁淡入淡出Canvas
        /// </summary>
        private void DestroyFadeCanvas()
        {
            if (fadeCanvas != null)
            {
                DestroyImmediate(fadeCanvas);
                fadeCanvas = null;
                fadeImage = null;
            }
        }
        
        /// <summary>
        /// 淡入效果
        /// </summary>
        private IEnumerator FadeIn(Color fadeColor, float duration)
        {
            if (fadeCanvas == null || fadeImage == null) yield break;
            
            fadeCanvas.SetActive(true);
            fadeColor.a = 0f;
            fadeImage.color = fadeColor;
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = elapsed / duration;
                fadeColor.a = alpha;
                fadeImage.color = fadeColor;
                yield return null;
            }
            
            fadeColor.a = 1f;
            fadeImage.color = fadeColor;
        }
        
        /// <summary>
        /// 淡出效果
        /// </summary>
        private IEnumerator FadeOut(float duration)
        {
            if (fadeCanvas == null || fadeImage == null) yield break;
            
            var fadeColor = fadeImage.color;
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / duration);
                fadeColor.a = alpha;
                fadeImage.color = fadeColor;
                yield return null;
            }
            
            fadeColor.a = 0f;
            fadeImage.color = fadeColor;
            fadeCanvas.SetActive(false);
        }
        
        /// <summary>
        /// 隐藏淡入淡出效果
        /// </summary>
        private void HideFadeEffect()
        {
            if (fadeCanvas != null)
            {
                fadeCanvas.SetActive(false);
            }
        }
        
        #endregion
        
        #region 静态工具方法
        
        /// <summary>
        /// 为GameObject添加摄像机过渡组件
        /// </summary>
        /// <param name="gameObject">目标GameObject</param>
        /// <returns>CameraTransition组件</returns>
        public static CameraTransition AddTo(GameObject gameObject)
        {
            if (gameObject == null) return null;
            
            var transition = gameObject.GetComponent<CameraTransition>();
            if (transition == null)
            {
                transition = gameObject.AddComponent<CameraTransition>();
            }
            
            return transition;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 过渡类型
    /// </summary>
    public enum TransitionType
    {
        Smooth,     // 平滑过渡
        Instant,    // 瞬间切换
        Fade,       // 淡入淡出
        Slide       // 滑动过渡
    }
}