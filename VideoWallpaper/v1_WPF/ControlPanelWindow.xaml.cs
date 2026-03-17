using System.Windows;
using Wpf.Ui.Controls;

namespace VideoWallpaper
{
    public partial class ControlPanelWindow : FluentWindow
    {
        private App _app;
        private bool _isInitialized = false;

        public ControlPanelWindow(App app)
        {
            InitializeComponent();
            _app = app;
            ExtendsContentIntoTitleBar = true;
            Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);

            // Set initial UI state
            AutoStartSwitch.IsChecked = AppSettings.AutoStart;
            MuteSwitch.IsChecked = AppSettings.IsMuted;
            VolumeSlider.Value = AppSettings.Volume;
            
            if (AppSettings.WallpaperMode == "Web")
                WebRadioButton.IsChecked = true;
            else
                VideoRadioButton.IsChecked = true;
                
            UrlTextBox.Text = AppSettings.WallpaperPath;
            
            _isInitialized = true;
        }

        private void ChangeVideo_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Wallpaper Files (*.mp4;*.mkv;*.mov;*.html)|*.mp4;*.mkv;*.mov;*.html|All Files|*.*",
                Title = "Select a file for your wallpaper"
            };

            if (dialog.ShowDialog() == true)
            {
                AppSettings.WallpaperPath = dialog.FileName;
                UrlTextBox.Text = dialog.FileName;
                
                if (dialog.FileName.EndsWith(".html") || dialog.FileName.EndsWith(".htm"))
                    WebRadioButton.IsChecked = true;
                else
                    VideoRadioButton.IsChecked = true;
                
                _app.RefreshWallpapers();
            }
        }
        
        private void SetWebUrl_Click(object sender, RoutedEventArgs e)
        {
             if (string.IsNullOrWhiteSpace(UrlTextBox.Text)) return;
             AppSettings.WallpaperPath = UrlTextBox.Text;
             WebRadioButton.IsChecked = true;
             _app.RefreshWallpapers();
        }

        private void WebMode_Checked(object sender, RoutedEventArgs e)
        {
             if (!_isInitialized) return;
             AppSettings.WallpaperMode = "Web";
             _app.RefreshWallpapers();
        }

        private void VideoMode_Checked(object sender, RoutedEventArgs e)
        {
             if (!_isInitialized) return;
             AppSettings.WallpaperMode = "Video";
             _app.RefreshWallpapers();
        }

        private void AutoStartSwitch_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            AppSettings.AutoStart = true;
        }

        private void AutoStartSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            AppSettings.AutoStart = false;
        }

        private void MuteSwitch_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            AppSettings.IsMuted = true;
            _app.RefreshWallpapers();
        }

        private void MuteSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            AppSettings.IsMuted = false;
            _app.RefreshWallpapers();
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isInitialized) return;
            AppSettings.Volume = (int)e.NewValue;
            _app.RefreshWallpapers();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
