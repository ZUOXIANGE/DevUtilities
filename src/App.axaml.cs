using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using DevUtilities.ViewModels;
using DevUtilities.Views;
using DevUtilities.Core.Services;

namespace DevUtilities;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        // 初始化服务容器
        InitializeServices();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// 初始化服务容器和注册服务
    /// </summary>
    private void InitializeServices()
    {
        try
        {
            // 配置默认服务
            ServiceContainer.Instance.ConfigureDefaultServices();
            
            // 这里可以添加更多的服务注册
            // 例如：ServiceContainer.Instance.RegisterSingleton<ICustomService, CustomService>();
        }
        catch (System.Exception ex)
        {
            // 记录服务初始化错误
            System.Diagnostics.Debug.WriteLine($"服务初始化失败: {ex.Message}");
        }
    }
}