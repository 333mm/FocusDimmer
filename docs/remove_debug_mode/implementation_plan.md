# Remove Debug Mode

## Goal

Remove the debug mode feature from the application, including the debug overlay, the UI toggle button, and associated localization strings.

## User Review Required
>
> [!IMPORTANT]
> The debug overlay feature will be completely removed. This includes the ability to inspect window properties under the mouse cursor.

## Proposed Changes

### FocusDimmer2

#### [MODIFY] [MainWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml)

- Remove `ToggleButton` named `DebugModeButton` and its associated UI logic.

#### [MODIFY] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)

- Remove `DebugMode_Click` event handler.
- Remove `_debugOverlay` field.
- Remove `DebugOverlay` class instantiation and usage.

#### [DELETE] [DebugOverlay.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Views/DebugOverlay.cs)

- Delete the `DebugOverlay` class file.

#### [MODIFY] [Languages/*.json]

- Remove the following keys from all language files (en.json, ja.json, etc.):
  - `BtnDebug`
  - `TooltipDebugMode`
  - `DebugNoWindow`
  - `DebugProcess`
  - `DebugClass`
  - `DebugTitle`
  - `DebugHoleAnalysis`
  - `DebugTaskbar`
  - `DebugTopmost`
  - `DebugMenu`
  - `DebugDialog`
  - `DebugToolWindow`
  - `DebugStandardWindow`
  - `DebugAddToDarkList`

## Verification Plan

### Automated Tests

- Build the solution to ensure no compilation errors due to missing `DebugOverlay` reference.

### Manual Verification

- Launch the application.
- Verify that the "Debug" button is no longer visible in the Exclusion Lists tab.
- Verify that the application runs normally without crashes.
