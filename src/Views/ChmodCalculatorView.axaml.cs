using Avalonia.Controls;
using System.Diagnostics;

namespace DevUtilities.Views;

public partial class ChmodCalculatorView : UserControl
{
    public ChmodCalculatorView()
    {
        Debug.WriteLine("[ChmodCalculatorView] 开始初始化视图");
        InitializeComponent();
        Debug.WriteLine("[ChmodCalculatorView] 视图初始化完成");
        
        // 监听视图加载事件
        Loaded += OnViewLoaded;
        Unloaded += OnViewUnloaded;
    }
    
    private void OnViewLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Debug.WriteLine("[ChmodCalculatorView] 视图已加载到可视化树");
        Debug.WriteLine($"[ChmodCalculatorView] DataContext类型: {DataContext?.GetType().Name ?? "null"}");
    }
    
    private void OnViewUnloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Debug.WriteLine("[ChmodCalculatorView] 视图已从可视化树卸载");
    }
}