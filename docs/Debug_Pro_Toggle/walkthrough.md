# デバッグ用Pro切り替えスイッチの実装完了

デバッグビルド時のみ使用可能な、アドオン機能のアンロック状態を切り替えるスイッチを実装しました。これにより、開発中のPro版機能のテストが容易になります。

## 実施内容

### 1. デバッグ専用プロパティの追加

`MainWindow.xaml.cs` に、ビルド構成に依存するプロパティを追加しました。

- `DebugModeVisibility`: `#if DEBUG` の場合にのみ `Visible` を返します。
- `IsProDebugToggle`: このプロパティを切り替えると `IsPro` 状態が更新され、モニターの再スキャン（無料版の制限解除など）が行われます。

### 2. UIへのスイッチ配置

`MainWindow.xaml` のヘッダー右側、言語選択の隣にスイッチを追加しました。

```xml
<CheckBox Content="Pro (Debug)" 
          IsChecked="{Binding IsProDebugToggle}" 
          Visibility="{Binding DebugModeVisibility}" ... />
```

### 3. 動作の確認

- **デバッグビルド:** チェックボックスが表示され、チェックのON/OFFで即座にPro/Free状態が切り替わることを確認しました。
- **リリースビルド:** スイッチが非表示になり、通常のライセンスチェックロジックが優先されます。

## 検証結果

- [x] デバッグビルドでのスイッチ動作確認済み。
- [x] `dotnet build` によるコンパイルエラーがないことを確認済み。
