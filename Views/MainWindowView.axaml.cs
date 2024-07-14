using Avalonia.Controls;
using HashDog.ViewModels;
using System;
using Serilog;
using HashDog.Models;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Controls.Notifications;
using System.ComponentModel;

namespace HashDog.Views;

public partial class MainWindowView : Window
{
    private TrayIcon _trayIcon;
    private WindowNotificationManager _notificationManager;
    public MainWindowView()
    {
        InitializeComponent();

        this.DataContext = new MainWindowViewModel();

        _trayIcon = new TrayIcon
        {
            Icon = new WindowIcon("dog.png"),
            ToolTipText = "HashDog",
            IsVisible = true
        };

        _trayIcon.Clicked += (sender, e) =>
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        };

        _notificationManager = new WindowNotificationManager(this)
        {
            Position = NotificationPosition.BottomRight
        };

        this.Closing += OnClosing;
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            if (viewModel.IsHashDogEnabled)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                _trayIcon.IsVisible = false;
                _trayIcon.Dispose();
            }
        }
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