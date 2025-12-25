# 長時間放置時クラッシュ問題の調査・修正

- [x] クラッシュ原因の調査
  - [x] タイマー関連の問題（`_monitorTimer`, `_saveTimer`）
  - [x] イベントハンドラの蓄積（`PropertyChanged`）
  - [x] リソースリーク（Window, Brush, Geometry）
  - [x] COM相互運用の問題
  - [x] スリープ/復帰時の問題
- [x] 修正の実装
- [x] 検証
