using System.ComponentModel;
using System.Runtime.CompilerServices;
using DevUtilities.Core.Services;
using DevUtilities.Core.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DevUtilities.Core.ViewModels;

/// <summary>
/// 基础ViewModel类，提供属性变更通知和服务访问
/// </summary>
public abstract class BaseViewModel : ObservableObject
{
    /// <summary>
    /// 文件服务
    /// </summary>
    protected IFileService FileService => ServiceLocator.FileService;

    /// <summary>
    /// 剪贴板服务
    /// </summary>
    protected IClipboardService ClipboardService => ServiceLocator.ClipboardService;

    /// <summary>
    /// 配置服务
    /// </summary>
    protected IConfigurationService ConfigurationService => ServiceLocator.ConfigurationService;

    /// <summary>
    /// 日志服务
    /// </summary>
    protected ILoggingService LoggingService => ServiceLocator.LoggingService;

    /// <summary>
    /// 获取服务实例
    /// </summary>
    /// <typeparam name="T">服务类型</typeparam>
    /// <returns>服务实例</returns>
    protected T GetService<T>() where T : class
    {
        return ServiceLocator.GetService<T>();
    }

    /// <summary>
    /// 尝试获取服务实例
    /// </summary>
    /// <typeparam name="T">服务类型</typeparam>
    /// <returns>服务实例，如果未注册则返回null</returns>
    protected T? TryGetService<T>() where T : class
    {
        return ServiceLocator.TryGetService<T>();
    }
}