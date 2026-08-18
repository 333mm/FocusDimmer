# プリセット機能 - 実装完了レポート

## 概要

FocusDimmer2にグローバルプリセット機能を実装しました。全モニタに一括で適用されるプリセットの作成/選択/編集/削除と、特定プロセス実行時の自動切替機能が利用可能です。

## 変更内容

### 新規ファイル

| ファイル | 説明 |
|---------|------|
| `Models/Preset.cs` | プリセット設定を保持するモデルクラス |
| `Models/ProcessPresetRule.cs` | プロセス名とプリセットIDの関連付けルール |

### 変更ファイル

#### `Models/AppSettings.cs`

- `Presets` (List<Preset>): グローバルプリセットリスト
- `SelectedPresetId` (string): 現在選択中のプリセットID

#### `MainWindow.xaml`

フッター部分にコンパクトなプリセット管理UIを追加：

- プリセット選択ComboBox
- 編集/追加/削除ボタン
- プロセスルール管理ボタン

#### `MainWindow.xaml.cs`

- `GlobalPresets`: プリセットのObservableCollection
- `SelectedGlobalPresetId`: 選択中プリセットID
- `GlobalPreset_SelectionChanged`: プリセット選択時に全モニタに適用
- `AddGlobalPreset_Click`: 現在の設定を新規プリセットとして保存
- `EditGlobalPresetName_Click`: プリセット名の編集
- `DeleteGlobalPreset_Click`: プリセット削除
- `ManageProcessRules_Click`: プロセスルール追加
- `ApplyGlobalPresetToAllMonitors`: 全モニタへの一括適用

#### `MonitorTimer_Tick`

プロセス自動切替ロジックを追加：

- アクティブプロセス名を取得
- GlobalPresetsからマッチするProcessRuleを検索
- マッチした場合、該当プリセットを全モニタに適用

#### `Services/LocalizationService.cs`

英語/日本語の新規文字列を追加：

- `HeaderPreset`: "PRESETS" / "プリセット"
- `TooltipEditPreset`, `TooltipAddPreset`, `TooltipDeletePreset`
- `LabelProcessSwitch`, `BtnAddProcessRule`
- `DefaultPresetName`, `NewPresetName`
- `MsgConfirmDeletePreset`, `MsgEnterPresetName`

## 検証

```
✓ ビルド成功
```

## 使用方法

1. **プリセット作成**: フッターの「+」ボタンで現在の全モニタ設定を新規プリセットとして保存
2. **プリセット選択**: ドロップダウンからプリセットを選択すると全モニタに適用
3. **プリセット編集**: 鉛筆アイコンでプリセット名を変更
4. **プリセット削除**: ゴミ箱アイコンで選択中のプリセットを削除
5. **プロセス自動切替**: 歯車アイコンからプロセスを追加すると、そのプロセスがアクティブ時にプリセットが自動適用

## 次のステップ

- [ ] 手動テスト実施
  - プリセット作成/選択/編集/削除の動作確認
  - プロセス自動切替の動作確認
  - 設定永続化の確認
