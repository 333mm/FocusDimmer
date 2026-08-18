# Windows 11と親和性の高いFluent UIデザインへの刷新 実装計画

Windows 11のデザインシステム（Fluent Design System / WinUI 3）に準拠し、最新のWindows 11環境とシームレスに調和する美しくモダンなUIへの刷新を行います。

## デザイン刷新の要点

1. **マテリアルとレイヤリング（Surface & Layering）**
   - **ウィンドウ背景**: `#E6202020` / `#D91C1C1C`（Win11 ダークモード標準のアクリル/Mica調和色）。
   - **カードコンテナ（CardStyle）**: 背景 `#0CFFFFFF`（CardBackgroundFillDefault）、ボーダー `#14FFFFFF`、角丸 `CornerRadius="8"`。
   - **コントロール（ボタン・入力等）**: 角丸 `4〜6px`、ホバー・押下時の繊細なフィードバック。

2. **タイポグラフィとアイコン階層**
   - フォントファミリに `Segoe UI Variable Text`, `Segoe UI` を優先指定。
   - アイコンは `Segoe Fluent Icons`, `Segoe MDL2 Assets` を統一使用。
   - タイトル・セクション見出し・ラベル・補助テキストのフォントウェイトとカラーを最適化（プライマリ: `#FFFFFF`, セカンダリ: `#C8C8C8`, ターシャリ: `#8A8A8A`）。

3. **コントロール（WinUI 3 風デザイン）**
   - **Slider**: WinUI 3 スタイルのスライダートラック（4px 高、角丸 2px、AccentColor 塗りつぶし、ホバー・ドラッグ時のスムーズスケール）。
   - **TabItem**: ピル型デザイン（角丸 6px、選択時ハイライト、すっきりとしたタブバー）。
   - **ComboBox**: 角丸 4px、ドロップダウンメニューの影とスムーズな開閉。
   - **Buttons**:
     - プライマリアクセントボタン（`AccentColor` 背景、ハイコントラスト文字）
     - 標準ボタン（`#10FFFFFF` 背景、`#18FFFFFF` ボーダー、角丸 4px）
     - アイコンボタン（ツールバー用）
   - **Caption Buttons**: 最小化・最大化・閉じるボタンの Win11 標準スタイル（リサイズ可能時に最大化ボタンを表示、最大化時はアイコン切替）。

4. **レイアウト構造（MainWindow）の正常化**
   - **Header（Row 0）**: アプリロゴ・タイトル・FREE/PROバッジ・言語選択・プロモバナーを最上部に整然と配置。
   - **Main Content（Row 1）**: 左側モニタータブ設定カード群と、右側アプリケーション除外リストを美しく2ペイン配置。

---

## 変更予定ファイル

### App.xaml
- カラー・ブラシ定義の整理と WinUI 3 スタイルの拡張
- `FluentWindowStyle`（WindowChrome、タイトルバー、キャプションボタン、最大化トリガー）の強化
- コントロールスタイル（Button, Slider, CheckBox, TextBox, ComboBox, TabItem, CardStyle, ToolTip, ScrollBar）の Fluent Design 化

### MainWindow.xaml
- ヘッダー（Row 0）とメインコンテンツ（Row 1）のグリッド分離
- 重複する `<Window.Resources>` を整理し `App.xaml` に一元化
- 各カード・スライダー・入力欄・プロモバナーのレイアウトとマージンを Windows 11 Fluent ガイドラインに沿って最適化

### サブウィンドウ群
- ProcessSelectionWindow.xaml
- Views/ColorPickerWindow.xaml
- Views/DebugInspectorWindow.xaml
- Views/InspectorActionDialog.xaml
- Views/MigrationGuideWindow.xaml
  - 統一された `FluentWindowStyle` を適用し、一貫したダークFluentデザインを維持。

---

## 検証計画

### 1. ビルド検証
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Debug`
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Release`
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Pro`
- 警告0・エラー0を確認。

### 2. UI検証
- メインウィンドウのヘッダー・タブ・カード・除外リストが整然と美しくレイアウトされていることを確認。
- 最大化・最小化・リサイズがスムーズに動作することを確認。
- 各サブウィンドウ（プロセス選択、カラーピッカー等）が統一感のあるデザインで表示されることを確認。
