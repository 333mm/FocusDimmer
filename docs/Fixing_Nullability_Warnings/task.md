# タスクリスト (Task List)

## Null許容警告の修正

- [x] **CS8618 (未初期化フィールド)** の修正
  - [x] `MonitorProfile` (MonitorName, DeviceName, ScreenRef)
  - [x] `ProcessSelectionWindow` (SelectedProcessName, _allProcesses,_anchorButton)
  - [x] `DebugInspector` (_window, events)
  - [x] `WindowData` (ProcessName, Title, etc.)
  - [x] `MainWindow` (AppVersion, _notifyIcon, timers, etc.)
  - [x] `ColorPickerWindow` (_hexString) :new:
  - [x] `InspectorActionDialog` (ActionType) :new:
- [x] **CS8625 (nullリテラル変換)** の修正
  - [x] `MonitorTimer_Tick` 呼び出し時の引数修正 (`null` -> `EventArgs.Empty`)
  - [x] `NotifyPropertyChanged` の引数型修正 (`string` -> `string?`) :new:
  - [x] `NativeMethods.FindWindow` のシグネチャ修正
- [x] **CS8602 (null参照の可能性)** の修正
  - [x] `MainWindow` (`_monitorTimer`, `_saveTimer`)
  - [x] `DimmerOverlay` (`_window`, `_brush`, `ScreenRef` 等の広範なチェック) :new:
  - [x] `DebugInspector` (`_window`) :new:
  - [x] `StartupManager` (`MainModule`) :new:
- [x] **その他 (CS8600, CS8601, CS8603)** の修正
  - [x] `ProcessInfoHelper` (TryGetValue out param)
  - [x] `LocalizationService` (Indexer return type)
  - [x] `InspectorActionDialog` (Tag assignment)

## 検証

- [x] プロジェクト全体のビルド (警告0件を確認)
