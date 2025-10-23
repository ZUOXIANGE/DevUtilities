using System;
using Avalonia.Controls;
using Avalonia.Input;
using DevUtilities.ViewModels;
using Serilog;

namespace DevUtilities.Views;

public partial class Base64EncoderView : UserControl
{
    public Base64EncoderView()
    {
        Log.Debug("[Base64EncoderView] 开始初始化Base64编码器视图");
        
        try
        {
            InitializeComponent();
            Log.Debug("[Base64EncoderView] InitializeComponent完成");
            
            AddHandler(DragDrop.DropEvent, OnDrop);
            Log.Debug("[Base64EncoderView] 拖拽事件处理器已添加");
            
            // 订阅视图事件
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DataContextChanged += OnDataContextChanged;
            
            Log.Information("[Base64EncoderView] Base64编码器视图初始化完成");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[Base64EncoderView] Base64编码器视图初始化时发生错误");
            throw;
        }
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        Log.Debug("[Base64EncoderView] 检测到拖拽操作");
        
        try
        {
            if (DataContext is Base64EncoderViewModel viewModel)
            {
                Log.Debug("[Base64EncoderView] 正在处理拖拽的文件");
                await viewModel.HandleDrop(e);
                Log.Debug("[Base64EncoderView] 拖拽文件处理完成");
            }
            else
            {
                Log.Warning("[Base64EncoderView] DataContext不是Base64EncoderViewModel类型，无法处理拖拽");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[Base64EncoderView] 处理拖拽操作时发生错误");
        }
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Log.Debug("[Base64EncoderView] 视图已加载");
        Log.Debug("[Base64EncoderView] DataContext状态: {DataContextType}", 
            DataContext?.GetType().Name ?? "null");
    }

    private void OnUnloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Log.Debug("[Base64EncoderView] 视图正在卸载");
        
        // 取消事件订阅
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
        DataContextChanged -= OnDataContextChanged;
        
        Log.Information("[Base64EncoderView] Base64编码器视图已卸载");
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        Log.Debug("[Base64EncoderView] DataContext已更改为: {DataContextType}", 
            DataContext?.GetType().Name ?? "null");
    }
}