# Implementation Plan: Restore and Improve Rename Functionality

## Goal

Restore rename capability by implementing a context menu (Right-Click) on the preset dropdown items, as requested by the user. Also check and fix the missing rename button if possible.

## User Review Required

- N/A

## Proposed Changes

### 1. Rename Logic

- **[Modify] `FocusDimmer.MainWindow.xaml.cs`**:
  - Add `RenamePresetCommand` property.
  - Implement `RenamePreset(Preset preset)` method (refactoring existing `EditGlobalPresetName_Click` logic).
  - `RenamePresetCommand` will call `RenamePreset`.

### 2. UI Updates (`MainWindow.xaml`)

- **Preset ComboBox ItemTemplate**:
  - Add `ContextMenu` to the root `StackPanel` of the item template.
  - Add `MenuItem` Header="Rename" Command="{Binding DataContext.RenamePresetCommand, RelativeSource={RelativeSource AncestorType=Window}}" CommandParameter="{Binding}".
- **Edit Button**:
  - Verify its visibility. If redundant or broken, clean it up.

## Verification

- **Manual**:
  - Right-click a preset in the dropdown.
  - Select "Rename".
  - Verify input dialog appears and rename works.
  - Verify renaming the *active* preset updates the selection correctly.
