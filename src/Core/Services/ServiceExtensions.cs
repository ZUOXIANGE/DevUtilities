using System;
using DevUtilities.Core.Services.Interfaces;
using Serilog;

namespace DevUtilities.Core.Services;

/// <summary>
/// 服务注册扩展方法
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// 注册核心服务
    /// </summary>
    /// <param name="container">服务容器</param>
    /// <returns>服务容器实例</returns>
    public static ServiceContainer AddCoreServices(this ServiceContainer container)
    {
        Log.Debug("[ServiceExtensions] 开始注册核心服务");
        
        try
        {
            container.RegisterSingleton<IClipboardService, Implementations.ClipboardService>();
            container.RegisterSingleton<IFileService, Implementations.FileService>();
            container.RegisterSingleton<IConfigurationService, Implementations.ConfigurationService>();
            container.RegisterSingleton<UserSettingsService>();
            
            // 使用工厂方法注册LoggingService，确保依赖正确解析
            container.RegisterSingleton<ILoggingService>(() => 
            {
                var settingsService = container.GetService<UserSettingsService>();
                return new LoggingService(settingsService);
            });
            
            Log.Information("[ServiceExtensions] 核心服务注册完成");
            return container;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ServiceExtensions] 注册核心服务时发生错误");
            throw;
        }
    }

    /// <summary>
    /// 注册HTTP服务（当实现后）
    /// </summary>
    /// <param name="container">服务容器</param>
    /// <returns>服务容器实例</returns>
    public static ServiceContainer AddHttpService(this ServiceContainer container)
    {
        Log.Debug("[ServiceExtensions] 开始注册HTTP服务");
        
        try
        {
            // TODO: 当IHttpService实现后取消注释
            // container.RegisterSingleton<IHttpService, Implementations.HttpService>();
            
            Log.Debug("[ServiceExtensions] HTTP服务注册完成");
            return container;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ServiceExtensions] 注册HTTP服务时发生错误");
            throw;
        }
    }

    /// <summary>
    /// 注册通知服务（当实现后）
    /// </summary>
    /// <param name="container">服务容器</param>
    /// <returns>服务容器实例</returns>
    public static ServiceContainer AddNotificationService(this ServiceContainer container)
    {
        Log.Debug("[ServiceExtensions] 开始注册通知服务");
        
        try
        {
            // TODO: 当INotificationService实现后取消注释
            // container.RegisterSingleton<INotificationService, Implementations.NotificationService>();
            
            Log.Debug("[ServiceExtensions] 通知服务注册完成");
            return container;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ServiceExtensions] 注册通知服务时发生错误");
            throw;
        }
    }

    /// <summary>
    /// 注册所有可用服务
    /// </summary>
    /// <param name="container">服务容器</param>
    /// <returns>服务容器实例</returns>
    public static ServiceContainer AddAllServices(this ServiceContainer container)
    {
        Log.Information("[ServiceExtensions] 开始注册所有服务");
        
        try
        {
            container
                .AddCoreServices()
                .AddHttpService()
                .AddNotificationService();
            
            Log.Information("[ServiceExtensions] 所有服务注册完成");
            return container;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ServiceExtensions] 注册所有服务时发生错误");
            throw;
        }
    }
}