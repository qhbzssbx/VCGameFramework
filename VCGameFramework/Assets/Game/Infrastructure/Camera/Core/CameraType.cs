namespace Game.Infrastructure.Camera.Core
{
    /// <summary>
    /// 摄像机类型枚举
    /// </summary>
    public enum CameraType
    {
        /// <summary>
        /// 主摄像机 - 用于渲染游戏世界
        /// </summary>
        Main = 0,
        
        /// <summary>
        /// UI摄像机 - 专门用于渲染UI界面
        /// </summary>
        UI = 100,
        
        /// <summary>
        /// 特效摄像机 - 用于渲染特殊效果
        /// </summary>
        Effect = 200,
        
        /// <summary>
        /// 小地图摄像机 - 用于渲染小地图
        /// </summary>
        Minimap = 300,
        
        /// <summary>
        /// 过场动画摄像机 - 用于播放过场动画
        /// </summary>
        Cinematic = 400
    }
}