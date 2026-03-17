# Architecture Notes

The application is intentionally split so that Explorer/Win32 concerns stay in Infrastructure, orchestration stays in Application, and user-facing WPF concerns stay in UI.

## Layer Boundaries

- Core defines contracts and pure models.
- Application coordinates use cases and background services.
- Infrastructure implements contracts for Windows.
- UI bootstraps DI and presents configuration workflows.

## Runtime Flow

1. `WallpaperEngine.UI.App` builds the host and dependency graph.
2. `SettingsViewModel` loads settings and the local wallpaper library.
3. `PlaybackController` resolves monitor layout and wallpaper sessions.
4. `MediaElementVideoPlaybackEngine` creates a wallpaper window per session.
5. `WorkerWDesktopIntegrationService` reparents those windows behind desktop icons.
6. `FullscreenDetector` pauses playback during fullscreen apps.
7. `DesktopRecoveryService` periodically reattaches windows in case Explorer restarts.
