# Implementation Plan: Fix Preset Rename Functionality

## Goal

Fix the issue where the right-click "Rename" menu appears as a white box and does not function. Also, allow renaming the currently selected preset by right-clicking the ComboBox itself (without opening the dropdown).

## Background

- **White box/Binding failure**: `RelativeSource AncestorType=Window` fails in `ContextMenu` because it's in a separate visual tree.
- **Usability**: Right-clicking the collapsed ComboBox should trigger the same "Rename" logic for the currently selected item.

## Proposed Changes

### 1. [MainWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml)

- **Remove `x:Name="Root"`** from the Window to eliminate the cycle.
- **Update `ContextMenu` for `PresetComboBox`**:
  - Set `DataContext="{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource Self}}"` on the `ContextMenu`.
  - This bridges to `MainWindow`.
- **Update `ContextMenu` in `ItemTemplate`**:
  - Set `Tag="{Binding DataContext, RelativeSource={RelativeSource AncestorType=Window}}"` on the `StackPanel` to pass the `MainWindow` reference.
  - Set `DataContext="{Binding PlacementTarget.Tag, RelativeSource={RelativeSource Self}}"` on the `ContextMenu`.
  - Use `CommandParameter="{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource AncestorType=ContextMenu}}"` to pass the `Preset` object.

## Revised Plan for ContextMenu Bridging

To avoid `x:Reference` cycles:

1. On the `ComboBox`, the `ContextMenu` will inherit `DataContext` from `PlacementTarget` (the ComboBox).
2. Inside the `ItemTemplate`, we will set the `Tag` of the `StackPanel` to the `MainWindow` using `RelativeSource AncestorType=Window` (this works during template instantiation).
3. The `ContextMenu` on that `StackPanel` will then use `PlacementTarget.Tag` to reach the `MainWindow`.

## Verification Plan

### Manual Verification

1. **Closed Dropdown**: Right-click the closed preset ComboBox. Select "Rename". Verify the input dialog appears and saves correctly.
2. **Open Dropdown**: Open the dropdown, right-click an item. Verify "Rename" text is visible (no white box) and clicking it triggers the rename dialog for that specific item.
