# Implementation Plan: Default Preset & Minimize Behavior

## Goal

1. Allow users to set a "Default Preset" which is automatically applied when no specific process rules match the active window.
2. Change minimize behavior: Minimize to Taskbar, Close to Tray.
3. Use smooth animations when switching presets (differential update of opacity).

## User Review Required

- N/A

## Proposed Changes

### 1. Default Preset Logic

- **[Modify] `FocusDimmer.Models.AppSettings`**: Add `DefaultPresetId` property.
- **[Modify] `FocusDimmer.MainWindow.xaml.cs`**:
  - Add `DefaultPresetId` property to ViewModel (sync with AppSettings).
  - Add logic in `ActiveProcessCheckTimer_Tick`:
    - If matching rule found -> switch to it.
    - If NO matching rule found -> switch to `DefaultPresetId` (if set and different from current).
  - Add `SetDefaultPreset_Click` handler.

### 2. UI Updates (`MainWindow.xaml`)

- **Preset List**:
  - Update `ItemTemplate` to show "(Default)" text next to the preset name if it matches `DefaultPresetId`.
- **Default Button**:
  - Add a button NEXT to the preset dropdown.
  - Icon: ☑ (Checkbox).
  - Tooltip: "Set as Default Preset" / "既定のプリセットに設定".
  - Logic: Clicking it sets the currently selected preset as the default. Visual state should reflect if the current preset is already default (e.g., filled/colored if default, outline if not, or disabled if already default).

### 3. Minimize Behavior (`MainWindow.xaml.cs`)

- **[Modify] `MinimizeButton_Click`**: Change from `this.Hide()` to `this.WindowState = WindowState.Minimized;`.
- **[Modify] `OnStateChanged`**: Ensure restoring from minimized state works correctly if needed (standard WPF usually handles this).

### 4. Smooth Transition

- **[Modify] `FocusDimmer.Components.DimmerOverlay.cs`** (or relevant overlay class):
  - Identify where `Opacity` is updated.
  - Instead of direct assignment, use `DoubleAnimation` to transition the `Opacity` property over a short duration (e.g., 200-300ms).
- **[Modify] `FocusDimmer.Models.Preset`**:
  - Ensure `ApplyToProfile` applies changes in a way that triggers valid property changes for the overlay to animate.

## Verification

- **Automated**: Build clean (0 errors/warnings).
- **Manual**:
  - Set "Preset A" as default.
  - Open app matched to "Preset B" -> Switches to B.
  - Open unrelated app -> Switches to A (Default).
  - Minimize -> Taskbar.
  - Close -> Tray.
  - Verify smooth fading when switching.
