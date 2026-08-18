# OSアクセントカラー調整とプリセット永続化の修正

OSのアクセントカラーの視認性向上と、プリセット設定の保存機能の不具合を修正しました。

## 実施内容

### 1. OSアクセントカラーの自動調整

OSのアクセントカラー（WindowGlassColor）を取得し、彩度や明度が低い場合（グレーに近い色や暗すぎる色）に、アプリ内で見やすい値まで自動的にブーストする機能を実装しました。

- **実装:** [ColorHelper.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Helpers/ColorHelper.cs) を新規作成し、HSV空間での色調整ロジックを追加。
- **反映:** [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs) の `Loaded` イベントおよびOSの設定変更（`UserPreferenceChanged`）時に `ApplyAccentColorFix` を呼び出し、UIリソース（`AccentColor`, `ColorAccent` 等）を動的に更新するようにしました。

### 2. プリセット永続化の修正

「デフォルトプリセット」の設定がアプリ再起動時に保存されない問題を修正しました。

- **修正:** `MainWindow.xaml.cs` の `SaveSettingsActual` メソッドにおいて、`DefaultPresetId` を保存対象の `AppSettings` オブジェクトに含めるように修正しました。

## 検証結果

### ビルド確認

- [x] `dotnet build` にてエラーなく正常にビルドできることを確認しました。

### 修正のポイント

```csharp
// OSアクセントカラーの調整ロジック (ColorHelper.cs)
public static Color EnsureVisibleAccentColor(Color baseColor)
{
    ColorToHsv(baseColor, out double h, out double s, out double v);
    if (s < 0.6) s = 0.6; // 彩度を最低0.6に
    if (v < 0.7) v = 0.7; // 明度を最低0.7に
    return ColorFromHsv(h, s, v);
}
```

```csharp
// プリセット保存の修正 (MainWindow.xaml.cs)
var settings = new AppSettings 
{ 
    // ... 他の設定
    DefaultPresetId = DefaultPresetId // 保存漏れを修正
};
```
