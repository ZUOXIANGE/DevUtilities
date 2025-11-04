# 依赖注入改进文档

## 概述

本文档记录了DevUtilities项目中依赖注入系统的改进内容，包括新增的组件、使用方法和最佳实践。

## 改进内容

### 1. 日志优化

#### 减少Info级别日志
- **FileService.cs**: 将文件对话框完成和路径选择的日志从 `Log.Information` 改为 `Log.Debug`
- **ConfigurationService.cs**: 将配置保存、加载、设置等操作的日志从 `Log.Information` 改为 `Log.Debug`
- **ServiceContainer.cs**: 将服务注册的详细日志从 `Log.Information` 改为 `Log.Debug`

#### 保留的关键日志
- 应用程序启动和关闭
- 错误和异常信息
- 重要的业务操作完成状态

### 2. 新增组件

#### ServiceLocator 类
**位置**: `src/Core/Services/ServiceLocator.cs`

提供简化的服务访问方式：

```csharp
// 初始化
ServiceLocator.Initialize(serviceContainer);

// 获取服务
var fileService = ServiceLocator.FileService;
var clipboardService = ServiceLocator.ClipboardService;
var configService = ServiceLocator.ConfigurationService;

// 泛型方法
var service = ServiceLocator.GetService<IMyService>();
var optionalService = ServiceLocator.TryGetService<IOptionalService>();
```

#### BaseViewModel 类
**位置**: `src/Core/ViewModels/BaseViewModel.cs`

为所有ViewModel提供统一的基类：

```csharp
public abstract class BaseViewModel : ObservableObject
{
    // 直接访问核心服务
    protected IFileService FileService => ServiceLocator.FileService;
    protected IClipboardService ClipboardService => ServiceLocator.ClipboardService;
    protected IConfigurationService ConfigurationService => ServiceLocator.ConfigurationService;
    
    // 泛型服务访问
    protected T GetService<T>() where T : class;
    protected T? TryGetService<T>() where T : class;
}
```

#### ServiceExtensions 类
**位置**: `src/Core/Services/ServiceExtensions.cs`

提供模块化的服务注册：

```csharp
// 注册核心服务
container.AddCoreServices();

// 注册HTTP服务（当实现后）
container.AddHttpService();

// 注册所有服务
container.AddAllServices();
```

### 3. ViewModel更新

#### 继承关系优化
- `TimestampConverterViewModel`: 继承 `BaseViewModel`
- `BaseConverterViewModel`: 继承 `BaseViewModel`
- `BaseToolViewModel`: 继承 `BaseViewModel`

#### 服务访问简化
ViewModel现在可以直接通过基类访问服务：

```csharp
public partial class MyViewModel : BaseViewModel
{
    private async Task SaveFile()
    {
        // 直接使用基类提供的服务
        var result = await FileService.SaveFileAsync();
        await ClipboardService.SetTextAsync(result);
        ConfigurationService.SetValue("lastSave", DateTime.Now);
    }
}
```

### 4. 应用程序初始化更新

**App.axaml.cs** 中的初始化流程：

```csharp
public override void OnFrameworkInitializationCompleted()
{
    // 配置服务容器
    ServiceContainer.Instance.ConfigureDefaultServices();
    
    // 初始化ServiceLocator
    ServiceLocator.Initialize(ServiceContainer.Instance);
    
    // 其他初始化...
}
```

## 使用指南

### 1. 在ViewModel中使用服务

```csharp
public partial class MyToolViewModel : BaseViewModel
{
    [RelayCommand]
    private async Task ProcessData()
    {
        try
        {
            // 使用文件服务
            var files = await FileService.OpenFileDialogAsync();
            
            // 使用剪贴板服务
            await ClipboardService.SetTextAsync("处理完成");
            
            // 使用配置服务
            ConfigurationService.SetValue("lastProcessed", DateTime.Now);
        }
        catch (Exception ex)
        {
            // 错误处理
        }
    }
}
```

### 2. 注册新服务

```csharp
// 在ServiceExtensions中添加新的扩展方法
public static ServiceContainer AddMyService(this ServiceContainer container)
{
    container.RegisterSingleton<IMyService, MyServiceImplementation>();
    return container;
}

// 在App.axaml.cs中使用
ServiceContainer.Instance
    .ConfigureDefaultServices()
    .AddMyService();
```

### 3. 访问自定义服务

```csharp
public partial class MyViewModel : BaseViewModel
{
    private void UseCustomService()
    {
        var myService = GetService<IMyService>();
        // 或者安全访问
        var optionalService = TryGetService<IOptionalService>();
        if (optionalService != null)
        {
            // 使用服务
        }
    }
}
```

## 最佳实践

### 1. 服务生命周期
- **单例服务**: 用于无状态的服务，如文件操作、配置管理
- **瞬态服务**: 用于有状态的服务或需要每次创建新实例的服务

### 2. 错误处理
- 始终在服务调用周围使用try-catch
- 记录错误日志但不暴露内部实现细节
- 为用户提供友好的错误消息

### 3. 日志记录
- 使用Debug级别记录详细操作信息
- 使用Information级别记录重要的业务事件
- 使用Error级别记录异常和错误

### 4. 测试友好
- 通过接口定义服务契约
- 在测试中可以轻松模拟服务
- 保持服务的单一职责原则

## 性能考虑

### 1. 服务解析
- ServiceLocator使用缓存提高性能
- 避免在循环中重复解析服务
- 优先使用属性访问而非方法调用

### 2. 内存管理
- 单例服务在应用程序生命周期内保持
- 及时释放不再需要的瞬态服务
- 避免循环依赖

## 未来改进

### 1. 构造函数注入
考虑在未来版本中支持构造函数注入，进一步提高可测试性。

### 2. 配置驱动的服务注册
支持通过配置文件动态注册服务。

### 3. 服务健康检查
添加服务健康检查机制，确保关键服务正常运行。

## 总结

通过这些改进，DevUtilities项目的依赖注入系统变得更加：
- **简洁**: ServiceLocator简化了服务访问
- **一致**: BaseViewModel提供统一的服务访问方式
- **可维护**: 模块化的服务注册和清晰的日志记录
- **可扩展**: 易于添加新服务和功能

这些改进为项目的长期维护和功能扩展奠定了坚实的基础。