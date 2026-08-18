# Implementation Plan: Localize New Features

## Goal

Localize the UI elements added in the recent "Default Preset", "Minimize Behavior", and "Rename Preset" updates.

## Strings to Localize

| Key | English | Japanese | Context |
| :--- | :--- | :--- | :--- |
| `LabelDefault` | (Default) | (既定) | Label next to the default preset name |
| `TooltipSetAsDefault` | Set as Default Preset | 既定のプリセットに設定 | Tooltip for the checkmark button |
| `MenuRename` | Rename | 名前を変更 | Context menu item for renaming presets |

## Proposed Changes

### 1. JSON Files

- Update `en.json`, `ja.json`, `zh.json`, `de.json`, `es.json`, `pt.json`, `fr.json` in `FocusDimmer2\Languages\`.

### 2. Localization Service

- **[Modify] `FocusDimmer.Services.LocalizationService.cs`**:
  - Add properties: `LabelDefault`, `TooltipSetAsDefault`, `MenuRename`.
  - Update `LoadLanguage` to populate these properties.

### 3. UI Updates

- **[Modify] `MainWindow.xaml`**:
  - Replace hardcoded strings with binding to `Strings.LabelDefault`, `Strings.TooltipSetAsDefault`, `Strings.MenuRename`.

## Verification

- **Manual**: Switch languages and verify the new elements update correctly.
