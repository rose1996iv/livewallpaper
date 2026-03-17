# WallpaperEngine

`WallpaperEngine` is a modular .NET 8 WPF desktop wallpaper engine for Windows 10 and Windows 11. It renders MP4 and WEBM video wallpapers behind desktop icons by attaching borderless playback windows to the Explorer `WorkerW` desktop layer.

## Architecture

The solution follows a clean, layered structure:

- `src/WallpaperEngine.Core`
  Contains domain models and interfaces for settings, wallpaper metadata, monitor discovery, playback, startup registration, tray integration, and fullscreen detection.
- `src/WallpaperEngine.Application`
  Contains orchestrating services such as `WallpaperManager`, `MonitorManager`, `PlaybackController`, `SettingsService`, `FullscreenDetector`, and `DesktopRecoveryService`.
- `src/WallpaperEngine.Infrastructure`
  Contains Windows-specific implementations for WorkerW desktop integration, MediaElement playback, Win32 fullscreen detection, registry startup, JSON storage, system tray integration, monitor enumeration, and file logging.
- `src/WallpaperEngine.UI`
  Contains the WPF presentation layer, DI/bootstrap logic, settings window, wallpaper library window, commands, and view models.
- `tests/WallpaperEngine.Tests`
  Contains basic unit tests for application services.

## Project Structure

```text
WallpaperEngine/
├── src/
│   ├── WallpaperEngine.Core/
│   ├── WallpaperEngine.Application/
│   ├── WallpaperEngine.Infrastructure/
│   └── WallpaperEngine.UI/
├── tests/
│   └── WallpaperEngine.Tests/
├── assets/
│   └── wallpapers/
├── config/
│   ├── settings.json
│   └── wallpaper-library.json
├── docs/
└── README.md
```

## How WorkerW Works

Explorer owns the desktop through the `Progman` window and a set of hidden worker windows. The app sends the undocumented `0x052C` message to `Progman` using `SendMessageTimeout`, which causes Explorer to ensure a hidden `WorkerW` host exists behind the `SHELLDLL_DefView` layer that owns desktop icons.

The engine then:

1. Finds `Progman` with `FindWindow`.
2. Forces the WorkerW layer to exist with `SendMessageTimeout`.
3. Enumerates top-level windows with `EnumWindows` and looks for the `WorkerW` sibling that sits behind the desktop icon layer.
4. Parents each wallpaper playback window into that WorkerW with `SetParent`.

That is what keeps the wallpaper visible on the desktop without appearing as a normal app window.

## Features

- MP4 and WEBM playback with `MediaElement`
- Looping playback
- Per-monitor or span wallpaper layout
- Alt+Tab suppression
- Fullscreen pause/resume background service
- Explorer restart recovery background service
- System tray controller
- JSON-based wallpaper library and settings
- Structured file logging to `logs/app.log`
- Startup with Windows via registry
- Playback speed, volume, stretch, mute, and aspect-ratio options

## How To Add Wallpapers

1. Run the app.
2. Open `Wallpaper Library`.
3. Click `Add Wallpaper`.
4. Choose an `.mp4` or `.webm` file.
5. Save settings to apply it to one or more monitors.

Imported media is copied into `assets/wallpapers/`, and metadata is stored in `config/wallpaper-library.json`.

## Build And Run

From the repository root:

```powershell
dotnet new sln -n WallpaperEngine
dotnet new wpf -n WallpaperEngine.UI -o src/WallpaperEngine.UI
dotnet new classlib -n WallpaperEngine.Core -o src/WallpaperEngine.Core
dotnet new classlib -n WallpaperEngine.Application -o src/WallpaperEngine.Application
dotnet new classlib -n WallpaperEngine.Infrastructure -o src/WallpaperEngine.Infrastructure
dotnet add src/WallpaperEngine.Application/WallpaperEngine.Application.csproj reference src/WallpaperEngine.Core/WallpaperEngine.Core.csproj
dotnet add src/WallpaperEngine.Infrastructure/WallpaperEngine.Infrastructure.csproj reference src/WallpaperEngine.Application/WallpaperEngine.Application.csproj src/WallpaperEngine.Core/WallpaperEngine.Core.csproj
dotnet add src/WallpaperEngine.UI/WallpaperEngine.UI.csproj reference src/WallpaperEngine.Application/WallpaperEngine.Application.csproj src/WallpaperEngine.Infrastructure/WallpaperEngine.Infrastructure.csproj src/WallpaperEngine.Core/WallpaperEngine.Core.csproj
dotnet build WallpaperEngine.sln
dotnet run --project src/WallpaperEngine.UI/WallpaperEngine.UI.csproj
```

The scaffolding commands above document how the solution was created. To run the generated project in this repository today, use the last two commands.

## Publish And Installer

To create a redistributable publish folder:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Publish-Release.ps1
```

To build the installer when Inno Setup is installed and `iscc` is available on `PATH`:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Build-Installer.ps1
```

Installer assets live in:

- `installer/WallpaperEngine.iss`
- `dist/publish/win-x64`
- `dist/installer`

## VS Code

1. Open the `WallpaperEngine` folder in VS Code.
2. Install the C# extension if needed.
3. Use the integrated terminal.
4. Run:

```powershell
dotnet build WallpaperEngine.sln
dotnet run --project src/WallpaperEngine.UI/WallpaperEngine.UI.csproj
```

## End-User Behavior

- On first run, the settings window opens if no playable wallpaper session is configured.
- After `Save, Apply, Hide`, the app hides to the system tray and the wallpaper continues running behind desktop icons.
- On later launches, the app starts in the tray when a valid wallpaper session already exists.
- Use tray menu `Open Settings` or `Wallpaper Library` to reopen the UI.

## Logging

Application logs are written to:

```text
logs/app.log
```

## Notes

- `MediaElement` is used as the initial playback engine behind the `IVideoPlaybackEngine` abstraction so future FFmpeg or DirectShow integration can be added without rewriting the application layer.
- Hardware acceleration is handled through the WPF rendering mode toggle where possible; decode behavior still depends on the installed Windows media pipeline and codecs.
