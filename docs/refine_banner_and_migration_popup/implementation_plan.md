# バナー改善と移行案内ポップアップ刷新

## 目標

1. **バナーUI/UXの修正**:
    - 配置を中央に戻す。
    - 閉じるボタンがデザインを崩さないようマージン調整。
    - バナー本体をクリックしても「通知済み」にせず、**閉じるボタンを押した時だけ**格納（メガホン化）する。
    - メガホンアイコンを白色に変更。
2. **移行案内ポップアップの刷新**:
    - 指定された日本語の長文案内を表示するため、スクロール可能なカスタムウィンドウ (`MigrationGuideWindow`) を実装する。
    - 全言語（en, de, es, fr, pt, zh, ja）に対応した翻訳テキストを追加する。

## 変更内容

### FocusDimmer2

#### [NEW] [Views/MigrationGuideWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Views/MigrationGuideWindow.xaml)

- スクロール可能な `TextBlock` を持つウィンドウ。
- **「統合版のストアページを開く」ボタン** (`MigrationOpenStorePage`) と「閉じる」ボタンを配置。
- スタイリングはメインアプリに合わせる（ダークモード考慮）。

#### [MODIFY] [MainWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml)

- バナーの `HorizontalAlignment` を `Center` に戻す。
- 閉じるボタンの配置（Margin）を調整し、見切れを防ぐ。
- メガホンアイコン (`TextBlock Text="&#xE789;"`) の `Foreground` を白 (`White` or `#FFFFFFFF`) に変更。

#### [MODIFY] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)

- `MigrationInfo_Click`:
  - `IsLegacyBannerDismissed = true` の行を削除（バナー本体クリックでは消さない）。
  - `MessageBox.Show` の代わりに `new MigrationGuideWindow().ShowDialog()` を呼び出すように変更。
- `CloseBanner_Click`:
  - ここでのみ `IsLegacyBannerDismissed = true` を実行する（現状通り）。

#### [MODIFY] [Resources/*.json]

- 各言語ファイルに `MigrationGuideText` キーを追加し、翻訳されたテキストを格納する。
- 各言語ファイルに `MigrationOpenStorePage` キーを追加し、「統合版のストアページを開く」の翻訳を格納する。

## 翻訳テキスト（予定）

- **JA**: 指定された原文
- **EN**: 英訳
- **ZH/DE/ES/FR/PT**: それぞれ機械翻訳等で適切な文章を作成

## 検証計画

1. **バナー動作**:
    - 本体クリック → ガイドが出るが、バナーは消えない。
    - 閉じるボタン → バナーが消え、メガホンが出る。
    - 再度リセットして表示確認。
2. **ガイド表示**:
    - 長文がスクロールでき、レイアウトが崩れていないか。
    - 言語切り替え時にテキストが切り替わるか（再起動等の既存仕様に準ずる）。
3. **リンク**:
    - ガイド内のボタンから正しくストアなどが開くか（またはリンクを表示するか）。
