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

    public void RefreshDataGrids_Click(object sender, RoutedEventArgs args)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.RefreshDataGrids();
        }              
    }

    public void AddOutpost_Click(object sender, RoutedEventArgs args)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            
        }              
    }

    private async void SelectFolder_Click(object sender, RoutedEventArgs args)
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(this);

            var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select the Folder to be Monitored",
                AllowMultiple = false,
            });

            if (folder != null && folder.Count > 0)
            {
                if (DataContext is MainWindowViewModel viewModel)
                {
                    
                    var selectedFolder = folder[0];
                    viewModel.FolderPath_Text = selectedFolder.Path.LocalPath;
                    Log.Information($"Selected Folder Path: {selectedFolder.Path.LocalPath}");
                }
                
            }
            else
            {
                Log.Information("Folder selection canceled or no folder selected.");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error selecting folder: {ex.Message}");
        }
    }

}