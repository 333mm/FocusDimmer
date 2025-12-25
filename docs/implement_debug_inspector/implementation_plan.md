# 高度な検出機能付きデバッグモードの再実装

## 目的

マウス入力を透過するオーバーレイやシステムウィンドウを含め、カーソル下のすべてのウィンドウを特定できる強力なデバッグモード（インスペクター）を実装します。

## ユーザー確認事項
>
> [!NOTE]
> この実装では、マウス操作を透過するウィンドウ（`WS_EX_TRANSPARENT`）も含め、カーソル位置にある**すべての**トップレベルウィンドウを列挙して表示します。

## 変更内容

### FocusDimmer2

#### [MODIFY] [MainWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml)

- 除外リストタブ（または設定エリア）に「デバッグ（インスペクター）」ボタンを追加します。
- 新しい `ToggleDebugMode` ロジックに関連付けます。

#### [MODIFY] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)

- `ToggleDebugMode` のロジックを実装します。
- インスペクターの開始/終了処理を記述します。

#### [NEW] [DebugInspector.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Helpers/DebugInspector.cs)

- **新規ロジック**:
  - `NativeMethods.GetCursorPos` で現在のマウス座標を取得。
  - `NativeMethods.EnumWindows` を使用して全ウィンドウを反復処理。
  - 各ウィンドウについて:
    - `IsWindowVisible` で可視状態を確認。
    - `GetWindowRect` でマウスカーソルが含まれているか判定。
    - 詳細情報の収集: ハンドル (HWND), タイトル, クラス名, プロセス名, ウィンドウプレースメント, スタイル (`GetWindowLong`, `ExStyle`)。
  - 取得したウィンドウリストをZオーダー順（手前順）に整理。
  - 検出されたウィンドウリストをオーバーレイウィンドウに表示。

#### [NEW] [Views/DebugInspectorWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Views/DebugInspectorWindow.xaml)

- 常に最前面に表示され、操作を受け付けない（クリック透過）シンプルな情報表示ウィンドウ。
- マウスカーソルの近く、または画面隅に情報を表示。

#### [MODIFY] [Languages/*.json]

- デバッグ機能用の言語リソースを追加・復元します。
  - `BtnDebug`: "インスペクター" (Inspector)
  - `TooltipDebugInspector`: "マウス入力を透過する透明なウィンドウも含め、カーソル下の全ウィンドウを調査します。"

## 検証計画

### 手動検証

- アプリを起動し、インスペクターを有効にする。
- 問題の「明るい領域」やオーバーレイがある場所にカーソルを合わせる。
- これまで検出されなかったウィンドウ（透明なオーバーレイなど）がリストアップされるか確認する。
- リストアップされた情報（クラス名、プロセス名）が正しいか確認する。
