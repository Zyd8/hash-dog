using Avalonia.Controls;
using HashDog.ViewModels;
using System;
using Serilog;
using HashDog.Models;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;


namespace HashDog.Views;

public partial class MainWindowView : Window
{
    public MainWindowView()
    {
        InitializeComponent();

        this.DataContext = new MainWindowViewModel();      
    
        //this.Closed += OnWindowClosed;
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

    public void RefreshDataGrids_Click(object sender, RoutedEventArgs args)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.RefreshDataGrids();
        }              
    }

    public void ViewOutpost_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is OutpostEntry outpost)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.SelectedOutpost = outpost;
            }
        }
    }

    public void DeleteOutpost_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is OutpostEntry outpost)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                Log.Information(outpost.CheckPath);
                viewModel.RemoveOutpost(outpost);
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

    public void AddOutpost_Click(object sender, RoutedEventArgs args)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            AddOutpostView addOutpostView = new AddOutpostView(viewModel);
            addOutpostView.Show();
        }

    }


}