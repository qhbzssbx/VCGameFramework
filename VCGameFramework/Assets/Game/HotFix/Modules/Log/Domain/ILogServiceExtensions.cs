namespace Game.Modules.Log.Domain
{
    /// <summary>
    /// ILogService的扩展方法，提供便利的日志记录接口
    /// </summary>
    public static class ILogServiceExtensions
    {
        /// <summary>
        /// 记录信息级别日志
        /// </summary>
        /// <param name="logService">日志服务实例</param>
        /// <param name="message">日志消息</param>
        public static void Info(this ILogService logService, string message)
        {
            logService.Log(LogLevel.Info, message);
        }
        
        /// <summary>
        /// 记录警告级别日志
        /// </summary>
        /// <param name="logService">日志服务实例</param>
        /// <param name="message">日志消息</param>
        public static void Warning(this ILogService logService, string message)
        {
            logService.Log(LogLevel.Warning, message);
        }
        
        /// <summary>
        /// 记录错误级别日志
        /// </summary>
        /// <param name="logService">日志服务实例</param>
        /// <param name="message">日志消息</param>
        public static void Error(this ILogService logService, string message)
        {
            logService.Log(LogLevel.Error, message);
        }
    }
}