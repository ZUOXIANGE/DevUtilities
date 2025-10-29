using Serilog;
using Serilog.Events;
using DevUtilities.Core.Services.Interfaces;

namespace DevUtilities.Core.Services;

/// <summary>
/// 日志服务，用于管理Serilog配置
/// </summary>
public class LoggingService : ILoggingService
{
    private readonly UserSettingsService _settingsService;

    public LoggingService(UserSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <summary>
    /// 配置Serilog日志系统
    /// </summary>
    public void ConfigureLogging()
    {
        var logLevel = GetLogLevel();
        var enableFileLogging = _settingsService.GetSetting(SettingsKeys.Logging.EnableFileLogging, true);
        var enableConsoleLogging = _settingsService.GetSetting(SettingsKeys.Logging.EnableConsoleLogging, true);

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(logLevel);

        if (enableConsoleLogging)
        {
            loggerConfig.WriteTo.Console();
        }

        if (enableFileLogging)
        {
            loggerConfig.WriteTo.File("logs/devutilities-.log", rollingInterval: RollingInterval.Day);
        }

        Log.Logger = loggerConfig.CreateLogger();
            
        Log.Information("[LoggingService] 日志系统已配置，级别: {LogLevel}, 文件日志: {FileLogging}, 控制台日志: {ConsoleLogging}", 
            logLevel, enableFileLogging, enableConsoleLogging);
    }

    /// <summary>
    /// 更新日志级别
    /// </summary>
    /// <param name="logLevel">新的日志级别</param>
    public void UpdateLogLevel(string logLevel)
    {
        _settingsService.SetSetting(SettingsKeys.Logging.LogLevel, logLevel);
        _ = _settingsService.SaveSettingsAsync();
            
        // 重新配置日志系统
        ConfigureLogging();
    }

    /// <summary>
    /// 获取当前日志级别
    /// </summary>
    /// <returns>日志级别</returns>
    public LogEventLevel GetLogLevel()
    {
        var logLevelString = _settingsService.GetSetting(SettingsKeys.Logging.LogLevel, "Information");
            
        return logLevelString.ToLowerInvariant() switch
        {
            "verbose" => LogEventLevel.Verbose,
            "debug" => LogEventLevel.Debug,
            "information" => LogEventLevel.Information,
            "warning" => LogEventLevel.Warning,
            "error" => LogEventLevel.Error,
            "fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }

    /// <summary>
    /// 获取可用的日志级别列表
    /// </summary>
    /// <returns>日志级别字符串数组</returns>
    public string[] GetAvailableLogLevels()
    {
        return new[] { "Verbose", "Debug", "Information", "Warning", "Error", "Fatal" };
    }
}
