# VideoWallpaper (Live Wallpaper V2)

VideoWallpaper is a lightweight Windows desktop application built with C# and WPF (.NET 8). It allows you to set any video file or interactive web page (HTML/URL) as your live desktop background, rendering smoothly behind your desktop icons.

## Features

- **2-Process Architecture:** Split into a lightweight background engine and a beautiful UI dashboard, mirroring advanced software like Lively Wallpaper.
- **Web & Video Support:** Set local video files (MP4, MKV), local HTML interactive pages, or live web URLs (like Shadertoy or YouTube) as your wallpaper using `LibVLCSharp` and `WebView2`.
- **Multi-Monitor Support:** Automatically detects all connected displays and spans/tiles the live wallpapers across each screen flawlessly.
- **System Resource Management:** Intelligently pauses wallpaper playback when running games or movies in full-screen mode, or when running on battery power to save CPU/GPU/RAM.
- **Smart Auto-Rotation:** Set a timer (e.g., every 15 minutes) to automatically rotate between your favorite wallpapers from a designated gallery.
- **Interactive Web Wallpapers:** Forwards mouse inputs to HTML/Shadertoy web pages, allowing the desktop to react to your mouse movements.
- **Taskbar Customization:** Effortlessly set the Windows Taskbar to be fully transparent or blurred for an immersive desktop experience.
- **Gallery Store:** Integrated built-in thumbnail UI to preview and browse recently used or downloaded video wallpapers in a local repository directory.
- **Fluent Dashboard:** A modern Win11-styled Control Panel featuring Mica effects, toggles for Auto-Start, Mute, Volume Control, and Wallpaper Types.
- **True Desktop Integration:** Renders directly to the Windows Desktop layer (behind desktop icons), completely hidden from the Alt+Tab menu and Windows Taskbar.

---

## 🏗️ Architecture (V2)

The application employs an advanced modular architecture to minimize performance overhead and ensure the UI never crashes the core engine.

1. **WallpaperEngine.exe (The Core Background Process):**
   - Headless background process that manages LibVLC and WebView2 players.
   - Utilizes low-level **Win32 P/Invoke APIs** to spawn a hidden `WorkerW` window sandwiched exactly between the desktop wallpaper and the desktop icons.
   - Hosts a responsive System Tray (Notification) icon to Play/Pause videos, open the Dashboard, or close the application.
   - Listens to system-wide `EventWaitHandle` signals to instantly update volumes, mutes, or wallpaper swaps in real-time.

2. **WallpaperUI.exe (The Fluent Dashboard):**
   - Built exclusively with `WPF-UI` to provide native Windows 11 Fluent Design, Dark Mode, and Mica Backdrop effects.
   - Allows users to configure persistent settings (Registry-backed `AppSettings`).
   - Signals the `WallpaperEngine` whenever the user applies new settings via named events, resulting in gapless and crash-free transitions.

3. **WallpaperCore.dll (Shared Library):**
   - Connects the Engine and the UI.
   - Houses the `AppSettings` class to interact with the Windows Registry (`SOFTWARE\VideoWallpaper`) to save states between reboots.

---

## 🚀 Project Pipeline / Setup

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Windows 10 or 11 OS
- Microsoft Edge WebView2 Runtime (Included in most modern Windows installs)

### Installation & Execution
1. Clone the repository:
   ```bash
   git clone https://github.com/rose1996iv/livewallpaper.git
   cd livewallpaper/VideoWallpaper/v2_WPF_UI
   ```

2. Run the application through the .NET CLI:
   ```bash
   dotnet run --project WallpaperUI/WallpaperUI.csproj
   ```
   *Opening the UI will automatically start the WallpaperEngine in the background.*

### Building a Standalone Application
You can compile both the Engine and the UI into ready-to-use, robust binaries:

```bash
dotnet publish WallpaperEngine/WallpaperEngine.csproj -c Release -p:PublishSingleFile=true --runtime win-x64 -o dist
dotnet publish WallpaperUI/WallpaperUI.csproj -c Release -p:PublishSingleFile=true --runtime win-x64 -o dist
```
All `.exe` files and required libraries will be placed in the `\dist\` folder. You can move this folder anywhere and run `WallpaperUI.exe`.

---

## Technical Dependencies

- `WPF-UI` (For the Fluent Dashboard)
- `LibVLCSharp.WPF` (For gapless video decoding)
- `VideoLAN.LibVLC.Windows` (Core VLC Libraries)
- `Microsoft.Web.WebView2` (For rendering HTML and URLs as wallpapers)
