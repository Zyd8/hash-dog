using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using HashDog.ViewModels;
using Serilog;
using System;

namespace HashDog;

public partial class AddOutpostView : Window
{

    public AddOutpostView(MainWindowViewModel mainWindowViewModel)
    {
        InitializeComponent();
        this.DataContext = new AddOutpostViewModel(mainWindowViewModel);
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
                if (DataContext is AddOutpostViewModel viewModel)
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

    public void AddOutpostConfirm_Click(object sender, RoutedEventArgs args)
    {
        if (DataContext is AddOutpostViewModel viewModel)
        {
            viewModel.AddOutpost(this);
        }
    }

}