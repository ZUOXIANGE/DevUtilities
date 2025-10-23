using System;
using Avalonia;
using Serilog;
using DevUtilities.Core.Services;
using DevUtilities.Core.Services.Interfaces;

namespace DevUtilities;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // 初始化服务容器
        var container = ServiceContainer.Instance;
        container.ConfigureDefaultServices();
        ServiceLocator.Initialize(container);

        // 获取日志服务并配置日志系统
        var loggingService = ServiceLocator.GetService<ILoggingService>();
        loggingService.ConfigureLogging();

        Log.Debug("[Program] 开始配置Serilog日志系统");
        Log.Information("[Program] DevUtilities应用程序启动，参数: {Args}", args);

        try
        {
            Log.Debug("[Program] 开始构建Avalonia应用程序");
            var app = BuildAvaloniaApp();
            Log.Debug("[Program] Avalonia应用程序构建完成");
            
            Log.Information("[Program] 启动Avalonia应用程序主循环");
            app.StartWithClassicDesktopLifetime(args);
            
            Log.Information("[Program] Avalonia应用程序正常退出");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "[Program] 应用程序启动失败");
            throw;
        }
        finally
        {
            Log.Information("[Program] 应用程序关闭，清理资源");
            Log.CloseAndFlush();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        Log.Debug("[Program] 开始配置Avalonia应用程序构建器");
        
        try
        {
            var builder = AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
                
            Log.Debug("[Program] Avalonia应用程序构建器配置完成");
            return builder;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[Program] 配置Avalonia应用程序构建器时发生错误");
            throw;
        }
    }
}