# タスクリスト: プリセット自動保存および除外設定の保存不具合修正

- [x] UI（XAML）レイアウトの変更
  - [x] [MainWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml) のプリセットセクションを移動
- [x] 除外設定の保存不具合修正
  - [x] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs) の `SaveSettingsActual` メソッド修正
- [x] 設定変更時の自動保存・同期の実装
  - [x] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs) で `_isApplyingPreset` フラグを追加
  - [x] `SyncProfileToPreset` および `PropagateToOtherMonitors` メソッドを実装
  - [x] `InitializeMonitors` の PropertyChanged イベントを修正
  - [x] `ApplyGlobalPresetToAllMonitors` メソッドを修正
- [x] ビルド検証
  - [x] プロジェクトのクリーンビルド
- [x] ドキュメント作成
  - [x] [walkthrough.md](file:///d:/Dev/FocusDimmer2/docs/preset_autosave_and_exclusion_fix/walkthrough.md) の作成
