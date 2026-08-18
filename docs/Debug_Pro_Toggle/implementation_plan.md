# デバッグ用Pro切り替えスイッチの実装計画

デバッグ構成でビルドした際にのみ、ヘッダー部分でアドオン（Pro版）のアンロック状態を動的に切り替えられるスイッチを実装します。

## 変更内容

### [MainWindow Component]

#### [MODIFY] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)

- `IsPro` プロパティのセッターを `internal` に変更し、外部（デバッグ用プロパティ）からの変更を許可します。
- `DebugModeVisibility` プロパティを追加し、`#if DEBUG` 定数に基づいて表示/非表示を切り替えます。
- `IsProDebugToggle` プロパティを追加し、ここを介して `IsPro` の変更とモニターの再初期化（`InitializeMonitors`）を行います。

#### [MODIFY] [MainWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml)

- ヘッダーの言語選択 `ComboBox` の隣に、デバッグ専用の `CheckBox` を追加します。
- `Visibility` プロパティに `DebugModeVisibility` をバインドし、リリースビルドでは完全に隠れるようにします。

## 検証計画

### 動作確認

- デバッグ構成でビルドし、ヘッダーに "Pro (Debug)" チェックボックスが表示されることを確認。
- チェックを入れると、即座に "PRO" バッジが表示され、全モニターがディマー対象になることを確認。
- チェックを外すと、"FREE" バッジに戻り、メインモニター以外が非表示になることを確認。
- リリース構成でビルド（または表示設定を確認）し、スイッチが表示されないことを確認。
