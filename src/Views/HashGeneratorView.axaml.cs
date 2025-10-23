using System;
using Avalonia.Controls;
using Serilog;

namespace DevUtilities.Views;

public partial class HashGeneratorView : UserControl
{
    public HashGeneratorView()
    {
        Log.Debug("[HashGeneratorView] 开始初始化哈希生成器视图");
        
        try
        {
            InitializeComponent();
            Log.Debug("[HashGeneratorView] InitializeComponent完成");
            
            // 订阅视图事件
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DataContextChanged += OnDataContextChanged;
            
            Log.Information("[HashGeneratorView] 哈希生成器视图初始化完成");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[HashGeneratorView] 哈希生成器视图初始化时发生错误");
            throw;
        }
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Log.Debug("[HashGeneratorView] 视图已加载");
        Log.Debug("[HashGeneratorView] DataContext状态: {DataContextType}", 
            DataContext?.GetType().Name ?? "null");
    }

    private void OnUnloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Log.Debug("[HashGeneratorView] 视图正在卸载");
        
        // 取消事件订阅
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
        DataContextChanged -= OnDataContextChanged;
        
        Log.Information("[HashGeneratorView] 哈希生成器视图已卸载");
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        Log.Debug("[HashGeneratorView] DataContext已更改为: {DataContextType}", 
            DataContext?.GetType().Name ?? "null");
    }
}