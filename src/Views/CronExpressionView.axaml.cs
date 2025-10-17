using Avalonia.Controls;
using DevUtilities.ViewModels;

namespace DevUtilities.Views;

public partial class CronExpressionView : UserControl
{
    public CronExpressionView()
    {
        InitializeComponent();
        DataContext = new CronExpressionViewModel();
    }
}