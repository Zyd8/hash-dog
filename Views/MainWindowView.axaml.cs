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
}