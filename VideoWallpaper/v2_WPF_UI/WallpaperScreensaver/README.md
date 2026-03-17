# 🎬 LiveWallpaper Screensaver

A built-in Windows Video Screensaver (`.scr`) companion app for **LiveWallpaper**. This screensaver plays your currently selected video wallpaper with your configured settings whenever your PC goes idle.

<p align="center">
  <img src="https://img.shields.io/badge/Platform-Windows-blue?style=flat-square&logo=windows" />
  <img src="https://img.shields.io/badge/.NET-8.0-purple?style=flat-square&logo=.net" />
  <img src="https://img.shields.io/badge/UI-WPF-blueviolet?style=flat-square" />
</p>

## ✨ Features
*   **Video Playback**: Automatically plays your current `.mp4` video wallpaper when the PC is locked or idle.
*   **Multi-Monitor Support**: Spans the video playback across all connected displays.
*   **Seamless Integration**: Share the same volume, mute, and video source settings as your main LiveWallpaper app via the Windows Registry.
*   **Native Windows Support**: Responds to standard Windows Screensaver arguments (`/s` for full screen, `/p` for preview box, `/c` for settings config).
*   **Auto Exit**: Exits smoothly upon any mouse movement, mouse click, or keyboard press.

---

## 📥 Installation & Usage

### Method 1: Installing the Pre-built `.scr`
If you downloaded the official release or built the project:
1. Locate the `WallpaperScreensaver.scr` file.
2. **Right-click** on `WallpaperScreensaver.scr` and select **Install**.
3. Windows will open the **Screen Saver Settings** window.
4. Ensure `WallpaperScreensaver` is selected in the dropdown.
5. Set your preferred idle 'Wait' time.
6. Click **Apply** and **OK**.

### Method 2: Manual Installation (System32)
1. Copy the `WallpaperScreensaver.scr` file.
2. Paste it into your `C:\Windows\System32` directory.
   *(You will need Administrator privileges for this step)*.
3. Open Windows Start menu, search for **"Change screen saver"**.
4. Select `WallpaperScreensaver` from the dropdown list.

> **💡 Note:** To configure the video playing in the screensaver, just open your main **LiveWallpaper** UI app and select a new video. The screensaver will automatically use the same video and volume settings!

---

## 🛠️ Building from Source

To build the screensaver manually from the terminal:

```ps1
# Navigate to the project directory
cd VideoWallpaper/v2_WPF_UI/WallpaperScreensaver

# Build it as a standalone, single-file executable to be dropping-ready
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist/ReadyToUse
```

> **📌 Important Output Step**: When compiling manually or using `dotnet run`, the compiler outputs an `.exe` file. For Windows to recognize it as a screensaver perfectly, you must rename the published file from `WallpaperScreensaver.exe` to `WallpaperScreensaver.scr`. 
*(Note: A post-build script is included in the `.csproj` to automatically create a `.scr` copy in your local `bin` folder on regular debug builds).*

---

## 🧩 How it Works (Under the Hood)
Windows screensavers are just standard `.exe` files renamed to `.scr`. Windows triggers screensavers by passing specific command-line arguments:
*   `/s` - **Start**: The application intercepts this to launch the `MainWindow` spanning all bounds across all screens (`System.Windows.Forms.Screen.AllScreens`).
*   `/p <HWND>` - **Preview**: Windows passes a window handle (`<HWND>`). The application uses native user32.dll `SetParent` via WPF's `HwndSource` to cleanly render the `ScreensaverControl.xaml` into the native mini-preview box.
*   `/c` - **Configure**: The application catches this to simply launch the main `WallpaperUI.exe` so you can change your video settings.

## 📜 License
This component is part of the LiveWallpaper project and is released under the MIT License.
