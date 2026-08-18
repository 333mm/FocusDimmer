# Implementation Plan - Preset Process Association Fix

## Goal

Fix the issue where preset-process association rules are defined but not applied, and add a visual indicator to show which processes are linked to a preset.

## User Review Required

None.

## Proposed Changes

### Logic Implementation (`MainWindow.xaml.cs`)

- Add a `DispatcherTimer` to monitor the active window every 500ms.
- Use `NativeMethods.GetForegroundWindow` and `GetWindowThreadProcessId` to identify the active process.
- Logic:
  - If the active process matches a rule in any Global Preset, automatically switch to that preset.
  - Implement a mechanism to avoid conflict with manual preset switching (e.g., only switch when the active process *changes*).

### UI Improvements (`MainWindow.xaml`, `Preset.cs`)

- **[Modify] `FocusDimmer.Models.Preset`**:
  - Add a read-only property `ProcessRulesDisplay` that returns a comma-separated string of associated process names (e.g., "chrome, notepad").
- **[Modify] `FocusDimmer.MainWindow.xaml`**:
  - In the `GlobalPresets` list (presumed to be a `ListBox` or similar), add a `TextBlock` binding to `ProcessRulesDisplay` to show associated processes next to the preset name.

## Verification Plan

### Automated Tests

- Run `dotnet build` to ensure no compilation errors.

### Manual Verification

1. Open the application.
2. Create two presets (e.g., "Work" and "Play").
3. Assign "notepad" to "Work" preset.
4. Open Notepad. The application should automatically switch to "Work" preset.
5. Verify that "notepad" is displayed next to "Work" in the preset list.
