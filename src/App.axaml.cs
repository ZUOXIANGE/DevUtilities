using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DevUtilities.Core.Services;
using DevUtilities.ViewModels;
using DevUtilities.Views;
using Serilog;

namespace DevUtilities;

public partial class App : Application
{
    public override void Initialize()
    {
        Log.Debug("[App] 开始初始化应用程序");
        
        try
        {
            Log.Debug("[App] 开始加载Avalonia XAML资源");
            AvaloniaXamlLoader.Load(this);
            Log.Debug("[App] Avalonia XAML资源加载完成");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[App] 加载Avalonia XAML资源时发生错误");
            throw;
        }
        
        Log.Information("[App] 应用程序初始化完成");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Log.Debug("[App] 开始框架初始化完成处理");
        
        try
        {
            Log.Debug("[App] 开始配置服务容器");
            ConfigureServices();
            Log.Debug("[App] 服务容器配置完成");

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                Log.Debug("[App] 检测到桌面应用程序生命周期");
                
                Log.Debug("[App] 开始创建MainWindow");
                desktop.MainWindow = new MainWindow();
                Log.Debug("[App] MainWindow创建完成");
                
                Log.Debug("[App] 开始创建MainWindowViewModel");
                var viewModel = new MainWindowViewModel();
                desktop.MainWindow.DataContext = viewModel;
                Log.Debug("[App] MainWindow DataContext设置完成，ViewModel类型: {ViewModelType}", viewModel.GetType().Name);
            }
            else
            {
                Log.Warning("[App] 未检测到桌面应用程序生命周期，ApplicationLifetime类型: {LifetimeType}", 
                    ApplicationLifetime?.GetType().Name ?? "null");
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "[App] 框架初始化完成处理时发生严重错误");
            throw;
        }

        Log.Debug("[App] 调用基类OnFrameworkInitializationCompleted");
        base.OnFrameworkInitializationCompleted();
        Log.Debug("[App] 框架初始化完成");
    }

    private void ConfigureServices()
    {
        Log.Debug("[App] 开始配置默认服务");
        
        try
        {
            var container = ServiceContainer.Instance;
            Log.Debug("[App] 获取ServiceContainer实例成功");
            
            container.ConfigureDefaultServices();
            Log.Information("[App] 默认服务配置完成");
            
            // 初始化服务定位器
            ServiceLocator.Initialize(container);
            
            // 记录已注册的服务
            var registeredServices = container.GetRegisteredServiceTypes();
            Log.Debug("[App] 已注册的服务数量: {ServiceCount}", registeredServices.Count());
            
            foreach (var serviceType in registeredServices)
            {
                Log.Debug("[App] 已注册服务: {ServiceType}", serviceType.Name);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[App] 配置服务时发生错误");
            throw;
        }
    }
}