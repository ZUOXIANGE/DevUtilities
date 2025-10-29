using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DevUtilities.ViewModels;

namespace DevUtilities.Views;

public partial class SettingsDialog : Window
{
    public SettingsDialog()
    {
        InitializeComponent();
    }

    public SettingsDialog(SettingsDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
        
        // 订阅ViewModel的关闭事件
        if (viewModel != null)
        {
            viewModel.CloseRequested += OnCloseRequested;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void OnCloseRequested(object? sender, bool dialogResult)
    {
        Close(dialogResult);
    }
    
    protected override void OnClosed(EventArgs e)
    {
        // 取消订阅事件
        if (DataContext is SettingsDialogViewModel viewModel)
        {
            viewModel.CloseRequested -= OnCloseRequested;
        }
        base.OnClosed(e);
    }
}