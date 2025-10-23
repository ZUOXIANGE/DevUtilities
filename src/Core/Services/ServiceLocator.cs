using System;
using DevUtilities.Core.Services.Interfaces;
using Serilog;

namespace DevUtilities.Core.Services;

/// <summary>
/// 服务定位器，提供简化的服务访问方式
/// </summary>
public static class ServiceLocator
{
    private static ServiceContainer? _container;

    /// <summary>
    /// 初始化服务定位器
    /// </summary>
    /// <param name="container">服务容器</param>
    public static void Initialize(ServiceContainer container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
        Log.Information("[ServiceLocator] 服务定位器初始化完成");
    }

    /// <summary>
    /// 获取服务实例
    /// </summary>
    /// <typeparam name="T">服务类型</typeparam>
    /// <returns>服务实例</returns>
    /// <exception cref="InvalidOperationException">服务定位器未初始化或服务未注册</exception>
    public static T GetService<T>() where T : class
    {
        if (_container == null)
        {
            throw new InvalidOperationException("ServiceLocator has not been initialized. Call Initialize() first.");
        }

        return _container.GetService<T>();
    }

    /// <summary>
    /// 尝试获取服务实例
    /// </summary>
    /// <typeparam name="T">服务类型</typeparam>
    /// <returns>服务实例，如果未注册则返回null</returns>
    public static T? TryGetService<T>() where T : class
    {
        if (_container == null)
        {
            Log.Warning("[ServiceLocator] 服务定位器未初始化");
            return null;
        }

        return _container.TryGetService<T>();
    }

    /// <summary>
    /// 检查服务是否已注册
    /// </summary>
    /// <typeparam name="T">服务类型</typeparam>
    /// <returns>是否已注册</returns>
    public static bool IsRegistered<T>()
    {
        if (_container == null)
        {
            return false;
        }

        return _container.IsRegistered<T>();
    }

    /// <summary>
    /// 获取文件服务
    /// </summary>
    /// <returns>文件服务实例</returns>
    public static IFileService FileService => GetService<IFileService>();

    /// <summary>
    /// 获取剪贴板服务
    /// </summary>
    /// <returns>剪贴板服务实例</returns>
    public static IClipboardService ClipboardService => GetService<IClipboardService>();

    /// <summary>
    /// 获取配置服务
    /// </summary>
    /// <returns>配置服务实例</returns>
    public static IConfigurationService ConfigurationService => GetService<IConfigurationService>();

    /// <summary>
    /// 获取日志服务
    /// </summary>
    /// <returns>日志服务实例</returns>
    public static ILoggingService LoggingService => GetService<ILoggingService>();

    /// <summary>
    /// 重置服务定位器
    /// </summary>
    internal static void Reset()
    {
        _container = null;
        Log.Debug("[ServiceLocator] 服务定位器已重置");
    }
}