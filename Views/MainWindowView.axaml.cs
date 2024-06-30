using Avalonia.Controls;
using System.Linq;
using HashDog;
using HashDog.ViewModels;
using System;
using Serilog;


namespace HashDog.Views;

public partial class MainWindowView : Window
{
    private Service _service;

    public MainWindowView()
    {
        InitializeComponent();
        this.DataContext = new MainWindowViewModel();

       
        _service = new Service();

        _service.AddOutpost(_service.GetNewOutpostPath());
        _service.AddOutpost(@"C:\Users\Zyd\testing\outpost2");

        _service.SetScheduleRunTimer();

        Log.Information("Test again");


        this.Closed += OnWindowClosed;
    }

    // for testing purposes only
    private void OnWindowClosed(object? sender, EventArgs e)
    {
        _service.Dispose(); 
        Log.Information("Service Disposed");
    }
}