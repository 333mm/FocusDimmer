# テキストとアイコンの視認性改善（アクセントカラーの排除）

テキストやアイコンにおいて、OSのアクセントカラーを使用せず、常に高いコントラストが得られる色（白や薄いグレー）を使用するように変更しました。

## 実施内容

### 1. テキスト・アイコンからのアクセントカラー排除

OSの設定によってはアクセントカラーが背景と馴染みすぎてしまい、文字やアイコンが読みづらくなることがありました。これを防ぐため、情報伝達に関わる部分ではアクセントカラーを使用しないように統一しました。

- **修正箇所:** [MainWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml)
- **調整内容:**
  - 各セクションのヘッダーアイコン（Overlay, Animation, Exclusion Lists 等）を `AccentColor` から `TextSecondary` (薄いグレー) に変更。
  - 「🔒 PRO」バッジのテキストを `AccentColor` から `TextSecondary` に変更。
  - プリセット一覧の「 (Default)」ラベルを `AccentColor` から **`White`** に変更し、最も目立つように調整。
  - スタートアップ設定等の重要アイコンを `TextSecondary` に変更。

### 2. 視認性の確保

インタラクティブな要素（スライダーのつまみやチェックボックスの背景）にはアクセントカラーを残しつつ、読ませるための「文字」や「アイコン」は白系統に統一したことで、OSの設定に左右されず常に高い視認性が確保されます。

## 検証結果

### ビルド確認

- [x] `dotnet build` にてエラーなく正常にビルドできることを確認しました。

### 修正のポイント

```xml
<!-- MainWindow.xaml の修正例 -->
<!-- 以前: Foreground="{StaticResource AccentColor}" -->
<!-- 修正後: Foreground="{StaticResource TextSecondary}" または Foreground="White" -->
<TextBlock Text="&#xE790;" Foreground="{StaticResource TextSecondary}" ... />
<TextBlock Text="{Binding ... LabelDefault}" Foreground="White" FontWeight="Bold" ... />
```
