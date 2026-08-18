# デバッグ用Pro切り替えスイッチの実装（厳密版）

デバッグ構成でのみコードが存在し、リリース構成には一切の影響を与えない形でPro版切り替えスイッチを実装しました。

## 実施内容

### 1. コードの条件付きコンパイル

`MainWindow.xaml.cs` において、`IsProDebugToggle` および `DebugModeVisibility` プロパティの **定義そのもの** を `#if DEBUG` ブロックで囲みました。これにより、Releaseビルドではこれらのプロパティはコンパイル後バイナリに存在しなくなります。

```csharp
#if DEBUG
    public bool IsProDebugToggle { ... }
    public Visibility DebugModeVisibility => Visibility.Visible;
#endif
```

### 2. XAMLバインディングの安全化

プロパティが存在しないReleaseビルドで実行時エラー（バインディング失敗）が発生してもUIが崩れないよう、XAML側で `FallbackValue` を設定しました。

- **修正前:** `Visibility="{Binding DebugModeVisibility}"`
- **修正後:** `Visibility="{Binding DebugModeVisibility, FallbackValue=Collapsed}"`

これにより、Releaseビルドではプロパティが見つからずバインディングが失敗しますが、`FallbackValue=Collapsed` が適用されるため、スイッチは非表示となります。

### 3. 視認性の改善

スイッチが見つけにくいというフィードバックに基づき、フォントスタイルを太字(`FontWeight="Bold"`)、色を文字色(`TextPrimary`)に変更し、背景色とのコントラストを確保しました。

## 検証結果

- [x] **Debugビルド:** プロパティが存在し、スイッチが表示・機能することを確認。
- [x] **Releaseビルド (想定):** プロパティが存在せず、`FallbackValue` によりスイッチが非表示になる設計であることを確認。
- [x] `dotnet build` が正常に完了することを確認。
