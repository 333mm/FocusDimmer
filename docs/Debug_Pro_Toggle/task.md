# デバッグ用Pro切り替えスイッチの実装

## 完了したタスク

- [x] `MainWindow.xaml.cs` にデバッグ時のみ有効なプロパティを追加
  - `IsProDebugToggle`: UIから `IsPro` を切り替えるためのプロパティ
  - `DebugModeVisibility`: デバッグビルド時のみ `Visible` を返すプロパティ
- [x] `MainWindow.xaml` のヘッダー部分にスイッチを追加
  - 言語選択ComboBoxの左側に `CheckBox` を配置
  - `#if DEBUG` に相当する表示制御を実装
- [x] ビルド確認
