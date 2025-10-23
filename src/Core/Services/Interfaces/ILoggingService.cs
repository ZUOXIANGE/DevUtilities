using Serilog.Events;

namespace DevUtilities.Core.Services.Interfaces
{
    /// <summary>
    /// 日志服务接口
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// 配置Serilog日志系统
        /// </summary>
        void ConfigureLogging();

        /// <summary>
        /// 更新日志级别
        /// </summary>
        /// <param name="logLevel">新的日志级别</param>
        void UpdateLogLevel(string logLevel);

        /// <summary>
        /// 获取当前日志级别
        /// </summary>
        /// <returns>日志级别</returns>
        LogEventLevel GetLogLevel();

        /// <summary>
        /// 获取可用的日志级别列表
        /// </summary>
        /// <returns>日志级别字符串数组</returns>
        string[] GetAvailableLogLevels();
    }
}