using Avalonia.Controls;
using HashDog.ViewModels;
using System;
using Serilog;


namespace HashDog.Views;

public partial class MainWindowView : Window
{
    private Service _instance;

    public MainWindowView()
    {
        InitializeComponent();
        this.DataContext = new MainWindowViewModel();


        _instance = Service.Instance;

        _instance.AddOutpost(_instance.GetNewOutpostPath());
        _instance.AddOutpost(@"C:\Users\Zyd\testing\outpost2");

        _instance.SetScheduleRunTimer();

        Log.Information("Testingggggg");


        this.Closed += OnWindowClosed;
    }

    // for testing purposes only
    private void OnWindowClosed(object? sender, EventArgs e)
    {
        _instance.Dispose(); 
        Log.Information("Service Disposed");
    }
}