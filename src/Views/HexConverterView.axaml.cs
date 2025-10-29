using System;
using Avalonia.Controls;
using Serilog;

namespace DevUtilities.Views;

public partial class HexConverterView : UserControl
{
    public HexConverterView()
    {
        Log.Debug("[HexConverterView] 开始初始化十六进制转换器视图");
            
        try
        {
            InitializeComponent();
            Log.Debug("[HexConverterView] InitializeComponent完成");
                
            // 订阅视图事件
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DataContextChanged += OnDataContextChanged;
                
            Log.Information("[HexConverterView] 十六进制转换器视图初始化完成");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[HexConverterView] 十六进制转换器视图初始化时发生错误");
            throw;
        }
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Log.Debug("[HexConverterView] 视图已加载");
        Log.Debug("[HexConverterView] DataContext状态: {DataContextType}", 
            DataContext?.GetType().Name ?? "null");
    }

    private void OnUnloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Log.Debug("[HexConverterView] 视图正在卸载");
            
        // 取消事件订阅
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
        DataContextChanged -= OnDataContextChanged;
            
        Log.Information("[HexConverterView] 十六进制转换器视图已卸载");
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        Log.Debug("[HexConverterView] DataContext已更改为: {DataContextType}", 
            DataContext?.GetType().Name ?? "null");
    }
}
