using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HashDog.ViewModels;
using HashDog.Models;
using Serilog;

namespace HashDog.Views;

public partial class OutpostFileView : Window
{
    public OutpostFileView()
    {
        InitializeComponent();
    }

    public OutpostFileView(OutpostFileViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

   
    private void DataGrid_SelectionOutpostFile(object sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid dataGrid && dataGrid.SelectedItem is FileEntry selectedOutpostFile)
        {
            if (DataContext is OutpostFileViewModel viewModel)
            {
                viewModel.SelectedOutpostFile = selectedOutpostFile;
                Log.Information("SELECT OUTPOSTFILE");
            }
        }
    }

}