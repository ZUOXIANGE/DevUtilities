using System;
using Avalonia.Controls;
using Serilog;

namespace DevUtilities.Views
{
    public partial class RegexTesterView : UserControl
    {
        public RegexTesterView()
        {
            Log.Debug("[RegexTesterView] 开始初始化正则表达式测试器视图");
            
            try
            {
                InitializeComponent();
                Log.Debug("[RegexTesterView] InitializeComponent完成");
                
                // 订阅视图事件
                Loaded += OnLoaded;
                Unloaded += OnUnloaded;
                DataContextChanged += OnDataContextChanged;
                
                Log.Information("[RegexTesterView] 正则表达式测试器视图初始化完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[RegexTesterView] 正则表达式测试器视图初始化时发生错误");
                throw;
            }
        }

        private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Log.Debug("[RegexTesterView] 视图已加载");
            Log.Debug("[RegexTesterView] DataContext状态: {DataContextType}", 
                DataContext?.GetType().Name ?? "null");
        }

        private void OnUnloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Log.Debug("[RegexTesterView] 视图正在卸载");
            
            // 取消事件订阅
            Loaded -= OnLoaded;
            Unloaded -= OnUnloaded;
            DataContextChanged -= OnDataContextChanged;
            
            Log.Information("[RegexTesterView] 正则表达式测试器视图已卸载");
        }

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            Log.Debug("[RegexTesterView] DataContext已更改为: {DataContextType}", 
                DataContext?.GetType().Name ?? "null");
        }
    }
}