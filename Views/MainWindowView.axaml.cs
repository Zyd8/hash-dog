using Avalonia.Controls;
using HashDog.ViewModels;
using System;
using Serilog;
using HashDog.Models;


namespace HashDog.Views;

public partial class MainWindowView : Window
{
    public MainWindowView()
    {
        InitializeComponent();

        this.DataContext = new MainWindowViewModel();      
    
        this.Closed += OnWindowClosed;
    }

    // for testing purposes only
    private void OnWindowClosed(object? sender, EventArgs e)
    {
        var _instance = Service.Instance;
        _instance.Dispose(); 
        Log.Information("Service Disposed");
    }

    private void DataGrid_SelectionOutpost(object sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid dataGrid && dataGrid.SelectedItem is OutpostEntry selectedOutpost)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.SelectedOutpost = selectedOutpost;
                Log.Information("SELECT OUTPOST");
            }
        }
    }

    private void DataGrid_SelectionOutpostFile(object sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid dataGrid && dataGrid.SelectedItem is FileEntry selectedOutpostFile)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.SelectedOutpostFile = selectedOutpostFile;
                Log.Information("SELECT OUTPOSTFILE");
            }
        }
    }

    private void DataGrid_SelectionMismatchArchive(object sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid dataGrid && dataGrid.SelectedItem is MismatchArchiveEntry selectedMismatchArchive)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.SelectedMismatchArchive = selectedMismatchArchive;
                Log.Information("SELECT MISMATCHARCHIVE");
            }
        }
    }
}