# Refactoring Plan: MainWindow.xaml.cs cleanup

The goal is to dismantle the "God Class" `MainWindow.xaml.cs` by extracting distinct responsibilities into their own files and folders. This will improve readability, maintainability, and testability.

## User Review Required
> [!NOTE]
> This refactoring mainly involves moving code to new files. No functional changes are intended.

## Proposed Changes

### 1. Folder Structure Creation
Create the following directories in the project root:
- `Models`
- `Services`
- `Helpers`
- `Converters`
- `Components`

### 2. Extract Classes

#### Models
- **Extract `MonitorProfile`** to `Models/MonitorProfile.cs`
- **Extract `AppSettings`** to `Models/AppSettings.cs`

#### Services / Interop
- **Extract `NativeMethods`** to `Services/NativeMethods.cs`
- **Extract `StartupManager`** to `Services/StartupManager.cs`
- **Extract `LocalizedStrings`** to `Services/LocalizationService.cs` (and rename to `LocalizationService`)

#### Helpers
- **Extract `ProcessInfoHelper`** to `Helpers/ProcessInfoHelper.cs`

#### Converters
- **Extract `HexToColorConverter`** to `Converters/HexToColorConverter.cs`

#### Components
- **Extract `DimmerOverlay`** to `Components/DimmerOverlay.cs`

### 3. Cleanup MainWindow.xaml.cs
- Remove the extracted classes.
- Add necessary `using` directives.
- Verify that everything compiles and runs as before.

## Verification Plan

### Automated Tests
- Build the solution to ensure no compilation errors.
- Run the application and verify:
    - Overlay appears on monitors.
    - Settings are loaded/saved correctly.
    - Hotkeys work.
    - Language switching works.
    - Startup registry execution works.
