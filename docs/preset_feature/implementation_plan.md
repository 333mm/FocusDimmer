# プリセット機能の実装計画

全モニタへ一括適用するグローバルプリセット機能と、特定プロセス実行時に自動切替機能を実装。

## 変更ファイル

### Models

- **[NEW] `Preset.cs`**: プリセット設定を保持するモデル。MonitorProfileとの相互変換メソッドを含む。
- **[NEW] `ProcessPresetRule.cs`**: プロセス名とプリセットIDの関連付けルール。
- **[MODIFY] `AppSettings.cs`**: `Presets` リストと `SelectedPresetId` を追加。
- **[MODIFY] `MonitorProfile.cs`**: 旧プリセット関連プロパティを削除。

### UI

- **[MODIFY] `MainWindow.xaml`**: フッターにプリセット管理UI（ComboBox、編集/追加/削除/プロセスルールボタン）を追加。

### Code-Behind

- **[MODIFY] `MainWindow.xaml.cs`**:
  - `GlobalPresets`, `SelectedGlobalPresetId` プロパティを追加
  - グローバルプリセット操作イベントハンドラを追加
  - `MonitorTimer_Tick`にプロセス自動切替ロジックを追加
  - `ApplyGlobalPresetToAllMonitors`で全モニタに一括適用

### Localization

- **[MODIFY] `LocalizationService.cs`**: プリセット関連の英語/日本語文字列を追加。

## 検証結果

- ビルド成功 ✓
