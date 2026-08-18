# Implementation Plan: Migrate Exclusion Lists to Global Settings

現在、除外設定リスト（IgnoreList, AlwaysBrightList, AlwaysDarkList）は各モニタ（`MonitorProfile.cs`）ならびに各プリセット（`Preset.cs`）ごとに保存・適用されています。
ユーザ要望に基づき、これらをグローバル設定（`AppSettings.cs`）へ移行し、全モニタ共通の設定として適用されるように変更します。
それにともない、メインウィンドウでのUI表示も左カラム（モニタ別タブ内）から右カラム（全体設定エリア）へ移動します。

## Proposed Changes

### Core Models & Serialization

#### [MODIFY] [AppSettings.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Models/AppSettings.cs)
- `IgnoreList`, `AlwaysBrightList`, `AlwaysDarkList` のプロパティ（初期値含む）を追加します。

#### [MODIFY] [MonitorProfile.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Models/MonitorProfile.cs)
- `IgnoreList`, `AlwaysBrightList`, `AlwaysDarkList` のプロパティを削除します。
- `ApplySettings()` メソッドから上記プロパティの代入処理を削除します。

#### [MODIFY] [Preset.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Models/Preset.cs)
- `IgnoreList`, `AlwaysBrightList`, `AlwaysDarkList` のプロパティを削除します。
- `FromProfile()` および `ApplyToProfile()` から上記プロパティの代入処理を削除します。

### Application Logic

#### [MODIFY] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)
- `MainWindow` クラス内で、UIからバインディングして保存処理に繋げるため、以下のプロパティを作ります：
    - `public string IgnoreList { get => _appSettings.IgnoreList; set { _appSettings.IgnoreList = value; NotifyPropertyChanged(); RequestSave(); } }`
    - `AlwaysBrightList`, `AlwaysDarkList` についても同様。
- `AddProcessToList()` やインスペクターからの処理において、特定の `MonitorProfile` に追加するのをやめ、MainWindow自身のグローバルプロパティへ追加するように変更します。

#### [MODIFY] [DimmerOverlay.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Components/DimmerOverlay.cs)
- モニタプロファイル(`LinkedProfile.IgnoreList`等)を参照してウィンドウの明るさを切り替えていた部分を修正します。
- メインウィンドウオブジェクト（`Application.Current.MainWindow as MainWindow`など）から最新のグローバルリストを参照するように変更します。

### UI Adjustments

#### [MODIFY] [MainWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml)
- `TabControl` 内の `DataTemplate` から「Exclusion Lists」セクション（"AppList", "AlwaysBright", "AlwaysDark"等）のカードを取り除きます。
- 取り除いたカードを右カラムの `ScrollViewer` 内に配置します。
- バインディングを `{Binding IgnoreList}` などの直接パスに変更します（右カラムの DataContext は MainWindow なので、`RelativeSource` の記述は不要になります）。

## Verification Plan

### Manual Verification
- ビルドがエラー・警告なく成功することを確認する。
- UIの右カラムに「除外リスト」項目が表示されており、左側のモニタ設定タブからは削除されていることを確認する。
- いずれかのモニタでアプリを除外・Always Bright等に設定した際、他のモニタも含めすべてのDimmerオーバレイで正しくそのプロセスが除外・明るく表示されることを確認する。
- アプリを再起動しても上述の除外設定の文字列が復元されること（保存/ロードの成功）を確認する。
