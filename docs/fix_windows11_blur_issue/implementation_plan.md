# Windows 11最新環境におけるブラー問題の修正と後方互換性対応の実装計画

最新版のWindows 11（22H2/23H2/24H2）において、アプリケーションのウィンドウ全体に不要なブラーがかかってしまい文字やUIがぼやけて使用できなくなる問題を解決し、Windows 10および過去バージョンのWindows 11との後方互換性を保ちながら最新環境に対応します。

## 問題の原因分析

1. **`WindowChrome.GlassFrameThickness="-1"` によるDWMグラス効果の全面拡張**
   - `App.xaml` の `FluentWindowStyle` で `GlassFrameThickness="-1"` が指定されているため、DWMがウィンドウのクライアント領域全体をGlass領域と認識します。
   - これが原因で、Windows 11の最新DWMコンポジションにおいて、ウィンドウ全面に強力なブラー効果・ぼかし合成が強制適用され、UI要素やテキストがぼやけてしまいます。
2. **`WindowHelper.cs` のバックドロップ適用ロジック**
   - Windows 11の最新API（`DWMWA_SYSTEMBACKDROP_TYPE`）への対応において、`DwmSetWindowAttribute` の定数定義やバージョンの安全なハンドリングが不十分な場合、フォールバックの `SetWindowCompositionAttribute` (`ACCENT_ENABLE_BLURBEHIND`) が呼び出されてしまう可能性がありました。
   - `ACCENT_ENABLE_BLURBEHIND` はWindows 10用の非公開APIであり、Windows 11ではウィンドウ全体に副作用のあるブラーを発生させます。
3. **ウィンドウ背景（Background）と透過設定**
   - バックドロップ（Mica/Acrylic）を正常にレンダリングするためには、`GlassFrameThickness="0"` としつつ、DWMが適用される場合は半透明〜透明な背景を通し、適用されないレガシー環境やフォールバック時でも視認性の高いダーク背景（例: `#202020` / `#1F1F1F`）が正しく表示されるように設計する必要があります。

## ユーザー確認事項 (User Review Required)

> [!NOTE]
> - Windows 11（Build 22621以降）では、公式にサポートされている `DWMWA_SYSTEMBACKDROP_TYPE` (Acrylic/Mica) を使用します。
> - Windows 11（Build 22000 21H2）では、当時の公式APIである `DWMWA_MICA_EFFECT` を使用します。
> - Windows 10以前の環境、またはDWM効果が無効化されている環境では、UIが見えなくなる事故を防ぐため、安全なソリッドダーク背景（`#1F1F1F` または薄い半透明）にフォールバックし、文字の視認性と操作性を100%保証します。

## 変更予定の内容

### 1. WindowHelper.cs の改善

#### [MODIFY] WindowHelper.cs
- `NativeMethods` 側の定数定義を整理（`DWMWA_SYSTEMBACKDROP_TYPE = 38`, `DWMWA_MICA_EFFECT = 1029`, `DWMWA_WINDOW_CORNER_PREFERENCE = 33`, `DWMWA_USE_IMMERSIVE_DARK_MODE = 20`）。
- OSバージョン判定およびDWM属性適用のフォールバックチェーンを実装：
  1. **Windows 11 Build 22621+ (22H2以降)**: `DWMWA_SYSTEMBACKDROP_TYPE` で `DWMSBT_TRANSIENTWINDOW` (Acrylic: 3) または `DWMSBT_MAINWINDOW` (Mica: 2) を適用。
  2. **Windows 11 Build 22000 (21H2)**: `DWMWA_MICA_EFFECT` (1029) を適用。
  3. **ダークモード属性**: `DWMWA_USE_IMMERSIVE_DARK_MODE` (20) を適用。
  4. **角丸属性**: `DWMWA_WINDOW_CORNER_PREFERENCE` (33) で `DWMWCP_ROUND` (2) を適用。
  5. **Windows 11では `ACCENT_ENABLE_BLURBEHIND` を絶対に実行しない**（全体ブラーの再発防止）。
  6. **Windows 10環境**: 安全なフォールバック処理を行い、UI描画の破損を防止。

### 2. XAML スタイルの修正

#### [MODIFY] App.xaml
- `FluentWindowStyle` の `WindowChrome` 設定において、`GlassFrameThickness="-1"` を `GlassFrameThickness="0"` に修正。
- ウィンドウの背景設定を見直し、DWMバックドロップが有効な場合は美しいすりガラス/Micaが表示され、無効・非対応な環境でも視認性の高いダーク背景が維持されるように調整。

### 3. 各ウィンドウ実装の整合性確認

#### [MODIFY] FluentWindow.cs
#### [MODIFY] MainWindow.xaml.cs
- `ApplySystemBackdrop` の重複呼び出し（`FluentWindow.Loaded` と `MainWindow.OnSourceInitialized`）を整理し、適切なタイミング（`SourceInitialized`）で一度だけ安全に適用されるよう統一。

---

## 検証計画

### 1. ビルド検証
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Debug`
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Release`
- 警告0・エラー0であることを確認。

### 2. 動作確認
- アプリケーション起動時に MainWindow 全体に不要なブラーがかからず、クリアに表示されることを確認。
- 各ウィンドウ（設定画面、プロセス選択ダイアログ、カラーピッカー、移行ガイド等）が正常な外観で開くことを確認。
- Windows 10およびWindows 11の互換性が保たれていることを確認。
