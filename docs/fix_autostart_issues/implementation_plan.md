# Auto-Start Issues Fix Implementation Plan

## Goal Description

Fix issues where the application starts with a minimal window size (title bar only) and does not start in the system tray when auto-started. Also investigate and improve slow startup time.

## User Review Required

None at this stage.

## Proposed Changes

### Startup Logic

- **App.xaml**:
  - Set `ShutdownMode="OnExplicitShutdown"` to allow the application to run without any visible windows (just tray icon and overlays).
- **App.xaml.cs**:
  - In `OnStartup`:
    - If `IsAutoStart` is true:
      - Do NOT call `w.Show()`.
      - Do NOT set `WindowState` to Minimized or `ShowInTaskbar` to false (avoiding the "parked window" artifact).
      - Call `w.ShowStartupNotification()`.
    - Else (Manual start):
      - Call `w.Show()`.

### Window Sizing

- **MainWindow.xaml.cs**:
  - The "minimal window size" issue is caused by the `WindowState.Minimized` + `ShowInTaskbar=false` combination in `App.xaml.cs`. Removing this code fixes it because the window will simply not be shown until requested.
  - When restored from Tray (DoubleClick or Menu), `SetupTrayIcon` already handles `Show()`, `WindowState=Normal` and `ShowInTaskbar=true`.
  - **Cleanup**: Ensure `Application.Current.Shutdown()` is called in `OnClosing` when `_reallyExit` is true, to support `OnExplicitShutdown` mode.

### Auto-Start Registration

- **Services/StartupManager.cs**:
  - Existing logic uses `/autostart`. Correct.
  - Current "Slow Startup" is likely due to standard Windows startup delays. Starting hidden will improve perceived performance as it won't block the user.

## Verification Plan

### Manual Verification

- Build and run.
- Enable auto-start in settings.
- Restart PC (or simulate by killing process and running the command from Registry).
- Verify app starts in tray (no window).
- Open window from tray and verify size.
