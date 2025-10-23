using System;
using Avalonia.Controls;
using Serilog;

namespace DevUtilities.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        Log.Debug("[MainWindow] 开始初始化主窗口");
        
        try
        {
            InitializeComponent();
            Log.Debug("[MainWindow] InitializeComponent完成");
            
            // 订阅窗口事件
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            Closing += OnClosing;
            
            Log.Debug("[MainWindow] 主窗口初始化完成");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[MainWindow] 主窗口初始化时发生错误");
            throw;
        }
    }
    
    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Log.Debug("[MainWindow] 主窗口已加载");
        Log.Debug("[MainWindow] 窗口大小: {Width}x{Height}", Width, Height);
        
        if (DataContext != null)
        {
            Log.Debug("[MainWindow] DataContext类型: {DataContextType}", DataContext.GetType().Name);
        }
        else
        {
            Log.Warning("[MainWindow] DataContext为null");
        }
    }
    
    private void OnUnloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Log.Debug("[MainWindow] 主窗口已卸载");
        
        // 取消订阅事件
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
        Closing -= OnClosing;
    }
    
    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        Log.Debug("[MainWindow] 主窗口正在关闭");
        Log.Debug("[MainWindow] 关闭原因: {CloseReason}", e.CloseReason);
    }
    
    protected override void OnDataContextChanged(EventArgs e)
    {
        Log.Debug("[MainWindow] DataContext发生变化");
        
        if (DataContext != null)
        {
            Log.Debug("[MainWindow] 新DataContext类型: {DataContextType}", DataContext.GetType().Name);
        }
        else
        {
            Log.Debug("[MainWindow] DataContext设置为null");
        }
        
        base.OnDataContextChanged(e);
    }
}