# UIの乱れの調査と修正の実装計画

アプリケーションのメインウィンドウにおいて、ヘッダー領域とメインコンテンツ領域が重なって表示されてしまうレイアウト崩れの原因を特定し、これを修正します。あわせて FluentWindowStyle の最大化ボタン制御を改善します。

## 問題の原因分析

1. **`MainWindow.xaml` における Grid.Row 指定の誤り**
   - `MainWindow.xaml` のルート Grid には `RowDefinition Height="Auto"`（Row 0）と `RowDefinition Height="*"`（Row 1）が定義されています。
   - しかし、Header（タイトル・FREE/PROバッジ・言語切替・バナー等）が `<StackPanel Grid.Row="1" ...>` と指定され、Main Content（モニタータブ・設定カード群）も `<Grid Grid.Row="1" ...>` と指定されていたため、**Row 0 が空白のまま Row 1 にヘッダーとコンテンツが完全に重複して描画**されていました。
2. **`FluentWindowStyle` の最大化ボタン制御**
   - `App.xaml` の共通ウィンドウテンプレートで最大化ボタンが `Visibility="Collapsed"` 固定になっており、リサイズ可能なメインウィンドウでも最大化ボタンが非表示になっていました。`ResizeMode` および `WindowState` と連動するトリガーを追加して操作性を改善します。

## ユーザー確認事項 (User Review Required)

> [!NOTE]
> - `MainWindow.xaml` のヘッダーを `Grid.Row="0"` に移動することで、ヘッダー（アプリアイコン、タイトル、言語切り替え、プロモバナー等）が上部に綺麗に配置され、その下にモニター設定・プロセス一覧等のメインコンテンツが正しく展開されます。

## 変更予定の内容

### 1. MainWindow.xaml のレイアウト修正

#### [MODIFY] MainWindow.xaml
- Header の `Grid.Row="1"` を `Grid.Row="0"` に修正。

### 2. App.xaml の FluentWindowStyle 改善

#### [MODIFY] App.xaml
- `FluentWindowStyle` の最大化ボタンにトリガー（`ResizeMode="CanResize"` / `CanResizeWithGrip"` で表示）を追加し、最大化時のアイコン変更およびマージン調整を追加。

---

## 検証計画

### 1. ビルド検証
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Debug`
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Release`
- 警告0・エラー0を確認。

### 2. 動作確認
- メインウィンドウを開いた際、ヘッダーとメインコンテンツが重複せず上下に整然と配置されることを確認。
