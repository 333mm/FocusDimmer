# メモリ使用量の最適化 修正内容の確認

FocusDimmer2 のメモリ使用量を最適化し、リソースの適切な解放が行われるように修正を完了しました。

## 変更内容

### [Core]

#### [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)

- `SystemEvents.PowerModeChanged` の購読を `OnClosing` で解除するように修正しました。これにより、アプリ終了後もイベントハンドラが残り続けるのを防ぎます。
- `InitializeMonitors` でモニター構成が変更された際、古い `DimmerOverlay` インスタンスを `Dispose` するように修正しました。

#### [DimmerOverlay.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Components/DimmerOverlay.cs)

- `IDisposable` インターフェースを実装しました。
- `Dispose` メソッド内で以下のクリーンアップを行います：
  - `MonitorProfile.PropertyChanged` の購読解除。
  - 各種タイマー (`_delayTimer`, `_fadeTimer`) の停止。
  - アニメーションイベントの解除。
  - オーバーレイウィンドウのクローズ。

#### [ProcessInfoHelper.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Helpers/ProcessInfoHelper.cs)

- `Process.GetProcessById` で取得した `Process` オブジェクトを `using` 文で囲み、確実に `Dispose` されるように修正しました。OS のプロセスハンドルを適切に解放します。

## 検証結果

### 1. ビルド検証

- `dotnet build` を実行し、修正したコードが正しくコンパイルされることを確認しました。

### 2. コードレビュー

- 追加したクリーンアップ処理が、不要になったタイミングで正しく呼び出されることを静的解析により確認しました。

## ユーザーへの影響

- 長時間の利用時でもメモリ使用量の増加が抑えられ、動作がより安定します。
- モニターの抜き差しや解像度変更を繰り返した際のメモリリークが解消されます。
