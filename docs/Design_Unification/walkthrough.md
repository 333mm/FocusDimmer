# ウォークスルー: Fluent Design の統一機構

本ドキュメントは、アプリケーション全体への統一された Fluent Design の適用について記述します。

## 変更内容

### 1. `FluentWindow` 基底クラスの作成

- `FocusDimmer.Components.FluentWindow` (Inherits `Window`) を作成しました。
- **機能**:
  - Windows 11 のモダンな背景効果 (Mica/Acrylic) を自動的に適用します。
  - 標準的なウィンドウコマンド (最小化、最大化、閉じる、元に戻す) を処理します。
  - カスタム Chrome をサポートするためのデフォルト動作を設定します (`WindowStyle="None"`, `ResizeMode="CanResize"`, `Background="Transparent"`).

### 2. グローバルリソース (`App.xaml`)

- スタイルとブラシを `MainWindow` から `App.xaml` に移行し、グローバルに利用可能にしました。
- `FluentWindowStyle` を定義しました:
  - カスタム `WindowChrome` 設定 (キャプション 32px)。
  - カスタムタイトルバーを含む一貫した `ControlTemplate`。
  - `Segoe Fluent Icons` を使用した標準化されたキャプションボタン。

### 3. ウィンドウのリファクタリング

すべてのアプリケーションウィンドウを `FluentWindow` を継承するように変更し、統一された `FluentWindowStyle` を適用しました。

| ウィンドウ | 変更点 | 結果 |
| :--- | :--- | :--- |
| **MainWindow** | `FluentWindow` を継承。新しいテンプレートに合わせて UI を再構築。 | 一貫した Acrylic 背景とタイトルバー。 |
| **ProcessSelectionWindow** | `FluentWindow` を継承。ローカルの Chrome/スタイルを削除。 | メインウィンドウと統一された外観。 |
| **InspectorActionDialog** | `FluentWindow` を継承。カスタムの境界線/影を削除。 | OS 標準の影と Acrylic 背景。 |
| **DebugInspectorWindow** | `FluentWindow` を継承。タイトルバーの対話操作を有効化。 | 移動/閉じる操作が容易に。 |
| **MigrationGuideWindow** | `FluentWindow` を継承。標準タイトルバーに切り替え。 | 独自の実装だったドラッグロジックを削除。 |
| **ColorPickerWindow** | `FluentWindow` を継承。カスタム Chrome を削除。 | 外観の標準化。 |

## 検証結果

### ビルド検証

- `dotnet build` は **エラー 0** で成功しました。
- `FluentWindow.cs` における `Brushes` のあいまいな参照を修正しました。

### 手動検証チェックリスト (ユーザーによる実施)

アプリケーションを起動し、以下の外観を確認してください:

- [ ] **Acrylic 効果**: すべてのウィンドウで半透明の Acrylic/Mica 効果が表示されることを確認。
- [ ] **タイトルバー**: すべてのウィンドウで共通のデザイン (高さ、フォント、ボタン) が適用されていることを確認。
- [ ] **対話操作**: すべてのウィンドウで最小化/閉じるボタンが機能することを確認。
- [ ] **ポップアップ**: 「プロセス選択」、「カラーピッカー」などを開き、視覚的な不具合なく描画されることを確認。
