# 🎬 LiveWallpaper Screensaver Utility

A built-in Windows Video Screensaver (`.scr`) companion app for **LiveWallpaper (V3)**. This screensaver natively triggers your currently selected wallpaper (whether Video, HTML, or 3D Parallax) whenever your PC goes idle.

<p align="center">
  <img src="https://img.shields.io/badge/Platform-Windows-blue?style=flat-square&logo=windows" />
  <img src="https://img.shields.io/badge/.NET-8.0-purple?style=flat-square&logo=.net" />
  <img src="https://img.shields.io/badge/UI-WPF-blueviolet?style=flat-square" />
</p>

## ✨ Features
*   **Zero-Overhead Forwarding**: Unlike older versions, this screensaver does not decode videos on its own. It acts as a lightweight forwarder and commands the main `LiveWallpaper.Engine.exe` to run in native Screensaver Mode.
*   **Web & 3D Parallax Support**: Because it forwards to the main engine, HTML wallpapers, WebGL, and ML Depth Map parallax wallpapers work flawlessly over your lock-screen.
*   **Native Windows Support**: Responds to standard Windows Screensaver arguments (`/s` for full screen, `/p` for preview box, `/c` for settings config).
*   **Auto Exit**: The engine smoothly closes upon any mouse movement, mouse click, or keyboard press directly over the screensaver context.

---

## 📥 Installation & Usage

### Installing the Pre-built `.scr`
If you published the application via the CLI:
1. Locate your published `dist/LiveWallpaper` directory.
2. Find `LiveWallpaper.Utility.Screensaver.scr` (Ensure you renamed the `.exe` to `.scr`).
3. **Right-click** on `LiveWallpaper.Utility.Screensaver.scr` and select **Install**.
4. Windows will open the **Screen Saver Settings** window.
5. Ensure `LiveWallpaper.Utility.Screensaver` is selected in the dropdown.
6. Click **Apply** and **OK**.

> **💡 Note:** To configure the video playing in the screensaver, just open your main **LiveWallpaper.UI.exe** and select a new wallpaper. The screensaver uses the same engine, so changes apply instantly!

---

## 🛠️ Building from Source

To build the screensaver manually from the terminal:

```ps1
# Build the UI, Engine, and Screensaver into a centralized 'dist' folder
dotnet publish LiveWallpaper.Utility.Screensaver\LiveWallpaper.Utility.Screensaver.csproj -c Release -o dist\LiveWallpaper
```

> **📌 Important Output Step**: The compiler outputs an `.exe` file. For Windows to recognize it as a screensaver perfectly, you must rename the published file from `LiveWallpaper.Utility.Screensaver.exe` to `LiveWallpaper.Utility.Screensaver.scr`. 

---

## 🧩 How it Works (Under the Hood)
Windows screensavers are just standard `.exe` files renamed to `.scr`. Windows triggers screensavers by passing specific command-line arguments:
*   `/s` - **Start**: The application intercepts this to launch the `LiveWallpaper.Engine.exe --screensaver` command. The engine then sets its window layout to `Topmost` over all bounds.
*   `/p <HWND>` - **Preview**: The application uses native user32.dll `SetParent` via WPF's `HwndSource` to cleanly render a placeholder preview into the native mini-preview box.
*   `/c` - **Configure**: The application catches this to simply launch the main `LiveWallpaper.UI.exe` so you can change your video settings.

## 📜 License
This component is part of the LiveWallpaper project and is released under the MIT License.
