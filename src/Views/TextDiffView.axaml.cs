using System;
using Avalonia.Controls;
using Serilog;

namespace DevUtilities.Views
{
    public partial class TextDiffView : UserControl
    {
        public TextDiffView()
        {
            Log.Debug("[TextDiffView] 开始初始化文本对比视图");
            
            try
            {
                InitializeComponent();
                Log.Debug("[TextDiffView] InitializeComponent完成");
                
                // 订阅视图事件
                Loaded += OnLoaded;
                Unloaded += OnUnloaded;
                
                Log.Information("[TextDiffView] 文本对比视图初始化完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[TextDiffView] 文本对比视图初始化时发生错误");
                throw;
            }
        }
        
        private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Log.Information("[TextDiffView] 文本对比视图已加载");
            
            if (DataContext != null)
            {
                Log.Debug("[TextDiffView] DataContext类型: {DataContextType}", DataContext.GetType().Name);
            }
            else
            {
                Log.Warning("[TextDiffView] DataContext为null");
            }
        }
        
        private void OnUnloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Log.Information("[TextDiffView] 文本对比视图已卸载");
            
            // 取消订阅事件
            Loaded -= OnLoaded;
            Unloaded -= OnUnloaded;
        }
        
        protected override void OnDataContextChanged(EventArgs e)
        {
            Log.Debug("[TextDiffView] DataContext发生变化");
            
            if (DataContext != null)
            {
                Log.Debug("[TextDiffView] 新DataContext类型: {DataContextType}", DataContext.GetType().Name);
            }
            else
            {
                Log.Debug("[TextDiffView] DataContext设置为null");
            }
            
            base.OnDataContextChanged(e);
        }
    }
}