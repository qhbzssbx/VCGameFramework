using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Infrastructure.Camera.Core
{
    /// <summary>
    /// 摄像机管理器接口
    /// </summary>
    public interface ICameraManager : IDisposable
    {
        /// <summary>
        /// 当前活动的摄像机
        /// </summary>
        UnityEngine.Camera ActiveCamera { get; }
        
        /// <summary>
        /// 摄像机切换事件
        /// </summary>
        event Action<CameraType, CameraType> OnCameraSwitched;
        
        /// <summary>
        /// 注册摄像机
        /// </summary>
        /// <param name="camera">摄像机实例</param>
        /// <param name="config">摄像机配置</param>
        /// <returns>是否注册成功</returns>
        bool RegisterCamera(UnityEngine.Camera camera, CameraConfig config);
        
        /// <summary>
        /// 注销摄像机
        /// </summary>
        /// <param name="cameraType">摄像机类型</param>
        /// <returns>是否注销成功</returns>
        bool UnregisterCamera(CameraType cameraType);
        
        /// <summary>
        /// 获取指定类型的摄像机
        /// </summary>
        /// <param name="cameraType">摄像机类型</param>
        /// <returns>摄像机实例，不存在则返回null</returns>
        UnityEngine.Camera GetCamera(CameraType cameraType);
        
        /// <summary>
        /// 获取所有注册的摄像机
        /// </summary>
        /// <returns>摄像机字典</returns>
        Dictionary<CameraType, UnityEngine.Camera> GetAllCameras();
        
        /// <summary>
        /// 切换到指定类型的摄像机
        /// </summary>
        /// <param name="cameraType">目标摄像机类型</param>
        /// <param name="immediate">是否立即切换，false则使用平滑过渡</param>
        /// <returns>切换任务</returns>
        UniTask SwitchCamera(CameraType cameraType, bool immediate = false);
        
        /// <summary>
        /// 激活摄像机
        /// </summary>
        /// <param name="cameraType">摄像机类型</param>
        void ActivateCamera(CameraType cameraType);
        
        /// <summary>
        /// 停用摄像机
        /// </summary>
        /// <param name="cameraType">摄像机类型</param>
        void DeactivateCamera(CameraType cameraType);
        
        /// <summary>
        /// 设置摄像机配置
        /// </summary>
        /// <param name="cameraType">摄像机类型</param>
        /// <param name="config">新的配置</param>
        /// <returns>是否设置成功</returns>
        bool SetCameraConfig(CameraType cameraType, CameraConfig config);
        
        /// <summary>
        /// 获取摄像机配置
        /// </summary>
        /// <param name="cameraType">摄像机类型</param>
        /// <returns>配置信息，不存在则返回null</returns>
        CameraConfig GetCameraConfig(CameraType cameraType);
        
        /// <summary>
        /// 应用震动效果
        /// </summary>
        /// <param name="duration">震动持续时间</param>
        /// <param name="magnitude">震动强度</param>
        /// <param name="roughness">震动频率</param>
        /// <param name="fadeIn">渐入时间</param>
        /// <param name="fadeOut">渐出时间</param>
        void ShakeCamera(float duration, float magnitude = 1f, float roughness = 1f, float fadeIn = 0f, float fadeOut = 0f);
        
        /// <summary>
        /// 停止摄像机震动
        /// </summary>
        void StopCameraShake();
        
        /// <summary>
        /// 设置摄像机跟随目标
        /// </summary>
        /// <param name="cameraType">摄像机类型</param>
        /// <param name="target">跟随目标</param>
        /// <param name="smoothTime">平滑时间</param>
        void SetFollowTarget(CameraType cameraType, Transform target, float smoothTime = 0.3f);
        
        /// <summary>
        /// 移除摄像机跟随目标
        /// </summary>
        /// <param name="cameraType">摄像机类型</param>
        void RemoveFollowTarget(CameraType cameraType);
        
        /// <summary>
        /// 创建新的摄像机
        /// </summary>
        /// <param name="config">摄像机配置</param>
        /// <param name="parent">父对象</param>
        /// <returns>创建的摄像机实例</returns>
        UnityEngine.Camera CreateCamera(CameraConfig config, Transform parent = null);
        
        /// <summary>
        /// 销毁摄像机
        /// </summary>
        /// <param name="cameraType">摄像机类型</param>
        /// <returns>是否销毁成功</returns>
        bool DestroyCamera(CameraType cameraType);
        
        /// <summary>
        /// 检查摄像机是否存在
        /// </summary>
        /// <param name="cameraType">摄像机类型</param>
        /// <returns>是否存在</returns>
        bool HasCamera(CameraType cameraType);
        
        /// <summary>
        /// 检查摄像机是否激活
        /// </summary>
        /// <param name="cameraType">摄像机类型</param>
        /// <returns>是否激活</returns>
        bool IsCameraActive(CameraType cameraType);
    }
}