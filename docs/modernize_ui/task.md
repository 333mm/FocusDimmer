# Exclusion Lists Migration Task

## Objective
除外設定（プロセス無視、常に明るく、常に暗く）をモニタごとからグローバル設定（全体共通）に移動し、UIを右カラム（グローバル設定領域）に配置変更する。

## Tasks
- [ ] `AppSettings.cs` への除外リストプロパティの追加
- [ ] `MonitorProfile.cs` と `Preset.cs` からプロパティを削除
- [ ] `MainWindow.xaml.cs` にグローバルプロパティを用意し追加のロジックを修正
- [ ] `DimmerOverlay.cs` 内での除外判定ロジックをグローバルプロパティ参照に修正
- [ ] `MainWindow.xaml` の UI を右カラムの ScrollViewer に移動・バインディング修正
- [ ] ビルドと表示・動作・保存の確認
