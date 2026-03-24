using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;

namespace LiveWallpaper.Utility.Screensaver;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Point _mousePosition;
    private bool _isActive;

    public MainWindow()
    {
        InitializeComponent();
        _mousePosition = new Point(-1, -1);
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        _isActive = true;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (_isActive)
            Application.Current.Shutdown();
    }

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isActive) return;

        Point currentPosition = e.GetPosition(this);

        if (_mousePosition.X == -1 && _mousePosition.Y == -1) // First move
        {
            _mousePosition = currentPosition;
        }
        else if (Math.Abs(_mousePosition.X - currentPosition.X) > 5 || 
                 Math.Abs(_mousePosition.Y - currentPosition.Y) > 5)
        {
            Application.Current.Shutdown();
        }
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_isActive)
            Application.Current.Shutdown();
    }
}