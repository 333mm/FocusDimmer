# UI Redesign and Modernization Walkthrough

## Changes Made
- **バックアップの作成**: ユーザの要望により、編集前のメインウィンドウ状態を保持するため `MainWindow.xaml.bak` を作成しました。
- **ウィンドウサイズの縮小**: `MainWindow.xaml` のサイズを `Height="680" Width="980"` に変更し、低解像度な環境でも画面内に収まるように調整しました。
- **最小サイズ・スクロール対応**: `MinHeight="600" MinWidth="900"` を指定し、それ以上の縮小時には右カラムに対して `ScrollViewer` でスクロールできるようにしました。
- **2カラムレイアウト化**: 
    - メインの Grid を2カラム構成に変更し、左側 (`Width="*"`) にモニタごとのタブを、右側 (`Width="420"`) にグローバル設定のカード群を配置しました。これにより縦長だったUIをワイド画面に対応するモダンな形式に再構築しました。

## Exclusion Lists (IgnoreList, AlwaysBrightList, AlwaysDarkList) Migration
- **データモデルの変更**: モニタプロファイル (`MonitorProfile.cs`) および プリセット (`Preset.cs`) から除外リストに関するプロパティを削除し、グローバル設定である `AppSettings.cs` に移動しました。
- **UI コンポーネントの移動**: `MainWindow.xaml` において、左カラム（各モニタタブの中）にあった「Exclusion Lists」のカードを取り除き、右カラムのグローバル設定エリア（ScrollViewer 内）に移動しました。
- **ロジックの修正**: 
    - `MainWindow.xaml.cs` でプロセスの追加・参照を行う際、モニタプロファイルではなくグローバル設定のリスト（`IgnoreList`, `AlwaysBrightList`, `AlwaysDarkList`）を参照するように変更しました。
    - `DimmerOverlay.cs` でのウィンドウ透過の判定処理（`UpdateHoles` 等）も、グローバルなリスト（`System.Windows.Application.Current.MainWindow` 経由）を参照するように修正しました。

## Testing and Validation
- `dotnet build` を実行し、ソースコード変更にともなう C# のコンパイルエラー（`_defaultPresetId` の参照エラー、`Application` の曖昧な参照エラーなど）や XAML のタグ不整合エラーをすべて修正し、ビルドが成功すること（0 エラー）を確認しました。

## Next Steps
- ユーザ自身でアプリを起動し、右カラムへ移動した除外リストUIからプロセスを追加し、全モニタで正しく除外判定（Dim/Bright/Dark）が行われるかを確認してください。
