# Implement Fluent Design for Popup Windows

## Goal

Unify the design of all popup windows with the main window's Fluent/Acrylic aesthetic.

## Proposed Changes

### 1. Shared Resources (App.xaml)

- Move all resource dictionaries (Colors, Brushes, Styles) from `MainWindow.xaml` to `App.xaml` to make them globally available.
- Define a `FluentWindowStyle` that includes:
  - `WindowChrome` configuration.
  - A `ControlTemplate` implementing the custom TitleBar (Icon, Title, Minimize/Close buttons).
  - Binding of Caption Buttons to `SystemCommands`.

### 2. FluentWindow Base Class

- Create `FocusDimmer.Components.FluentWindow` inheriting from `Window`.
- **Responsibilities**:
  - Apply Acrylic/Mica backdrop using `WindowHelper.ApplySystemBackdrop`.
  - Bind `SystemCommands.CloseWindowCommand`, `MinimizeWindowCommand`, `MaximizeWindowCommand` to window actions.
  - Set default `Style` to `FluentWindowStyle`.

### 3. Window Refactoring

Update the following windows to inherit from `FluentWindow` and remove redundant styling/code:

- `MainWindow` (Refactor to use the shared base)
- `ProcessSelectionWindow`
- `InspectorActionDialog`
- `Views/DebugInspectorWindow`
- `Views/MigrationGuideWindow`
- `Views/ColorPickerWindow` (if XAML based)

## Verification Plan

### Manual Verification

- Launch the application and open each popup.
- Verify:
  - Acrylic/Mica effect is visible (translucency).
  - Title bar is custom (black/dark, not white system bar).
  - Minimize/Close buttons work.
  - Resizing works (if applicable).
  - Dark mode consistency.

### Automated Tests

- Build verification (`dotnet build`).
