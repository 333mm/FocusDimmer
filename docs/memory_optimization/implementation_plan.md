# メモリ使用量の最適化 実装計画

FocusDimmer2 のメモリ使用量を削減し、長時間の使用でも安定して動作するように最適化を行います。調査の結果、いくつかのイベント購読解除漏れやタイマーの停止漏れ、IDisposable オブジェクトの未処理が判明しました。

## Proposed Changes

### [Core]

#### [MODIFY] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)

- `SystemEvents.PowerModeChanged` の購読を `Closed` イベントで解除するように修正。
- `InitializeMonitors` で古い `DimmerOverlay` を破棄する際、明示的なクリーンアップ処理（新設）を呼び出すように変更。

#### [MODIFY] [DimmerOverlay.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Components/DimmerOverlay.cs)

- クリーンアップ用メソッド（`Dispose` または `Cleanup`）を実装。
  - `LinkedProfile.PropertyChanged` の購読解除。
  - `_delayTimer`, `_fadeTimer` の停止。
  - 内部で管理している `Window` のクローズ処理。

#### [MODIFY] [ProcessInfoHelper.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Helpers/ProcessInfoHelper.cs)

- `Process.GetProcessById` で取得した `Process` オブジェクトを確実に `Dispose` するように修正（`using` 文の使用）。

### [Cleanup]

- その他の各 View や Window の `Closed` / `Unloaded` イベントにて、DataContext のクリーンアップや購読解除の徹底。

## Verification Plan

### Manual Verification

- アプリケーションを起動し、タスクマネージャーでメモリ使用量を確認。
- 設定変更やウィンドウの開閉、モニターの構成変更などを繰り返し、メモリ使用量が単調増加（リーク）していないか確認。
- アプリを最小化した際にメモリが適切に解放されるか確認（WPF の標準動作を含む）。
