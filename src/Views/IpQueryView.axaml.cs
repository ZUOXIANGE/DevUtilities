using Avalonia.Controls;
using Avalonia.Interactivity;
using DevUtilities.ViewModels;

namespace DevUtilities.Views;

public partial class IpQueryView : UserControl
{
    public IpQueryView()
    {
        InitializeComponent();
    }

    private void OnQuickQueryClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string ip && DataContext is IpQueryViewModel viewModel)
        {
            viewModel.InputIp = ip;
            viewModel.QueryIpCommand.Execute(null);
        }
    }
}