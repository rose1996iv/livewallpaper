<div align="center">
  <img src="banner.png" alt="Live Wallpaper Banner" width="600">
  
  <h1>✨ LiveWallpaper</h1>
  <p>A performant, modern Windows dynamic/video wallpaper application built with WPF, .NET 8, and WinUI 3 styling.</p>

  [![Download Latest Release](https://img.shields.io/badge/Download-Latest_Release-blue?style=for-the-badge&logo=windows)](https://github.com/rose1996iv/livewallpaper/releases/latest)
</div>

## 📥 Download & Installation

1. Click the download badge above or go to the [**Releases Page**](https://github.com/rose1996iv/livewallpaper/releases/latest).
2. Download the `LiveWallpaper_Standalone.zip` file.
3. Extract the `.zip` file into a folder of your choice.
4. Run `WallpaperUI.exe` to launch the application directly.

> **Note:** The standalone version comes fully packed with all dependencies included. You do not need to install any extra frameworks (like .NET) to run it!

## 🚀 Features
- **Video & Web Wallpapers**: Set any local `.mp4`, `.mkv`, `.mov`, or `.html` web pages as your interactive wallpaper.
- **Video Screensaver**: Includes a built-in screensaver (`.scr`) that plays your video wallpaper when your PC is idle.
- **Fluent Design Elements**: A modern and sleek user interface following the latest Windows 11 aesthetics.
- **Optimized & Lightweight**: Utilizes native Windows API tricks to render wallpapers beneath desktop icons smoothly via a separate background engine process.
- **Audio Controls**: Tweak the wallpaper volume or mute it entirely.
- **Auto-Start**: Set it to run seamlessly when Windows boots up.

## 🛠️ Building from Source

**Requirements:**
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Visual Studio 2022 or VS Code

To compile the standalone single-file executables via the terminal:

```ps1
# 1. Build the UI application
dotnet publish VideoWallpaper/v2_WPF_UI/WallpaperUI/WallpaperUI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist/ReadyToUse

# 2. Build the Wallpaper Engine
dotnet publish VideoWallpaper/v2_WPF_UI/WallpaperEngine/WallpaperEngine.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist/ReadyToUse

# 3. Build the Video Screensaver
dotnet publish VideoWallpaper/v2_WPF_UI/WallpaperScreensaver/WallpaperScreensaver.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist/ReadyToUse
# (Then rename WallpaperScreensaver.exe to WallpaperScreensaver.scr in the output folder to use it as a screensaver)
```

## 📜 License
Released under the MIT License.
