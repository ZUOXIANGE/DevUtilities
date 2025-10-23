using System;
using Avalonia.Controls;
using Serilog;

namespace DevUtilities.Views;

public partial class ParquetViewerView : UserControl
{
    public ParquetViewerView()
    {
        Log.Debug("[ParquetViewerView] 开始初始化Parquet查看器视图");
        
        try
        {
            InitializeComponent();
            Log.Debug("[ParquetViewerView] InitializeComponent完成");
            
            // 订阅视图事件
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            
            Log.Information("[ParquetViewerView] Parquet查看器视图初始化完成");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ParquetViewerView] Parquet查看器视图初始化时发生错误");
            throw;
        }
    }
    
    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Log.Information("[ParquetViewerView] Parquet查看器视图已加载");
        
        if (DataContext != null)
        {
            Log.Debug("[ParquetViewerView] DataContext类型: {DataContextType}", DataContext.GetType().Name);
        }
        else
        {
            Log.Warning("[ParquetViewerView] DataContext为null");
        }
    }
    
    private void OnUnloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Log.Information("[ParquetViewerView] Parquet查看器视图已卸载");
        
        // 取消订阅事件
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
    }
    
    protected override void OnDataContextChanged(EventArgs e)
    {
        Log.Debug("[ParquetViewerView] DataContext发生变化");
        
        if (DataContext != null)
        {
            Log.Debug("[ParquetViewerView] 新DataContext类型: {DataContextType}", DataContext.GetType().Name);
        }
        else
        {
            Log.Debug("[ParquetViewerView] DataContext设置为null");
        }
        
        base.OnDataContextChanged(e);
    }
}