using Avalonia.Controls;
using Avalonia.Input;
using DevUtilities.ViewModels;

namespace DevUtilities.Views;

public partial class Base64EncoderView : UserControl
{
    public Base64EncoderView()
    {
        InitializeComponent();
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        if (DataContext is Base64EncoderViewModel viewModel)
        {
            await viewModel.HandleDrop(e);
        }
    }
}