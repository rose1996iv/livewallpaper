# LiveWallpaper (V3 Lively Architecture)

LiveWallpaper is a lightweight Windows desktop application built with C# and WPF (.NET 8). It allows you to set any video file or interactive web page (HTML/URL) as your live desktop background, rendering smoothly behind your desktop icons.

## Features

- **Lively-Inspired Modular Architecture:** Split into multiple decoupled class libraries (`LiveWallpaper.Common`, `LiveWallpaper.ML`, `LiveWallpaper.Engine`, `LiveWallpaper.UI`), mirroring the exact robust architecture used by Lively Wallpaper.
- **Lively.ML (3D Depth Effect):** Now supports calculating 3D depth maps from standard 2D images using `MiDaS` ONNX models and `Magick.NET`. The result combines with WebGL for a responsive 3D parallax effect!
- **Native Screensaver Utility:** Includes a standalone lightweight `.scr` screensaver that natively triggers the wallpaper engine whenever your PC goes idle.
- **Web & Video Support:** Local video files (MP4, MKV), local HTML interactive pages, or live web URLs.
- **Multi-Monitor Support:** Automatically detects all connected displays and spans/tiles the live wallpapers across each screen flawlessly.
- **System Resource Management:** Intelligently pauses wallpaper playback when running games or movies in full-screen mode, or when running on battery.
- **Interactive Web Wallpapers:** Forwards mouse inputs to HTML/Shadertoy web pages, allowing the desktop to react to your mouse movements.

---

## 🏗️ Architecture (V3)

The application employs an advanced modular architecture to minimize performance overhead:

1. **LiveWallpaper.Engine (The Core Player):**
   - Headless background process that manages `LibVLC` and `WebView2` players.
   - Utilizes low-level **Win32 P/Invoke APIs** to spawn a hidden `WorkerW` window sandwiched exactly between the desktop wallpaper and the desktop icons.

2. **LiveWallpaper.UI (The Fluent Dashboard):**
   - Built exclusively with `WPF-UI` to provide native Windows 11 Fluent Design and dark modes.

3. **LiveWallpaper.Common (Shared Infrastructure):**
   - Connects the Engine, the UI, and the Screensaver via Windows Registry (`SOFTWARE\LiveWallpaperV3`) and shared utilities.

4. **LiveWallpaper.ML (Machine Learning Core):**
   - Separated ONNX inference module to keep the Engine lightweight while providing 3D depth map generation via `MagickImage`.

5. **LiveWallpaper.Utility.Screensaver:**
   - A tiny forwarder that responds to Windows `/s`, `/p`, `/c` screensaver commands, starting the engine natively over your lock screens without creating duplicate playback memory footprints.

---

## 🚀 Installation & Execution

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Windows 10 or 11 OS

### Run from Source
1. Clone the repository:
   ```bash
   git clone https://github.com/rose1996iv/livewallpaper.git
   cd livewallpaper/VideoWallpaper/v3_Lively_Architecture
   ```

2. Run the Engine:
   ```bash
   dotnet run --project LiveWallpaper.Engine
   ```
3. Run the UI:
   ```bash
   dotnet run --project LiveWallpaper.UI
   ```

### Building a Standalone Application
You can compile everything into ready-to-use, robust binaries:

```bash
dotnet publish LiveWallpaper.Engine\LiveWallpaper.Engine.csproj -c Release -o dist\LiveWallpaper
dotnet publish LiveWallpaper.UI\LiveWallpaper.UI.csproj -c Release -o dist\LiveWallpaper
dotnet publish LiveWallpaper.Utility.Screensaver\LiveWallpaper.Utility.Screensaver.csproj -c Release -o dist\LiveWallpaper
```
Once published, you can rename `LiveWallpaper.Utility.Screensaver.exe` to `.scr` and right-click "Install" to integrate with Windows!

---

## Technical Dependencies

- `Microsoft.ML.OnnxRuntime` and `Magick.NET` (For 3D Parallax generation)
- `WPF-UI` (For the Fluent Dashboard)
- `LibVLCSharp.WPF` (For gapless video decoding)
- `Microsoft.Web.WebView2` (For rendering HTML and URLs as wallpapers)
