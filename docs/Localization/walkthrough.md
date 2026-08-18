# Walkthrough - New Features Localization

## Completed Features

### 1. Localization Support

- **Keys Added**:
  - `LabelDefault`: Displays as `(Default)`, `(既定)`, etc. in the preset dropdown.
  - `TooltipSetAsDefault`: Displays tooltip for the "Set as Default" button.
  - `MenuRename`: Displays "Rename" / "名前を変更" in the context menu.
- **Languages Updated**:
  - English (`en.json`)
  - Japanese (`ja.json`)
  - Chinese (`zh.json`)
  - German (`de.json`)
  - Spanish (`es.json`)
  - Portuguese (`pt.json`)
  - French (`fr.json`)
- **Code**:
  - Updated `LocalizationService.cs` to handle the new keys.
  - Updated `MainWindow.xaml` to bind UI text to `LocalizationService` properties.

## Verification

- **Build**: Successfully built `FocusDimmer.csproj`.
- **Manual Verification Needed**:
    1. Launch app.
    2. Switch language to Japanese.
    3. Verify:
        - Preset dropdown shows `(既定)` for the default preset.
        - Hovering over `☑` button shows "既定のプリセットに設定".
        - Right-clicking a preset shows "名前を変更".
    4. Switch language to English and verify corresponding English strings.
