using System;
using Avalonia.Controls;
using Serilog;

namespace DevUtilities.Views;

public partial class JsonFormatterView : UserControl
{
    public JsonFormatterView()
    {
        Log.Debug("[JsonFormatterView] 开始初始化JSON格式化视图");
        
        try
        {
            InitializeComponent();
            Log.Debug("[JsonFormatterView] InitializeComponent完成");
            
            // 订阅视图事件
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            
            Log.Information("[JsonFormatterView] JSON格式化视图初始化完成");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[JsonFormatterView] JSON格式化视图初始化时发生错误");
            throw;
        }
    }
    
    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Log.Information("[JsonFormatterView] JSON格式化视图已加载");
        
        if (DataContext != null)
        {
            Log.Debug("[JsonFormatterView] DataContext类型: {DataContextType}", DataContext.GetType().Name);
        }
        else
        {
            Log.Warning("[JsonFormatterView] DataContext为null");
        }
    }
    
    private void OnUnloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Log.Information("[JsonFormatterView] JSON格式化视图已卸载");
        
        // 取消订阅事件
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
    }
    
    protected override void OnDataContextChanged(EventArgs e)
    {
        Log.Debug("[JsonFormatterView] DataContext发生变化");
        
        if (DataContext != null)
        {
            Log.Debug("[JsonFormatterView] 新DataContext类型: {DataContextType}", DataContext.GetType().Name);
        }
        else
        {
            Log.Debug("[JsonFormatterView] DataContext设置为null");
        }
        
        base.OnDataContextChanged(e);
    }
}