# プロセス選択ウィンドウ UI修正

## アクリル効果とスクロールバーの互換性修正

当初の試みは、`AllowsTransparency="True"` が最新のDWMアクリル効果をブロックしていることや、暗黙的なスタイルが `ListBox` のスクロールバーに正しく伝播していないことが原因で失敗しました。

## 提案される変更

### FocusDimmer2

#### [変更] [ProcessSelectionWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/ProcessSelectionWindow.xaml)

- **アクリル修正**:
  - `AllowsTransparency="False"` に設定。
  - `WindowChrome` を追加してウィンドウフレームを処理（標準の境界線を削除しつつキャプション機能を維持）。
  - `Background` を `Transparent` に設定。
- **スクロールバー修正**:
  - `ScrollBar` スタイルを明示的に使用する `ScrollViewer` スタイルを定義。
  - この `ScrollViewer` スタイルを `ListBox` に適用。

## 検証

- 「ゴースト」ウィンドウの問題（背景が黒くなる）が発生しないことを確認。
- アクリル（ブラー）効果が有効になっていることを確認。

## 修正ラウンド2 (ユーザーフィードバック対応)

### 問題点

1. **ブラーなし**: ウィンドウが完全に透明（ガラス効果は有効だがブラー属性が適用/動作していない）。
2. **太いスクロールバー**: ListBox上の明示的なScrollViewerスタイルが内部のScrollBarに適用されていなかった。

### 解決策

1. **ブラーの強制**: `SetWindowCompositionAttribute` (AccentPolicy) を `ACCENT_ENABLE_BLURBEHIND` で強制的に使用する。これは `AllowsTransparency=False` の場合にWin10/11で「すりガラス」の外観を得る最も確実な方法です。
2. **暗黙的なスクロールバー・スタイル**: `ProcessSelectionWindow.Resources` 内の `ThinScrollBar` スタイルの `x:Key` を**削除**し（暗黙的にし）、`ListBox`（およびウィンドウ内の他のすべて）が強制的にそれを使用するようにします。

## 修正ラウンド4 (ユーザーフィードバック対応 - アクリル代替とクラッシュ回避)

### 問題点

1. **透明すぎる/ブラー不安定**: アクリル効果が適用されず、ウィンドウが完全に透けてしまう、またはブラーが安定しない。
2. **参照ボタンクラッシュ**: `ExplorerBlurMica.dll` がモダンなファイルダイアログをフックしてクラッシュを引き起こす。

### 解決策

1. **不透明ダーク背景**: アクリル効果をオフにし、背景色を `#1F1F1F` に固定。
2. **レガシーダイアログの使用**: `System.Windows.Forms.OpenFileDialog` で `AutoUpgradeEnabled = false` を設定し、クラッシュするモダンフックを回避する。
