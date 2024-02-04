using Avalonia.Controls;
using HashDog.ViewModels;
using Avalonia.Markup.Xaml;

namespace HashDog;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}