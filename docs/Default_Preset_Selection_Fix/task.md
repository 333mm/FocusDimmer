# デバッグ用Pro切り替えスイッチの実装と修正

## 完了したタスク

- [x] `MainWindow.xaml.cs` にデバッグ時のみ有効なプロパティを追加
  - `IsProDebugToggle`: UIから `IsPro` を切り替えるためのプロパティ
  - `DebugModeVisibility`: デバッグビルド時のみ `Visible` を返すプロパティ
- [x] `MainWindow.xaml` のヘッダー部分にスイッチを追加
  - 言語選択ComboBoxの左側に `CheckBox` を配置
  - `#if DEBUG` に相当する表示制御を実装 (Releaseビルドへの混入防止)
- [x] ビルド確認
- [x] **[NEW] Proスイッチ切り替え時のプリセット選択不具合の修正**
  - `InitializeMonitors` 内で、プリセット再読み込み前に現在の選択(ID)を退避
  - 退避したID、または `DefaultPresetId` を優先的に復元するロジックを実装
  - 「一番最初のプリセット」に戻る問題を解決
