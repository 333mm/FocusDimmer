# Walkthrough - Default Preset & Minimize Improvements

## Completed Features

### 1. Default Preset

- **Functionality**: When the active window doesn't match any specific rule (e.g., Notepad, Chrome), the application now automatically switches to the designated "Default Preset".
- **UI**:
  - Added a "Set as Default" button (☑) next to the preset dropdown.
  - The default preset is marked with "(Default)" in the dropdown list.
  - The button updates the `DefaultPresetId` setting.
- **Persistence**: The default preset selection is saved in `settings.json` and restored on startup.

### 2. Minimize Behavior

- **Minimize to Taskbar**: Clicking the Minimize button (`_`) now minimizes the window to the Taskbar instead of hiding it to the Tray.
- **Close to Tray**: Clicking the Close button (`X`) continues to hide the window to the system tray.

### 3. Smooth Transitions

- **Animation**: When switching presets (e.g., active window changes or manual selection), the dimmer opacity now transitions smoothly.
  - **Darkening**: Uses the "Fade In" duration setting.
  - **Brightening**: Uses the "Fade Out" duration setting.
  - **Minimum Duration**: Enforced a minimum of 0.2s for visibility even if settings are faster.

## Verification

- **Build**: Successfully built `FocusDimmer.csproj`.
- **Manual Verification Needed**:
  1. Select a preset and click the "Set as Default" button (☑).
  2. Verify "(Default)" appears in the dropdown.
  3. Switch to a different app (unrelated to any rule) and verify the Default Preset is applied.
  4. Minimize the window and check it appears in the Taskbar.
  5. Close the window and check it disappears from Taskbar (to Tray).
