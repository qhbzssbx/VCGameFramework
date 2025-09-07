using System.Collections;
using UnityEngine;

namespace Game.Infrastructure.Camera.Effects
{
    /// <summary>
    /// 摄像机震动效果
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        [System.Serializable]
        public class ShakeParameters
        {
            [Header("震动参数")]
            public float duration = 1f;
            public float magnitude = 1f;
            public float roughness = 1f;
            public float fadeIn = 0f;
            public float fadeOut = 0f;
            
            [Header("轴向设置")]
            public bool shakeX = true;
            public bool shakeY = true;
            public bool shakeZ = false;
            
            [Header("高级设置")]
            public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
            public bool randomSeed = true;
            public int seed = 0;
        }
        
        private Transform targetTransform;
        private Vector3 originalPosition;
        private Coroutine shakeCoroutine;
        private bool isShaking = false;
        
        /// <summary>
        /// 当前是否在震动
        /// </summary>
        public bool IsShaking => isShaking;
        
        /// <summary>
        /// 震动开始事件
        /// </summary>
        public System.Action OnShakeStart;
        
        /// <summary>
        /// 震动结束事件
        /// </summary>
        public System.Action OnShakeEnd;
        
        #region Unity生命周期
        
        private void Awake()
        {
            targetTransform = transform;
            originalPosition = targetTransform.localPosition;
        }
        
        private void OnDisable()
        {
            StopShake();
        }
        
        #endregion
        
        #region 公共接口
        
        /// <summary>
        /// 开始震动
        /// </summary>
        /// <param name="parameters">震动参数</param>
        public void StartShake(ShakeParameters parameters)
        {
            if (parameters == null) return;
            
            StopShake(); // 停止之前的震动
            shakeCoroutine = StartCoroutine(ShakeCoroutine(parameters));
        }
        
        /// <summary>
        /// 开始震动（快速方法）
        /// </summary>
        /// <param name="duration">持续时间</param>
        /// <param name="magnitude">震动强度</param>
        /// <param name="roughness">震动频率</param>
        /// <param name="fadeIn">渐入时间</param>
        /// <param name="fadeOut">渐出时间</param>
        public void StartShake(float duration, float magnitude = 1f, float roughness = 1f, float fadeIn = 0f, float fadeOut = 0f)
        {
            var parameters = new ShakeParameters
            {
                duration = duration,
                magnitude = magnitude,
                roughness = roughness,
                fadeIn = fadeIn,
                fadeOut = fadeOut
            };
            
            StartShake(parameters);
        }
        
        /// <summary>
        /// 停止震动
        /// </summary>
        public void StopShake()
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
            }
            
            if (isShaking)
            {
                isShaking = false;
                ResetPosition();
                OnShakeEnd?.Invoke();
            }
        }
        
        /// <summary>
        /// 设置目标Transform
        /// </summary>
        /// <param name="target">目标Transform</param>
        public void SetTarget(Transform target)
        {
            if (target == null) return;
            
            StopShake();
            targetTransform = target;
            originalPosition = targetTransform.localPosition;
        }
        
        /// <summary>
        /// 重置位置
        /// </summary>
        public void ResetPosition()
        {
            if (targetTransform != null)
            {
                targetTransform.localPosition = originalPosition;
            }
        }
        
        /// <summary>
        /// 更新原始位置
        /// </summary>
        public void UpdateOriginalPosition()
        {
            if (targetTransform != null && !isShaking)
            {
                originalPosition = targetTransform.localPosition;
            }
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 震动协程
        /// </summary>
        private IEnumerator ShakeCoroutine(ShakeParameters parameters)
        {
            if (targetTransform == null) yield break;
            
            isShaking = true;
            OnShakeStart?.Invoke();
            
            float elapsed = 0f;
            
            // 设置随机种子
            if (parameters.randomSeed)
            {
                Random.InitState(System.DateTime.Now.Millisecond);
            }
            else
            {
                Random.InitState(parameters.seed);
            }
            
            Vector3 lastOffset = Vector3.zero;
            
            while (elapsed < parameters.duration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / parameters.duration;
                
                // 计算当前强度
                float currentIntensity = CalculateIntensity(normalizedTime, parameters);
                
                // 生成震动偏移
                Vector3 shakeOffset = GenerateShakeOffset(parameters, currentIntensity);
                
                // 应用位置
                targetTransform.localPosition = originalPosition + shakeOffset;
                lastOffset = shakeOffset;
                
                yield return null;
            }
            
            // 震动结束，恢复原始位置
            isShaking = false;
            ResetPosition();
            shakeCoroutine = null;
            
            OnShakeEnd?.Invoke();
        }
        
        /// <summary>
        /// 计算当前震动强度
        /// </summary>
        private float CalculateIntensity(float normalizedTime, ShakeParameters parameters)
        {
            float intensity = 1f;
            
            // 使用强度曲线
            if (parameters.intensityCurve != null && parameters.intensityCurve.keys.Length > 0)
            {
                intensity = parameters.intensityCurve.Evaluate(normalizedTime);
            }
            
            // 应用渐入渐出
            if (normalizedTime < parameters.fadeIn / parameters.duration)
            {
                float fadeInProgress = normalizedTime / (parameters.fadeIn / parameters.duration);
                intensity *= fadeInProgress;
            }
            else if (normalizedTime > 1f - (parameters.fadeOut / parameters.duration))
            {
                float fadeOutProgress = (1f - normalizedTime) / (parameters.fadeOut / parameters.duration);
                intensity *= fadeOutProgress;
            }
            
            return intensity * parameters.magnitude;
        }
        
        /// <summary>
        /// 生成震动偏移
        /// </summary>
        private Vector3 GenerateShakeOffset(ShakeParameters parameters, float intensity)
        {
            Vector3 offset = Vector3.zero;
            
            if (parameters.shakeX)
            {
                offset.x = (Mathf.PerlinNoise(Time.time * parameters.roughness, 0f) - 0.5f) * 2f * intensity;
            }
            
            if (parameters.shakeY)
            {
                offset.y = (Mathf.PerlinNoise(0f, Time.time * parameters.roughness) - 0.5f) * 2f * intensity;
            }
            
            if (parameters.shakeZ)
            {
                offset.z = (Mathf.PerlinNoise(Time.time * parameters.roughness * 0.5f, Time.time * parameters.roughness * 0.5f) - 0.5f) * 2f * intensity;
            }
            
            return offset;
        }
        
        #endregion
        
        #region 静态工具方法
        
        /// <summary>
        /// 为GameObject添加摄像机震动组件
        /// </summary>
        /// <param name="gameObject">目标GameObject</param>
        /// <returns>CameraShake组件</returns>
        public static CameraShake AddTo(GameObject gameObject)
        {
            if (gameObject == null) return null;
            
            var shake = gameObject.GetComponent<CameraShake>();
            if (shake == null)
            {
                shake = gameObject.AddComponent<CameraShake>();
            }
            
            return shake;
        }
        
        /// <summary>
        /// 创建预定义的震动参数
        /// </summary>
        /// <param name="type">震动类型</param>
        /// <returns>震动参数</returns>
        public static ShakeParameters CreatePreset(ShakePreset type)
        {
            switch (type)
            {
                case ShakePreset.Light:
                    return new ShakeParameters
                    {
                        duration = 0.2f,
                        magnitude = 0.1f,
                        roughness = 10f,
                        fadeOut = 0.1f
                    };
                    
                case ShakePreset.Medium:
                    return new ShakeParameters
                    {
                        duration = 0.5f,
                        magnitude = 0.3f,
                        roughness = 15f,
                        fadeOut = 0.2f
                    };
                    
                case ShakePreset.Heavy:
                    return new ShakeParameters
                    {
                        duration = 1f,
                        magnitude = 0.8f,
                        roughness = 20f,
                        fadeIn = 0.1f,
                        fadeOut = 0.3f
                    };
                    
                case ShakePreset.Explosion:
                    return new ShakeParameters
                    {
                        duration = 1.5f,
                        magnitude = 1.5f,
                        roughness = 25f,
                        fadeIn = 0.1f,
                        fadeOut = 0.8f,
                        intensityCurve = AnimationCurve.EaseInOut(0, 1, 1, 0)
                    };
                    
                default:
                    return new ShakeParameters();
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 震动预设类型
    /// </summary>
    public enum ShakePreset
    {
        Light,      // 轻微震动
        Medium,     // 中等震动
        Heavy,      // 强烈震动
        Explosion   // 爆炸震动
    }
}