using System;
using Avalonia.Controls;
using Serilog;

namespace DevUtilities.Views;

public partial class JsonYamlConverterView : UserControl
{
    public JsonYamlConverterView()
    {
        Log.Debug("[JsonYamlConverterView] 开始初始化JSON/YAML转换器视图");
        
        try
        {
            InitializeComponent();
            Log.Debug("[JsonYamlConverterView] InitializeComponent完成");
            
            // 订阅视图事件
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            
            Log.Information("[JsonYamlConverterView] JSON/YAML转换器视图初始化完成");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[JsonYamlConverterView] JSON/YAML转换器视图初始化时发生错误");
            throw;
        }
    }
    
    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Log.Debug("[JsonYamlConverterView] 视图已加载");
        
        try
        {
            // 可以在这里添加视图加载后的初始化逻辑
            // 例如：设置焦点、加载用户设置等
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[JsonYamlConverterView] 视图加载事件处理时发生错误");
        }
    }
    
    private void OnUnloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Log.Debug("[JsonYamlConverterView] 视图已卸载");
        
        try
        {
            // 清理资源
            Loaded -= OnLoaded;
            Unloaded -= OnUnloaded;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[JsonYamlConverterView] 视图卸载事件处理时发生错误");
        }
    }
}