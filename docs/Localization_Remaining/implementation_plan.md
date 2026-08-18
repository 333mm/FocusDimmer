# Implementation Plan: Localize Remaining Preset Strings

## Goal

Localize the "PRESETS" section header and the tooltips/labels for the Add, Delete, and Process Rule buttons in all supported languages.

## Strings to Localize (Missing in zh, de, es, pt, fr)

- `HeaderPreset`
- `TooltipAddPreset`
- `TooltipDeletePreset`
- `LabelProcessSwitch`
- `BtnAddProcessRule`
- `DefaultPresetName`
- `NewPresetName`
- `MsgConfirmDeletePreset`
- `MsgEnterPresetName`

## Proposed Changes

### 1. JSON Files

- **[Update] `zh.json`, `de.json`, `es.json`, `pt.json`, `fr.json`**:
  - Add translations for the keys listed above.

### 2. UI Updates (`MainWindow.xaml`)

- **[Verify/Modify]**: Ensure the "PRESETS" header and buttons like "Add" are using `{Binding Strings.HeaderPreset}` etc. (Based on previous review, `HeaderPreset` might be hardcoded or missing binding).

## Verification

- **Manual**: Switch languages and observe the Preset section header and button tooltips.
