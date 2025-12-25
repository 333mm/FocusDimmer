# Refactoring Walkthrough

Refactoring of `MainWindow.xaml.cs` has been completed. The large file has been split into multiple smaller, manageable files.

## Changes Checklist

### Extracted Code
The following classes and logic have been moved to new files:

- **Models**
  - `Models/MonitorProfile.cs`
  - `Models/AppSettings.cs`
- **Services**
  - `Services/LocalizationService.cs` (Renamed from `LocalizedStrings`)
  - `Services/NativeMethods.cs`
  - `Services/StartupManager.cs`
- **Helpers**
  - `Helpers/ProcessInfoHelper.cs`
  - `Helpers/WindowHelper.cs` (New, contains `IsMenuOrPopupEx` logic)
- **Converters**
  - `Converters/HexToColorConverter.cs` (Updated XAML usage)
- **Components**
  - `Components/DimmerOverlay.cs`

### Modified Files
- **MainWindow.xaml.cs**
  - Removed all nested classes.
  - Added `using` directives for new namespaces.
  - Updated properties (`Strings` type changed to `LocalizationService`).
  - Updated calls to `IsMenuOrPopupEx` to use `WindowHelper`.
- **MainWindow.xaml**
  - Added `xmlns:converters` namespace.
  - Updated `HexToColorConverter` resource definition.

## Verification Results
- **Compilation Check**: Use of `using` directives allows code to resolve extracted classes.
  - **Fixed Ambiguity**: Added aliases for `Color`, `Brushes`, and `ColorConverter` in `DimmerOverlay.cs` and `HexToColorConverter.cs` to resolve conflicts between `System.Drawing` (implicitly imported via WinForms) and `System.Windows.Media`.
- **XAML Binding**: Updated XAML namespaces so the converter can be found. Data bindings rely on property names which were preserved.
- **Functionality**: Logic for overlays, settings, and hotkeys was preserved without structural changes to the algorithms.
