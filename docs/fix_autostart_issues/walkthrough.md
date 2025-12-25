# Auto-Start Issues Fix Walkthrough

## Changes Made

### 1. Enable Background Execution

- **File**: `App.xaml`
- **Change**: Set `ShutdownMode="OnExplicitShutdown"`.
- **Reason**: To allow the application to keep running (in system tray) even when the main window is closed or never shown.

### 2. Fix Startup Logic

- **File**: `App.xaml.cs`
- **Change**: In `OnStartup`, if `IsAutoStart` is detected:
  - Skipped calling `w.Show()`.
  - Removed `WindowState = Minimized` and `ShowInTaskbar = false` assignment. This prevents the window from entering a "parked" state that resulted in a minimal title-bar-only size.
  - Added notification prompt.
- **Result**: App starts invisibly, initializing logic in background.

### 3. Clean Shutdown

- **File**: `MainWindow.xaml.cs`
- **Change**: Added `Application.Current.Shutdown()` to `OnClosing` logic.
- **Reason**: Since `ShutdownMode` is explicit, closing the window (via Tray Exit) wouldn't terminate the process process without this call.

### 4. Prevent Settings Corruption

- **File**: `MainWindow.xaml.cs`
- **Change**: Modified `SaveSettingsActual` to check if `ActualWidth > 0`.
- **Reason**: If the app runs in background and exits without ever showing the window, `ActualWidth` is 0. Saving this would cause the next launch to have a 0x0 window. The fix preserves previous settings if the window wasn't visible.

## Verification

- **Build Success**: The project compiles successfully.
- **Behavior Check**:
  - **Auto-Start**: App should start silently to tray.
  - **Window Size**: When opened from tray, it should restore to last known `WindowWidth`/`Height` instead of 0/minimal.
  - **Exit**: "Exit" from tray menu should correctly terminate the process.
