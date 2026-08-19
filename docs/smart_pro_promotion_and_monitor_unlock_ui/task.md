# タスクリスト: スマートなPRO版誘導UI ＆ マルチモニター表示ロック機能の実装

- [x] 設計と実装計画の作成、ユーザー承認 <!-- id: 0 -->
- [x] MonitorProfile に IsPrimary / IsLockedByFree プロパティを追加 <!-- id: 1 -->
- [x] InitializeMonitors でフリー版でも全モニターをタブに登録（サブモニターはオーバーレイ生成なし） <!-- id: 2 -->
- [x] 全7言語のJSONファイルおよび LocalizationService に翻訳キーを追加 <!-- id: 3 -->
  - `HeaderMultiMonitorPro`（マルチモニター減光制御）
  - `DescMultiMonitorPro`（接続された複数のディスプレイを個別に減光・連動制御するにはPRO版が必要です。）
  - `BtnUpgradePro`（PROにアップグレード）
- [x] MonitorTabs の DataTemplate にサブモニター時の 🔒 アイコンを追加 <!-- id: 4 -->
- [x] サブモニター選択時のスマートなロック案内カード ＆ アップグレードCTAの実装 <!-- id: 5 -->
- [x] サイドバーの PRO アップグレードバナーを上品で洗練されたデザインにリファイン <!-- id: 6 -->
- [x] プリセットページおよび各PRO機能のバッジ・カードを統一されたスマートな見た目に調整 <!-- id: 7 -->
- [x] ビルド検証 (Lite, Debug, Release, Pro) と警告・エラー修正 <!-- id: 8 -->
- [x] ウォークスルーの作成 <!-- id: 9 -->
