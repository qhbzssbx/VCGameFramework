namespace GameScript.Core.UI.Core
{
    /// <summary>
    /// UI层级枚举
    /// 定义UI面板在Canvas中的显示层级
    /// </summary>
    public enum UILayer
    {
        /// <summary>
        /// 背景层 - 最底层，通常用于游戏背景UI
        /// </summary>
        Background = 0,
        
        /// <summary>
        /// 窗口层 - 普通UI窗口，如设置面板、背包等
        /// </summary>
        Window = 1000,
        
        /// <summary>
        /// 弹窗层 - 重要提示、确认对话框等
        /// </summary>
        Popup = 2000,
        
        /// <summary>
        /// 顶层 - 最高优先级，如loading、网络提示等
        /// </summary>
        Top = 3000
    }
}