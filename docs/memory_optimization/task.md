# メモリ使用量の最適化

- [x] 現状の調査とボトルネックの特定 [x]
  - [x] `MainWindow.xaml.cs` の調査
  - [x] `Services` の調査
  - [x] イベントハンドラの解除漏れの確認
- [x] 実装の実施 [x]
  - [x] `DimmerOverlay.cs` のクリーンアップ処理実装
  - [x] `MainWindow.xaml.cs` のイベント解除追加
  - [x] `ProcessInfoHelper.cs` の `Dispose` 対応
- [x] 検証 [x]
  - [x] ビルド検証
  - [x] walkthrough.md の作成
