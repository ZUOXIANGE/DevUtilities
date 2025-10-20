using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DevUtilities.Core.Services.Interfaces;

namespace DevUtilities.Core.Services;

/// <summary>
/// 简单的依赖注入容器
/// </summary>
public class ServiceContainer
{
    private static ServiceContainer? _instance;
    private static readonly object _lock = new();

    /// <summary>
    /// 单例实例
    /// </summary>
    public static ServiceContainer Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new ServiceContainer();
                }
            }
            return _instance;
        }
    }

    private readonly ConcurrentDictionary<Type, object> _singletonServices = new();
    private readonly ConcurrentDictionary<Type, Func<object>> _transientFactories = new();
    private readonly ConcurrentDictionary<Type, Type> _serviceTypes = new();

    /// <summary>
    /// 私有构造函数
    /// </summary>
    private ServiceContainer()
    {
    }

    /// <summary>
    /// 注册单例服务
    /// </summary>
    /// <typeparam name="TInterface">服务接口类型</typeparam>
    /// <typeparam name="TImplementation">服务实现类型</typeparam>
    /// <returns>服务容器实例</returns>
    public ServiceContainer RegisterSingleton<TInterface, TImplementation>()
        where TImplementation : class, TInterface, new()
    {
        var interfaceType = typeof(TInterface);
        var implementationType = typeof(TImplementation);

        _serviceTypes[interfaceType] = implementationType;
        
        return this;
    }

    /// <summary>
    /// 注册单例服务实例
    /// </summary>
    /// <typeparam name="TInterface">服务接口类型</typeparam>
    /// <param name="instance">服务实例</param>
    /// <returns>服务容器实例</returns>
    public ServiceContainer RegisterSingleton<TInterface>(TInterface instance)
        where TInterface : class
    {
        var interfaceType = typeof(TInterface);
        _singletonServices[interfaceType] = instance;
        
        return this;
    }

    /// <summary>
    /// 注册瞬态服务
    /// </summary>
    /// <typeparam name="TInterface">服务接口类型</typeparam>
    /// <typeparam name="TImplementation">服务实现类型</typeparam>
    /// <returns>服务容器实例</returns>
    public ServiceContainer RegisterTransient<TInterface, TImplementation>()
        where TImplementation : class, TInterface, new()
    {
        var interfaceType = typeof(TInterface);
        _transientFactories[interfaceType] = () => new TImplementation();
        
        return this;
    }

    /// <summary>
    /// 注册瞬态服务工厂
    /// </summary>
    /// <typeparam name="TInterface">服务接口类型</typeparam>
    /// <param name="factory">服务工厂方法</param>
    /// <returns>服务容器实例</returns>
    public ServiceContainer RegisterTransient<TInterface>(Func<TInterface> factory)
        where TInterface : class
    {
        var interfaceType = typeof(TInterface);
        _transientFactories[interfaceType] = () => factory();
        
        return this;
    }

    /// <summary>
    /// 获取服务实例
    /// </summary>
    /// <typeparam name="T">服务类型</typeparam>
    /// <returns>服务实例</returns>
    public T GetService<T>() where T : class
    {
        var serviceType = typeof(T);
        
        // 首先检查是否有直接注册的单例实例
        if (_singletonServices.TryGetValue(serviceType, out var singletonInstance))
        {
            return (T)singletonInstance;
        }

        // 检查是否有瞬态工厂
        if (_transientFactories.TryGetValue(serviceType, out var factory))
        {
            return (T)factory();
        }

        // 检查是否有注册的单例类型
        if (_serviceTypes.TryGetValue(serviceType, out var implementationType))
        {
            var instance = _singletonServices.GetOrAdd(serviceType, _ => CreateInstance(implementationType));
            return (T)instance;
        }

        throw new InvalidOperationException($"Service of type {serviceType.Name} is not registered.");
    }

    /// <summary>
    /// 尝试获取服务实例
    /// </summary>
    /// <typeparam name="T">服务类型</typeparam>
    /// <returns>服务实例，如果未注册则返回null</returns>
    public T? TryGetService<T>() where T : class
    {
        try
        {
            return GetService<T>();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    /// <summary>
    /// 检查服务是否已注册
    /// </summary>
    /// <typeparam name="T">服务类型</typeparam>
    /// <returns>是否已注册</returns>
    public bool IsRegistered<T>()
    {
        var serviceType = typeof(T);
        return _singletonServices.ContainsKey(serviceType) ||
               _transientFactories.ContainsKey(serviceType) ||
               _serviceTypes.ContainsKey(serviceType);
    }

    /// <summary>
    /// 获取所有已注册的服务类型
    /// </summary>
    /// <returns>服务类型列表</returns>
    public IEnumerable<Type> GetRegisteredServiceTypes()
    {
        var types = new HashSet<Type>();
        
        foreach (var key in _singletonServices.Keys)
            types.Add(key);
            
        foreach (var key in _transientFactories.Keys)
            types.Add(key);
            
        foreach (var key in _serviceTypes.Keys)
            types.Add(key);
            
        return types;
    }

    /// <summary>
    /// 清除所有注册的服务
    /// </summary>
    public void Clear()
    {
        _singletonServices.Clear();
        _transientFactories.Clear();
        _serviceTypes.Clear();
    }

    /// <summary>
    /// 创建类型实例
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns>实例</returns>
    private object CreateInstance(Type type)
    {
        try
        {
            return Activator.CreateInstance(type) ?? throw new InvalidOperationException($"Failed to create instance of {type.Name}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create instance of {type.Name}: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// 服务容器扩展方法
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// 配置默认服务
    /// </summary>
    /// <param name="container">服务容器</param>
    /// <returns>服务容器实例</returns>
    public static ServiceContainer ConfigureDefaultServices(this ServiceContainer container)
    {
        // 注册核心服务
        container.RegisterSingleton<IClipboardService, Implementations.ClipboardService>();
        container.RegisterSingleton<IFileService, Implementations.FileService>();
        container.RegisterSingleton<IConfigurationService, Implementations.ConfigurationService>();
        
        // 注册其他服务（当实现后）
        // container.RegisterSingleton<INotificationService, Implementations.NotificationService>();
        // container.RegisterSingleton<IHttpService, Implementations.HttpService>();

        return container;
    }
}