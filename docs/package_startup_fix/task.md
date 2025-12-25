# パッケージ版自動起動修正タスク

- [x] 現状調査 <!-- id: 0 -->
  - [x] Package.appxmanifest の StartupTask 定義を確認 <!-- id: 1 -->
  - [x] スタートアップ登録/解除のコードを確認 (C#) <!-- id: 2 -->
  - [x] パッケージプロジェクトの設定確認 <!-- id: 3 -->
- [x] 原因特定と対策検討 <!-- id: 4 -->
- [x] 修正実施 <!-- id: 5 -->
  - [x] EntryPoint 削除 (失敗: 検証エラー) <!-- id: 7 -->
  - [x] EntryPoint に `Windows.FullTrustApplication` を設定する <!-- id: 8 -->
- [x] 動作確認手順のドキュメント化 <!-- id: 6 -->
