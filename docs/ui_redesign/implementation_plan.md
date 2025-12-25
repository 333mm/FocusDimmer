# Windows 11 Acrylic Style デザイン & 実装計画

## 概要

FocusDimmer の UI を Windows 11 のデザインガイドライン ("Mica" / "Acrylic" スタイル) に沿ったモダンなものに刷新する。
タイトルバーを統合し、アイコンを活用して視認性を向上させる。

## ユーザーレビューが必要な項目

- **アセット**: アイコンフォント (Segoe Fluent Icons) を使用するため、Windows 10 環境での表示確認が必要（Win10でもSegoe Fluent Iconsがあれば表示されるが、フォールバックが必要な場合がある）。ターゲットはWin10以降と想定。

## デザイン仕様

### 1. ウィンドウ構造 (Window Chrome)

- 標準のタイトルバーを非表示にし、クライアント領域に拡張する (`WindowChrome`).
- ウィンドウ上部に「アプリタイトル」と「バージョン」を配置（左揃え）。
- ウィンドウ操作ボタン（最小化・閉じる）を右上隅にカスタム実装する。

### 2. 背景効果 (Material)

- **Windows 11**: `Mica` または `Acrylic` 効果を適用。
  - P/Invoke: `DwmSetWindowAttribute` (`DWMWA_SYSTEMBACKDROP_TYPE`) を使用。
  - 背景色: 透明または半透明 (`#00000000` または `#20202020`) に設定し、DWMのエフェクトを透過させる。
- **Windows 10**: `Acrylic` (BlurBehind) または単色のダークテーマ (`#202020`) にフォールバック。

### 3. タイポグラフィ & アイコン

- **フォント**: `Segoe UI Variable` (Win11) / `Segoe UI` (Win10).
- **アイコン**: `Segoe Fluent Icons` フォントを使用。
  - 設定項目やタブヘッダーにアイコンを追加。
  - 例:
    - モニター: 🖥 (`\uE7F4`)
    - 色設定: 🎨 (`\uE790`)
    - 透明度: 🌗 (`\uE793`)
    - ショートカット: ⌨ (`\uE765`)
    - スタートアップ: 🚀 (`\uF3B3`)

### 4. コントロールスタイル

- **カード**: 角丸 (`CornerRadius="8"`), わずかなボーダー, 背景色は半透明 (`#1FFFFFFF` 等)。
- **ボタン**: 標準で背景薄め、ホバーで強調。
- **スライダー**: つまみ (Thumb) のサイズ調整、トラックの太さ調整。

## 実装計画

### Phase 1: インフラストラクチャ (Helpers & Resources)

#### [NEW] [WindowHelper.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Helpers/WindowHelper.cs) (既存または拡張)

- `ApplySystemBackdrop`: Windows 11 の Mica/Acrylic を適用するメソッドを追加。
- `ExtendFrameIntoClientArea`: タイトルバー拡張用のロジック。

#### [MODIFY] [MainWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml)

- `WindowChrome` の設定を追加。
- 背景ブラシを修正 (`Transparent` or High Transparency).
- フォントリソース (`Segoe Fluent Icons`) の定義。

### Phase 2: レイアウト & コントロール刷新

#### [MODIFY] [MainWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml)

- **TitleBar**: グリッドの最上部にドラッグ可能な領域 (`WindowChrome.IsHitTestVisibleInChrome="True"`) を作成。
- **Content**: `TabControl` や設定項目をカードデザイン (`Border` design) に格納。
- **Icons**: `TextBlock` (`FontFamily="Segoe Fluent Icons"`) を各ラベルの前に配置。

### Phase 3: コードビハインド追従

#### [MODIFY] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)

- `OnSourceInitialized` で背景効果適用メソッドを呼び出す。
- タイトルバーのドラッグ動作ハンドリング (必要であれば)。

## 検証計画

- Windows 11 環境での Mica 効果の確認。
- ウィンドウのドラッグ、リサイズ、最大化（今回はResizeMode="CanResize"だがMaxHeight設定あり？）動作の確認。
- ライト/ダークテーマ切り替え（今回はダーク固定の模様だが、OS設定に追従するか確認）。-> 現状は `#202020` 固定なのでダークベースの Material を適用。
