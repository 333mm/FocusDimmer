# 実装計画 - 自動起動、通知、スリープ復帰の修正

## ゴール

パッケージ版アプリの自動起動オプションが機能しない問題を修正し、自動起動時にWindows通知を表示する仕組みを実装する。また、スリープ復帰時のアプリ動作の安定性を向上させる。

## ユーザーレビューが必要な事項
>
> [!IMPORTANT]
> 自動起動の修正には、`Package.appxmanifest` に `StartupTask` 拡張機能を追加する必要があります。これはストアアプリの標準的な手法ですが、パッケージ版での検証が必要です。
> 「自動起動時の通知」については、可能であれば `Windows.ApplicationModel.Activation.StartupTaskActivatedEventArgs` を検出して判断します。パッケージ化されていないバージョン（デバッグ時など）では、引数による判定（例：`/autostart`）をフォールバックとして使用します。

## 提案される変更

### 自動起動ロジック

#### [MODIFY] [Package.appxmanifest](file:///d:/Dev/FocusDimmer2/FocusDimmer.Package/Package.appxmanifest)

- `windows.startupTask` のための `uap5:Extension` を追加します。
- ID `FocusDimmerStartup` で `StartupTask` を定義します。

#### [MODIFY] [StartupManager.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Services/StartupManager.cs)

- `RegisterInRegistry`, `RemoveFromRegistry`, `IsRegisteredInRegistry` メソッドを更新し、パッケージ化された環境（Packaged）に対応させます。
- パッケージアプリとして実行されている場合は、`Windows.ApplicationModel.StartupTask` API を使用します。
- パッケージ化されていない環境（デバッグ時）のために、レジストリロジックは維持します（Unpackaged検出用に `/autostart` 引数を追加）。

### 通知と検出

#### [MODIFY] [App.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/App.xaml.cs)

- `OnStartup` をオーバーライドします。
- パッケージ版の場合は `StartupTask` アクティベーションの種類を確認し、非パッケージ版の場合は `/autostart` 引数を確認します。
- `IsAutoStart` ステートを保持します。

#### [MODIFY] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)

- コンストラクタまたは `Loaded` イベントで `App.IsAutoStart` を確認します。
- 自動起動の場合:
  - ウィンドウを非表示のまま開始（最小化状態で起動）します。
  - `NotifyIcon` のバルーンチップまたは Windows 通知を表示します：「Focus Dimmer はバックグラウンドで実行中です。」
- 手動起動の場合:
  - 通常通りウィンドウを表示します。

### スリープ / 復帰

#### [MODIFY] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)

- `SystemEvents_PowerModeChanged` を強化します。
- `_monitorTimer` が確実に再開されるようにします。
- オーバーレイが有効かつ最前面にあることを確認するチェックを追加します。

#### [MODIFY] [DimmerOverlay.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Components/DimmerOverlay.cs)

- `ForceResetTopmost` の実装を確認・検証します（スリープ後に `HWND_TOPMOST` が正しく処理されるようにする）。

## 検証計画

### 自動テスト

- なし（動作・UI変更のため）。

### 手動検証

1. **自動起動（パッケージ版）:**
   - `FocusDimmer.Package` プロジェクトをビルドしてデプロイします。
   - アプリ設定で「Windows起動時に実行」を有効にします。
   - タスクマネージャー > スタートアップ アプリ に "Focus Dimmer" が表示されることを確認します。
   - PCを再起動（またはサインアウト/サインイン）します。
   - アプリが自動的に起動することを確認します。
   - 通知が表示されることを確認します。
   - メインウィンドウが表示されない（トレイに常駐する）ことを確認します。

2. **自動起動（非パッケージ版 - デバッグ）:**
   - `/autostart` 引数を指定して実行ファイルを起動します。
   - 通知が表示され、ウィンドウが非表示であることを確認します。
   - 引数なしで実行します。
   - ウィンドウが表示され、通知が表示されないことを確認します。

3. **スリープ/復帰:**
   - アプリを実行します。
   - PCをスリープにします。
   - PCを復帰させます。
   - Dimmer（オーバーレイ）が依然としてアクティブで機能していることを確認します（フォーカスを切り替えてテスト）。
   - クラッシュやフリーズがないことを確認します。
