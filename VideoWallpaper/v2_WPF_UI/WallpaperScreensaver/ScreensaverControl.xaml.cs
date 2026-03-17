using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using WallpaperCore;

namespace WallpaperScreensaver
{
    public partial class ScreensaverControl : UserControl
    {
        public ScreensaverControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string videoPath = AppSettings.WallpaperPath;
                if (!string.IsNullOrEmpty(videoPath) && File.Exists(videoPath))
                {
                    player.Source = new Uri(videoPath, UriKind.Absolute);
                    player.Volume = AppSettings.IsMuted ? 0 : ((double)AppSettings.Volume / 100.0);
                    player.Play();
                }
            }
            catch { }
        }

        private void player_MediaEnded(object sender, RoutedEventArgs e)
        {
            player.Position = TimeSpan.Zero;
            player.Play();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            player.Stop();
        }
    }
}
