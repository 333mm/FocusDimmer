# Auto-Start Issues Fix

- [x] Investigate current startup logic <!-- id: 0 -->
  - [x] Check `App.xaml.cs` for startup event handling
  - [x] Check `MainWindow.xaml.cs` for window initialization and state restoration
  - [x] Check `AutoStartup` related classes
- [x] Fix Window Size/State on Startup <!-- id: 1 -->
  - [x] Ensure window size is correctly restored or set to default
  - [x] Prevent "title bar only" size (Fixed by removing problematic WindowState logic and safe-guarding settings save)
- [x] Fix Startup to Tray <!-- id: 2 -->
  - [x] Implement logic to start minimized to tray if auto-started
  - [x] Check for startup arguments (e.g., `-autostart`)
- [x] Investigate Slow Startup <!-- id: 3 -->
  - [x] Check mechanism used for auto-start (Registry vs Shortcut)
  - [x] Verify changes improve perceived startup speed (Hidden start)
- [x] Verification <!-- id: 4 -->
  - [x] Verify auto-start behavior (Build passed, logic reviewed)
  - [x] Verify window size logic (Code fix confirmed)
  - [x] Verify tray behavior logic (Code fix confirmed)
