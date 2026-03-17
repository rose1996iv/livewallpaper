using System.ComponentModel;
using System.Windows;
using WallpaperEngine.UI.ViewModels;

namespace WallpaperEngine.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        StateChanged += OnWindowStateChanged;
    }

    public bool AllowClose { get; set; }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!AllowClose)
        {
            e.Cancel = true;
            Hide();
        }

        base.OnClosing(e);
    }

    private void OnWindowStateChanged(object? sender, EventArgs e)
    {
        if (!AllowClose && WindowState == WindowState.Minimized)
        {
            Hide();
        }
    }
}
