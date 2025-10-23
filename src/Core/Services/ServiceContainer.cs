using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DevUtilities.Core.Services.Interfaces;
using Serilog;

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
                    if (_instance == null)
                    {
                        Log.Debug("[ServiceContainer] 创建ServiceContainer单例实例");
                        _instance = new ServiceContainer();
                        Log.Information("[ServiceContainer] ServiceContainer单例实例创建完成");
                    }
                }
            }
            return _instance;
        }
    }

    private readonly ConcurrentDictionary<Type, object> _singletonServices = new();
    private readonly ConcurrentDictionary<Type, Func<object>> _transientFactories = new();
    private readonly ConcurrentDictionary<Type, Func<object>> _singletonFactories = new();
    private readonly ConcurrentDictionary<Type, Type> _serviceTypes = new();

    /// <summary>
    /// 私有构造函数
    /// </summary>
    private ServiceContainer()
    {
        Log.Debug("[ServiceContainer] ServiceContainer构造函数执行");
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

        Log.Debug("[ServiceContainer] 注册单例服务: {InterfaceType} -> {ImplementationType}", 
            interfaceType.Name, implementationType.Name);

        try
        {
            _serviceTypes[interfaceType] = implementationType;
            Log.Debug("[ServiceContainer] 单例服务注册成功: {InterfaceType} -> {ImplementationType}", 
                interfaceType.Name, implementationType.Name);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ServiceContainer] 注册单例服务失败: {InterfaceType} -> {ImplementationType}", 
                interfaceType.Name, implementationType.Name);
            throw;
        }
        
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
        
        Log.Debug("[ServiceContainer] 注册单例服务实例: {InterfaceType}", interfaceType.Name);
        
        try
        {
            _singletonServices[interfaceType] = instance;
            Log.Debug("[ServiceContainer] 单例服务实例注册成功: {InterfaceType}", interfaceType.Name);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ServiceContainer] 注册单例服务实例失败: {InterfaceType}", interfaceType.Name);
            throw;
        }
        
        return this;
    }

    /// <summary>
    /// 注册单例服务（仅实现类）
    /// </summary>
    /// <typeparam name="TImplementation">服务实现类型</typeparam>
    /// <returns>服务容器实例</returns>
    public ServiceContainer RegisterSingleton<TImplementation>()
        where TImplementation : class, new()
    {
        var implementationType = typeof(TImplementation);

        Log.Debug("[ServiceContainer] 注册单例服务: {ImplementationType}", implementationType.Name);

        try
        {
            _serviceTypes[implementationType] = implementationType;
            Log.Debug("[ServiceContainer] 单例服务注册成功: {ImplementationType}", implementationType.Name);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ServiceContainer] 注册单例服务失败: {ImplementationType}", implementationType.Name);
            throw;
        }
        
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
        var implementationType = typeof(TImplementation);
        
        Log.Debug("[ServiceContainer] 注册瞬态服务: {InterfaceType} -> {ImplementationType}", 
            interfaceType.Name, implementationType.Name);
        
        try
        {
            _transientFactories[interfaceType] = () => new TImplementation();
            Log.Debug("[ServiceContainer] 瞬态服务注册成功: {InterfaceType} -> {ImplementationType}", 
                interfaceType.Name, implementationType.Name);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ServiceContainer] 注册瞬态服务失败: {InterfaceType} -> {ImplementationType}", 
                interfaceType.Name, implementationType.Name);
            throw;
        }
        
        return this;
    }

    /// <summary>
    /// 注册单例服务工厂
    /// </summary>
    /// <typeparam name="TInterface">服务接口类型</typeparam>
    /// <param name="factory">服务工厂方法</param>
    /// <returns>服务容器实例</returns>
    public ServiceContainer RegisterSingleton<TInterface>(Func<TInterface> factory)
        where TInterface : class
    {
        var interfaceType = typeof(TInterface);
        
        Log.Debug("[ServiceContainer] 注册单例服务工厂: {InterfaceType}", interfaceType.Name);
        
        try
        {
            // 延迟创建单例实例
            _serviceTypes[interfaceType] = null; // 标记为工厂创建
            _singletonFactories[interfaceType] = () => factory();
            Log.Debug("[ServiceContainer] 单例服务工厂注册成功: {InterfaceType}", interfaceType.Name);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ServiceContainer] 注册单例服务工厂失败: {InterfaceType}", interfaceType.Name);
            throw;
        }
        
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
        
        Log.Debug("[ServiceContainer] 注册瞬态服务工厂: {InterfaceType}", interfaceType.Name);
        
        try
        {
            _transientFactories[interfaceType] = () => factory();
            Log.Debug("[ServiceContainer] 瞬态服务工厂注册成功: {InterfaceType}", interfaceType.Name);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ServiceContainer] 注册瞬态服务工厂失败: {InterfaceType}", interfaceType.Name);
            throw;
        }
        
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
        
        Log.Debug("[ServiceContainer] 获取服务: {ServiceType}", serviceType.Name);
        
        try
        {
            // 首先检查是否有直接注册的单例实例
            if (_singletonServices.TryGetValue(serviceType, out var singletonInstance))
            {
                Log.Debug("[ServiceContainer] 返回已注册的单例实例: {ServiceType}", serviceType.Name);
                return (T)singletonInstance;
            }

            // 检查是否有单例工厂
            if (_singletonFactories.TryGetValue(serviceType, out var singletonFactory))
            {
                Log.Debug("[ServiceContainer] 使用单例工厂创建实例: {ServiceType}", serviceType.Name);
                var instance = _singletonServices.GetOrAdd(serviceType, _ => singletonFactory());
                Log.Debug("[ServiceContainer] 单例工厂实例创建成功: {ServiceType}", serviceType.Name);
                return (T)instance;
            }

            // 检查是否有瞬态工厂
            if (_transientFactories.TryGetValue(serviceType, out var factory))
            {
                Log.Debug("[ServiceContainer] 使用瞬态工厂创建实例: {ServiceType}", serviceType.Name);
                var transientInstance = (T)factory();
                Log.Debug("[ServiceContainer] 瞬态实例创建成功: {ServiceType}", serviceType.Name);
                return transientInstance;
            }

            // 检查是否有注册的单例类型
            if (_serviceTypes.TryGetValue(serviceType, out var implementationType) && implementationType != null)
            {
                Log.Debug("[ServiceContainer] 创建单例类型实例: {ServiceType} -> {ImplementationType}", 
                    serviceType.Name, implementationType.Name);
                var instance = _singletonServices.GetOrAdd(serviceType, _ => CreateInstance(implementationType));
                Log.Debug("[ServiceContainer] 单例类型实例创建成功: {ServiceType}", serviceType.Name);
                return (T)instance;
            }

            Log.Warning("[ServiceContainer] 服务未注册: {ServiceType}", serviceType.Name);
            throw new InvalidOperationException($"Service of type {serviceType.Name} is not registered.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ServiceContainer] 获取服务时发生错误: {ServiceType}", serviceType.Name);
            throw;
        }
    }

    /// <summary>
    /// 尝试获取服务实例
    /// </summary>
    /// <typeparam name="T">服务类型</typeparam>
    /// <returns>服务实例，如果未注册则返回null</returns>
    public T? TryGetService<T>() where T : class
    {
        Log.Debug("[ServiceContainer] 尝试获取服务: {ServiceType}", typeof(T).Name);
        
        try
        {
            var service = GetService<T>();
            Log.Debug("[ServiceContainer] 服务获取成功: {ServiceType}", typeof(T).Name);
            return service;
        }
        catch (InvalidOperationException)
        {
            Log.Debug("[ServiceContainer] 服务未注册，返回null: {ServiceType}", typeof(T).Name);
            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ServiceContainer] 尝试获取服务时发生错误: {ServiceType}", typeof(T).Name);
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
        var isRegistered = _singletonServices.ContainsKey(serviceType) ||
               _transientFactories.ContainsKey(serviceType) ||
               _serviceTypes.ContainsKey(serviceType);
               
        Log.Debug("[ServiceContainer] 检查服务注册状态: {ServiceType} = {IsRegistered}", 
            serviceType.Name, isRegistered);
            
        return isRegistered;
    }

    /// <summary>
    /// 获取所有已注册的服务类型
    /// </summary>
    /// <returns>服务类型列表</returns>
    public IEnumerable<Type> GetRegisteredServiceTypes()
    {
        Log.Debug("[ServiceContainer] 获取所有已注册的服务类型");
        
        var types = new HashSet<Type>();
        
        foreach (var key in _singletonServices.Keys)
            types.Add(key);
            
        foreach (var key in _transientFactories.Keys)
            types.Add(key);
            
        foreach (var key in _serviceTypes.Keys)
            types.Add(key);
            
        Log.Debug("[ServiceContainer] 已注册服务类型数量: {Count}", types.Count);
        
        return types;
    }

    /// <summary>
    /// 清除所有注册的服务
    /// </summary>
    public void Clear()
    {
        Log.Information("[ServiceContainer] 清除所有注册的服务");
        
        try
        {
            var singletonCount = _singletonServices.Count;
            var transientCount = _transientFactories.Count;
            var typeCount = _serviceTypes.Count;
            
            _singletonServices.Clear();
            _transientFactories.Clear();
            _serviceTypes.Clear();
            
            Log.Information("[ServiceContainer] 服务清除完成 - 单例: {SingletonCount}, 瞬态: {TransientCount}, 类型: {TypeCount}", 
                singletonCount, transientCount, typeCount);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ServiceContainer] 清除服务时发生错误");
            throw;
        }
    }

    /// <summary>
    /// 创建类型实例
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns>实例</returns>
    private object CreateInstance(Type type)
    {
        Log.Debug("[ServiceContainer] 创建类型实例: {Type}", type.Name);
        
        try
        {
            var instance = Activator.CreateInstance(type) ?? throw new InvalidOperationException($"Failed to create instance of {type.Name}");
            Log.Debug("[ServiceContainer] 类型实例创建成功: {Type}", type.Name);
            return instance;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ServiceContainer] 创建类型实例失败: {Type}", type.Name);
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
        Log.Information("[ServiceContainer] 开始配置默认服务");
        
        try
        {
            // 使用新的扩展方法注册服务
            container.AddCoreServices();
            
            Log.Information("[ServiceContainer] 默认服务配置完成");
            return container;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ServiceContainer] 配置默认服务时发生错误");
            throw;
        }
    }
}