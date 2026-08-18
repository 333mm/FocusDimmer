# デバッグ用Pro切り替えスイッチの実装と修正

## 実施済み

- [x] Debug用Proスイッチの実装（厳密な分離）
- [x] 初期プリセット選択ロジックの修正

# 最小化ボタンの設置

## 完了したタスク

- [x] 既存の「閉じる(Close)」ボタンの確認
- [x] **ボタンの意味とラベルの不一致を解消**
  - ラベルを `BtnMinimize` ("最小化") に変更 (ja.json, en.json)
  - アイコンを `xE921` (ChromeMinimize) に変更
  - 既存の `MinimizeButton_Click` イベントを継続利用
