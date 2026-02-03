namespace MemoFramework
{
    public interface ILogger
    {
        /// <summary>
        /// 记录日志。
        /// </summary>
        /// <param name="level">游戏框架日志等级。</param>
        /// <param name="message">日志内容。</param>
        void Log(MFLogLevel level, object message);
    }
}