# 架构设计

本文档详细介绍 DevUtilities 项目的架构设计、技术选型和实现原理。

## 🏗️ 整体架构

### 架构概览
```
┌─────────────────────────────────────────────────────────────┐
│                    DevUtilities 应用程序                      │
├─────────────────────────────────────────────────────────────┤
│                        UI 层 (Views)                        │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────┐ │
│  │ MainWindow  │ │ QrCodeView  │ │ CryptoView  │ │   ...   │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────┘ │
├─────────────────────────────────────────────────────────────┤
│                    ViewModel 层 (MVVM)                      │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────┐ │
│  │MainViewModel│ │QrCodeViewModel│CryptoViewModel│   ...   │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────┘ │
├─────────────────────────────────────────────────────────────┤
│                      Model 层 (Models)                      │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────┐ │
│  │  ToolModel  │ │ QrCodeModel │ │ CryptoModel │ │   ...   │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────┘ │
├─────────────────────────────────────────────────────────────┤
│                     Service 层 (Services)                   │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────┐ │
│  │QrCodeService│ │CryptoService│ │FileService  │ │   ...   │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────┘ │
├─────────────────────────────────────────────────────────────┤
│                    基础设施层 (Infrastructure)                │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────┐ │
│  │   Logging   │ │Configuration│ │  Validation │ │   DI    │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### 技术栈
- **UI 框架**: Avalonia UI 11.0+
- **开发平台**: .NET 9.0
- **架构模式**: MVVM (Model-View-ViewModel)
- **依赖注入**: Microsoft.Extensions.DependencyInjection
- **配置管理**: Microsoft.Extensions.Configuration
- **日志记录**: Microsoft.Extensions.Logging

## 🎯 设计原则

### SOLID 原则
1. **单一职责原则 (SRP)**: 每个类只负责一个功能
2. **开闭原则 (OCP)**: 对扩展开放，对修改关闭
3. **里氏替换原则 (LSP)**: 子类可以替换父类
4. **接口隔离原则 (ISP)**: 使用多个专门的接口
5. **依赖倒置原则 (DIP)**: 依赖抽象而不是具体实现

### 设计模式
- **MVVM 模式**: 分离 UI 逻辑和业务逻辑
- **工厂模式**: 创建工具实例
- **策略模式**: 不同的加密算法实现
- **观察者模式**: 属性变更通知
- **命令模式**: UI 操作封装

## 📁 项目结构

### 目录结构
```
DevUtilities/
├── Assets/                     # 资源文件
│   ├── Icons/                  # 图标资源
│   └── Styles/                 # 样式文件
├── Models/                     # 数据模型
│   ├── ToolModel.cs           # 工具模型基类
│   ├── QrCodeModel.cs         # 二维码模型
│   └── CryptoModel.cs         # 加密模型
├── ViewModels/                 # 视图模型
│   ├── MainViewModel.cs       # 主窗口视图模型
│   ├── QrCodeViewModel.cs     # 二维码视图模型
│   └── CryptoToolsViewModel.cs # 加密工具视图模型
├── Views/                      # 视图
│   ├── MainWindow.axaml       # 主窗口
│   ├── QrCodeView.axaml       # 二维码视图
│   └── CryptoToolsView.axaml  # 加密工具视图
├── Services/                   # 服务层
│   ├── IQrCodeService.cs      # 二维码服务接口
│   ├── QrCodeService.cs       # 二维码服务实现
│   ├── ICryptoService.cs      # 加密服务接口
│   └── CryptoService.cs       # 加密服务实现
├── Infrastructure/             # 基础设施
│   ├── Configuration/         # 配置管理
│   ├── Logging/              # 日志记录
│   └── Validation/           # 数据验证
├── Converters/                # 值转换器
├── Controls/                  # 自定义控件
└── App.axaml                  # 应用程序入口
```

### 命名约定
- **命名空间**: `DevUtilities.{Layer}.{Feature}`
- **类名**: PascalCase (如 `QrCodeService`)
- **方法名**: PascalCase (如 `GenerateQrCode`)
- **属性名**: PascalCase (如 `InputText`)
- **字段名**: camelCase (如 `_qrCodeService`)
- **常量**: UPPER_CASE (如 `MAX_SIZE`)

## 🔧 核心组件

### 1. MVVM 架构实现

#### ViewModelBase
```csharp
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
            
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
```

#### RelayCommand
```csharp
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;
    
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }
    
    public event EventHandler? CanExecuteChanged;
    
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
    
    public void Execute(object? parameter) => _execute(parameter);
    
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
```

### 2. 依赖注入配置

#### ServiceCollection 配置
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDevUtilitiesServices(this IServiceCollection services)
    {
        // 注册服务
        services.AddSingleton<IQrCodeService, QrCodeService>();
        services.AddSingleton<ICryptoService, CryptoService>();
        services.AddSingleton<IFileService, FileService>();
        
        // 注册 ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<QrCodeViewModel>();
        services.AddTransient<CryptoToolsViewModel>();
        
        // 注册配置
        services.AddSingleton<IConfiguration>(provider => 
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        });
        
        // 注册日志
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddFile("logs/app.log");
        });
        
        return services;
    }
}
```

### 3. 服务层设计

#### 服务接口定义
```csharp
public interface IQrCodeService
{
    Task<byte[]> GenerateQrCodeAsync(string content, QrCodeOptions options);
    Task<string> ScanQrCodeAsync(byte[] imageData);
    Task<bool> SaveQrCodeAsync(byte[] qrCodeData, string filePath);
}

public interface ICryptoService
{
    Task<string> EncryptAsync(string plainText, string key, CryptoOptions options);
    Task<string> DecryptAsync(string cipherText, string key, CryptoOptions options);
    Task<string> HashAsync(string input, HashAlgorithm algorithm);
}
```

#### 服务实现
```csharp
public class QrCodeService : IQrCodeService
{
    private readonly ILogger<QrCodeService> _logger;
    
    public QrCodeService(ILogger<QrCodeService> logger)
    {
        _logger = logger;
    }
    
    public async Task<byte[]> GenerateQrCodeAsync(string content, QrCodeOptions options)
    {
        try
        {
            _logger.LogInformation("Generating QR code for content length: {Length}", content.Length);
            
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(content, options.ErrorCorrectionLevel);
            
            using var qrCode = new PngByteQRCode(qrCodeData);
            return await Task.FromResult(qrCode.GetGraphic(options.PixelsPerModule));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate QR code");
            throw;
        }
    }
}
```

## 🔄 数据流

### MVVM 数据绑定流程
```
┌─────────────┐    Command     ┌─────────────┐    Service Call    ┌─────────────┐
│    View     │ ──────────────→ │  ViewModel  │ ─────────────────→ │   Service   │
│   (XAML)    │                │   (Logic)   │                    │ (Business)  │
└─────────────┘                └─────────────┘                    └─────────────┘
       ↑                              ↑                                   │
       │         Property Changed     │            Result                  │
       └──────────────────────────────┴────────────────────────────────────┘
```

### 事件处理流程
1. **用户交互**: 用户在 UI 上执行操作
2. **命令触发**: UI 控件触发绑定的命令
3. **ViewModel 处理**: ViewModel 接收命令并调用服务
4. **服务执行**: Service 层执行具体的业务逻辑
5. **结果返回**: 结果通过 ViewModel 返回到 UI
6. **UI 更新**: UI 通过数据绑定自动更新

## 🛡️ 错误处理

### 异常处理策略
```csharp
public class GlobalExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }
    
    public void HandleException(Exception exception, string context)
    {
        _logger.LogError(exception, "Unhandled exception in {Context}", context);
        
        // 根据异常类型进行不同处理
        switch (exception)
        {
            case ValidationException validationEx:
                ShowValidationError(validationEx.Message);
                break;
            case ServiceException serviceEx:
                ShowServiceError(serviceEx.Message);
                break;
            default:
                ShowGenericError("An unexpected error occurred");
                break;
        }
    }
}
```

### 验证机制
```csharp
public class ValidationService : IValidationService
{
    public ValidationResult ValidateQrCodeInput(string content)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(content))
        {
            result.AddError("Content cannot be empty");
        }
        
        if (content.Length > 4296)
        {
            result.AddError("Content exceeds maximum length");
        }
        
        return result;
    }
}
```

## 🔌 扩展性设计

### 插件架构
```csharp
public interface IToolPlugin
{
    string Name { get; }
    string Description { get; }
    string Icon { get; }
    UserControl CreateView();
    ViewModelBase CreateViewModel();
}

public class PluginManager
{
    private readonly List<IToolPlugin> _plugins = new();
    
    public void LoadPlugins(string pluginDirectory)
    {
        var assemblies = Directory.GetFiles(pluginDirectory, "*.dll")
            .Select(Assembly.LoadFrom);
            
        foreach (var assembly in assemblies)
        {
            var pluginTypes = assembly.GetTypes()
                .Where(t => typeof(IToolPlugin).IsAssignableFrom(t) && !t.IsInterface);
                
            foreach (var pluginType in pluginTypes)
            {
                var plugin = (IToolPlugin)Activator.CreateInstance(pluginType)!;
                _plugins.Add(plugin);
            }
        }
    }
}
```

### 配置系统
```csharp
public class AppConfiguration
{
    public string Theme { get; set; } = "Light";
    public string Language { get; set; } = "en-US";
    public bool AutoSave { get; set; } = true;
    public bool CheckUpdates { get; set; } = true;
    public Dictionary<string, object> ToolSettings { get; set; } = new();
}

public class ConfigurationManager
{
    private readonly string _configPath;
    private AppConfiguration _configuration;
    
    public ConfigurationManager()
    {
        _configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DevUtilities",
            "config.json"
        );
        
        LoadConfiguration();
    }
    
    public void SaveConfiguration()
    {
        var json = JsonSerializer.Serialize(_configuration, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
        File.WriteAllText(_configPath, json);
    }
}
```

## 📊 性能优化

### 内存管理
- **对象池**: 重用频繁创建的对象
- **弱引用**: 避免内存泄漏
- **及时释放**: 实现 IDisposable 接口

### UI 性能
- **虚拟化**: 大列表使用虚拟化
- **异步操作**: 长时间操作使用异步
- **缓存机制**: 缓存计算结果

### 代码示例
```csharp
public class PerformanceOptimizedService
{
    private readonly ObjectPool<StringBuilder> _stringBuilderPool;
    private readonly MemoryCache _cache;
    
    public PerformanceOptimizedService(
        ObjectPool<StringBuilder> stringBuilderPool,
        MemoryCache cache)
    {
        _stringBuilderPool = stringBuilderPool;
        _cache = cache;
    }
    
    public async Task<string> ProcessDataAsync(string input)
    {
        // 检查缓存
        if (_cache.TryGetValue(input, out string? cachedResult))
        {
            return cachedResult;
        }
        
        // 使用对象池
        var sb = _stringBuilderPool.Get();
        try
        {
            // 处理逻辑
            var result = await ProcessWithStringBuilder(sb, input);
            
            // 缓存结果
            _cache.Set(input, result, TimeSpan.FromMinutes(10));
            
            return result;
        }
        finally
        {
            _stringBuilderPool.Return(sb);
        }
    }
}
```

## 🧪 测试架构

### 测试策略
- **单元测试**: 测试单个组件
- **集成测试**: 测试组件交互
- **UI 测试**: 测试用户界面
- **性能测试**: 测试性能指标

### 测试结构
```
Tests/
├── UnitTests/
│   ├── Services/
│   ├── ViewModels/
│   └── Models/
├── IntegrationTests/
│   ├── Services/
│   └── Infrastructure/
├── UITests/
│   ├── Views/
│   └── Workflows/
└── PerformanceTests/
    ├── Load/
    └── Stress/
```

## 🔄 持续集成

### CI/CD 流程
```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 9.0.x
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal
```

## 📈 监控和日志

### 日志配置
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "DevUtilities": "Debug",
      "Microsoft": "Warning"
    },
    "Console": {
      "IncludeScopes": true
    },
    "File": {
      "Path": "logs/app-.log",
      "RollingInterval": "Day",
      "RetainedFileCountLimit": 30
    }
  }
}
```

### 性能监控
```csharp
public class PerformanceMonitor
{
    private readonly ILogger<PerformanceMonitor> _logger;
    private readonly DiagnosticSource _diagnosticSource;
    
    public void TrackOperation(string operationName, Action operation)
    {
        using var activity = _diagnosticSource.StartActivity(operationName, null);
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            operation();
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "Operation {OperationName} completed in {ElapsedMs}ms",
                operationName,
                stopwatch.ElapsedMilliseconds
            );
        }
    }
}
```

---

这个架构设计确保了 DevUtilities 项目的可维护性、可扩展性和高性能。通过清晰的分层架构和现代化的技术栈，项目能够持续演进并满足用户需求。