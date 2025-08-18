namespace Game.Modules.Log.Domain
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public interface ILogService
    {
        void Log(LogLevel level, string message);
    }
}