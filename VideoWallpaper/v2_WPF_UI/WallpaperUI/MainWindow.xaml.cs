using System;
using System.IO;
using System.Windows;
using Wpf.Ui.Controls;
using WallpaperCore;

namespace WallpaperUI
{
    public partial class MainWindow : FluentWindow
    {
        private bool _isInitialized = false;

        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);

            AutoStartSwitch.IsChecked = AppSettings.GetAutoStart();
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
                
                AppSettings.NotifySettingsChanged();
            }
        }
        
        private void SetWebUrl_Click(object sender, RoutedEventArgs e)
        {
             if (string.IsNullOrWhiteSpace(UrlTextBox.Text)) return;
             AppSettings.WallpaperPath = UrlTextBox.Text;
             WebRadioButton.IsChecked = true;
             AppSettings.NotifySettingsChanged();
        }

        private void WebMode_Checked(object sender, RoutedEventArgs e)
        {
             if (!_isInitialized) return;
             AppSettings.WallpaperMode = "Web";
             AppSettings.NotifySettingsChanged();
        }

        private void VideoMode_Checked(object sender, RoutedEventArgs e)
        {
             if (!_isInitialized) return;
             AppSettings.WallpaperMode = "Video";
             AppSettings.NotifySettingsChanged();
        }

        private void AutoStartSwitch_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            // The UI will set the engine's path as the target for auto-start. Engine executable should be nearby.
            string engineExe = Path.Combine(AppContext.BaseDirectory, "WallpaperEngine.exe");
            AppSettings.SetAutoStart(true, engineExe);
        }

        private void AutoStartSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            string engineExe = Path.Combine(AppContext.BaseDirectory, "WallpaperEngine.exe");
            AppSettings.SetAutoStart(false, engineExe);
        }

        private void MuteSwitch_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            AppSettings.IsMuted = true;
            AppSettings.NotifySettingsChanged();
        }

        private void MuteSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            AppSettings.IsMuted = false;
            AppSettings.NotifySettingsChanged();
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isInitialized) return;
            AppSettings.Volume = (int)e.NewValue;
            AppSettings.NotifySettingsChanged();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            // First stop the engine if it is running
            foreach (var proc in System.Diagnostics.Process.GetProcessesByName("WallpaperEngine"))
            {
                try { proc.Kill(); } catch { }
            }
            System.Windows.Application.Current.Shutdown();
        }
    }
}
