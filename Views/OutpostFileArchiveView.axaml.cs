using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HashDog.ViewModels;

namespace HashDog.Views;

public partial class OutpostFileArchiveView : Window
{
    public OutpostFileArchiveView()
    {
        InitializeComponent();
        this.DataContext = new OutpostFileArchiveViewModel();
    }

    public OutpostFileArchiveView(OutpostFileArchiveViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}