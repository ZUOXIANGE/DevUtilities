using Avalonia.Controls;
using DevUtilities.ViewModels;

namespace DevUtilities.Views;

public partial class JsonExampleGeneratorView : UserControl
{
    public JsonExampleGeneratorView()
    {
        InitializeComponent();
    }

    public JsonExampleGeneratorView(JsonExampleGeneratorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}