# タスクリスト: タスクトレイ最小化オプション化 ＆ ディスプレイ設定チェーンリンク機能

- [x] 設計と実装計画の作成、ユーザー承認 <!-- id: 0 -->
- [x] 全7言語のJSONファイルおよび LocalizationService に翻訳キーを追加 <!-- id: 1 -->
  - `CheckCloseToTray`（閉じるボタンでタスクトレイに最小化）
  - `LabelLinkMonitors` / `TooltipLinkMonitors`（ディスプレイ設定のリンク）
- [x] AppSettings に CloseToTray および AreMonitorsLinked 設定プロパティを追加 <!-- id: 2 -->
- [x] サイドバーフッターの「タスクトレイに閉じる」ボタンを廃止 <!-- id: 3 -->
- [x] General ページに「閉じるボタンでタスクトレイに最小化」チェックボックスを追加 <!-- id: 4 -->
- [x] MainWindow.xaml.cs の OnClosing で CloseToTray 設定に応じた挙動分岐を実装 <!-- id: 5 -->
- [x] Monitors ページのタブ横にチェーン（リンク/アンリンク）トグルボタンを配置 <!-- id: 6 -->
- [x] AreMonitorsLinked 有効時の全モニタ同時連動（双方向同期）ロジックを実装 <!-- id: 7 -->
- [x] ビルド確認 (Debug, Release, Pro) と警告・エラー解消 <!-- id: 8 -->
- [x] ウォークスルーの作成 <!-- id: 9 -->
