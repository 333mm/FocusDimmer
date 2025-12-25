# 自動起動オプションの修正タスク

- [x] 現状の調査 <!-- id: 0 -->
  - [x] プロジェクト構成の確認 <!-- id: 1 -->
  - [x] 自動起動関連コードの特定 (`StartupTask` in manifest, `StartupManager.cs`) <!-- id: 2 -->
  - [x] 設定UIのバインディング確認 <!-- id: 3 -->
- [x] 修正計画の作成 (Implementation Plan) <!-- id: 4 -->
- [x] 修正の実施 <!-- id: 5 -->
  - [x] `StartupManager.cs` の非同期化リファクタリング <!-- id: 6 -->
  - [x] `MainWindow.xaml.cs` の初期化ロジック修正 <!-- id: 7 -->
  - [x] イベントハンドラの非同期対応 <!-- id: 8 -->
- [x] 検証 (Verification) <!-- id: 9 -->
  - [x] タスクマネージャーでの状態反映確認 <!-- id: 10 -->
  - [x] 再起動テスト <!-- id: 11 -->
  - [x] 動作確認手順のドキュメント化 (Walkthrough) <!-- id: 12 -->
