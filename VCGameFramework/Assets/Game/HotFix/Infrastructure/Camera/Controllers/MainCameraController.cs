using UnityEngine;
using Game.Infrastructure.Camera.Core;
using CameraType = Game.Infrastructure.Camera.Core.CameraType;

namespace Game.Infrastructure.Camera.Controllers
{
    /// <summary>
    /// 主摄像机控制器 - 负责游戏世界的渲染
    /// </summary>
    public class MainCameraController : BaseCameraController
    {
        [Header("主摄像机设置")]
        [SerializeField] private Transform followTarget;
        [SerializeField] private Vector3 offset = new Vector3(0, 5, -10);
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private float rotationSpeed = 2f;
        [SerializeField] private bool enableMouseLook = false;
        [SerializeField] private bool enableKeyboardMovement = false;
        
        [Header("限制设置")]
        [SerializeField] private Vector2 pitchLimits = new Vector2(-60f, 60f);
        [SerializeField] private Vector3 movementBounds = Vector3.zero;
        [SerializeField] private bool useBounds = false;
        
        private Vector3 currentVelocity;
        private float currentPitch = 0f;
        private float currentYaw = 0f;
        private Vector3 lastMousePosition;
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            cameraType = CameraType.Main;
            
            // 初始化旋转角度
            var eulerAngles = transform.eulerAngles;
            currentPitch = eulerAngles.x;
            currentYaw = eulerAngles.y;
            
            // 如果没有跟随目标，尝试自动查找
            if (followTarget == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    followTarget = player.transform;
                    Debug.Log($"[MainCameraController] 自动找到跟随目标: {player.name}");
                }
            }
        }
        
        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();
            
            HandleInput();
            HandleFollowTarget();
            HandleKeyboardMovement();
            ApplyBounds();
        }
        
        /// <summary>
        /// 设置跟随目标
        /// </summary>
        /// <param name="target">跟随目标</param>
        /// <param name="newOffset">相对偏移</param>
        public void SetFollowTarget(Transform target, Vector3? newOffset = null)
        {
            followTarget = target;
            
            if (newOffset.HasValue)
            {
                offset = newOffset.Value;
            }
            
            Debug.Log($"[MainCameraController] 设置跟随目标: {(target ? target.name : "null")}");
        }
        
        /// <summary>
        /// 设置摄像机位置和旋转
        /// </summary>
        /// <param name="position">目标位置</param>
        /// <param name="rotation">目标旋转</param>
        /// <param name="smooth">是否平滑过渡</param>
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation, bool smooth = true)
        {
            if (smooth)
            {
                StartCoroutine(SmoothMoveToPosition(position, rotation));
            }
            else
            {
                transform.position = position;
                transform.rotation = rotation;
                
                var eulerAngles = rotation.eulerAngles;
                currentPitch = eulerAngles.x;
                currentYaw = eulerAngles.y;
            }
        }
        
        /// <summary>
        /// 启用/禁用鼠标控制
        /// </summary>
        /// <param name="enabled">是否启用</param>
        public void SetMouseLookEnabled(bool enabled)
        {
            enableMouseLook = enabled;
            
            if (enabled)
            {
                lastMousePosition = Input.mousePosition;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }
        
        /// <summary>
        /// 启用/禁用键盘移动
        /// </summary>
        /// <param name="enabled">是否启用</param>
        public void SetKeyboardMovementEnabled(bool enabled)
        {
            enableKeyboardMovement = enabled;
        }
        
        /// <summary>
        /// 处理输入
        /// </summary>
        private void HandleInput()
        {
            if (!enableMouseLook) return;
            
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
            
            currentYaw += mouseX;
            currentPitch -= mouseY;
            
            // 限制俯仰角度
            currentPitch = Mathf.Clamp(currentPitch, pitchLimits.x, pitchLimits.y);
            
            transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        }
        
        /// <summary>
        /// 处理跟随目标
        /// </summary>
        private void HandleFollowTarget()
        {
            if (followTarget == null) return;
            
            Vector3 targetPosition;
            
            if (enableMouseLook)
            {
                // 如果启用鼠标控制，保持相对偏移
                targetPosition = followTarget.position + transform.rotation * offset;
            }
            else
            {
                // 否则使用固定偏移
                targetPosition = followTarget.position + offset;
            }
            
            // 平滑移动到目标位置
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                targetPosition, 
                ref currentVelocity, 
                1f / followSpeed
            );
        }
        
        /// <summary>
        /// 处理键盘移动
        /// </summary>
        private void HandleKeyboardMovement()
        {
            if (!enableKeyboardMovement) return;
            
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
            {
                Vector3 movement = transform.right * horizontal + transform.forward * vertical;
                movement = movement.normalized * followSpeed * Time.deltaTime;
                
                transform.position += movement;
            }
        }
        
        /// <summary>
        /// 应用移动边界限制
        /// </summary>
        private void ApplyBounds()
        {
            if (!useBounds) return;
            
            Vector3 pos = transform.position;
            
            if (movementBounds.x > 0)
            {
                pos.x = Mathf.Clamp(pos.x, -movementBounds.x, movementBounds.x);
            }
            
            if (movementBounds.y > 0)
            {
                pos.y = Mathf.Clamp(pos.y, -movementBounds.y, movementBounds.y);
            }
            
            if (movementBounds.z > 0)
            {
                pos.z = Mathf.Clamp(pos.z, -movementBounds.z, movementBounds.z);
            }
            
            transform.position = pos;
        }
        
        /// <summary>
        /// 平滑移动到指定位置
        /// </summary>
        private System.Collections.IEnumerator SmoothMoveToPosition(Vector3 targetPos, Quaternion targetRot)
        {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            
            float elapsed = 0f;
            float duration = 1f / followSpeed;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
                
                yield return null;
            }
            
            transform.position = targetPos;
            transform.rotation = targetRot;
            
            var eulerAngles = targetRot.eulerAngles;
            currentPitch = eulerAngles.x;
            currentYaw = eulerAngles.y;
        }
        
        protected override void OnActivate()
        {
            base.OnActivate();
            Debug.Log("[MainCameraController] 主摄像机激活");
        }
        
        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            Debug.Log("[MainCameraController] 主摄像机停用");
        }
    }
}