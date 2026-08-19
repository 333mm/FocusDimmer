# 実装計画: タスクトレイ最小化オプション化 ＆ ディスプレイ設定チェーンリンク機能

ユーザー要望に基づき、以下の 2 つの新機能と UI 改善を実装します。

1. **タスクトレイに閉じるボタンの廃止 ＆ チェックボックスオプション化**:
   - サイドバー下部の専用ボタンを廃止し、一般設定（General）に「閉じるボタンでタスクトレイに最小化する」チェックボックスを追加。
2. **ディスプレイ設定のチェーンリンク（同時連動）機能**:
   - モニタタブの横に「チェーンリンク（🔗）」トグルボタンを追加。
   - リンクが ON のときは、いずれかのディスプレイで不透明度や色・マージン・動作設定を変更すると、すべてのディスプレイにリアルタイムで同時反映される。
   - リンクが OFF のときは、各ディスプレイが独立して個別設定できる。

---

## 提案する変更内容

### 1. タスクトレイ最小化オプションの実装
- **UI 変更**:
  - [MainWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml): サイドバー最下部の「タスクトレイに閉じる」ボタンを削除。
  - [MainWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml): General ページの「起動・動作設定カード」に `<CheckBox Content="{Binding Strings.CheckCloseToTray}" IsChecked="{Binding CloseToTray}"/>` を追加。
- **ロジック変更**:
  - [AppSettings.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Models/AppSettings.cs): `public bool CloseToTray { get; set; } = true;` を追加。
  - [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs): `OnClosing` ハンドラ内で、`CloseToTray == true` なら `this.Hide()` でトレイ格納、`false` ならそのままアプリを完全終了する。

---

### 2. ディスプレイ設定のチェーンリンク（連動同期）機能の実装
- **UI 変更**:
  - [MainWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml): モニタページのヘッダー右側に、スタイリッシュな **チェーンリンクトグルボタン（🔗）** を配置。
    - **ON（リンク時）**: アイコン `&#xE71B;`（チェーン接続）＋ アクセントハイライト ＋「全ディスプレイ連動」
    - **OFF（解除時）**: アイコン `&#xE7AC;`（チェーン切断）＋「個別設定」
- **ロジック変更**:
  - [AppSettings.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Models/AppSettings.cs): `public bool AreMonitorsLinked { get; set; } = false;` を追加。
  - [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs):
    - リンクボタン ON 時に、現在選択中のモニタ設定を全モニタへ一括同期。
    - 各モニタのプロパティ変更検知時（`SyncProfileToPreset`）、`AreMonitorsLinked == true` の場合は他の全モニタプロファイルにも同一値を即時反映・オーバーレイ更新。

---

### 3. 多言語対応（全 7 言語）
- [Languages/*.json](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Languages/):
  - `CheckCloseToTray`: 閉じるボタン（×）でタスクトレイに最小化する
  - `LabelLinkMonitors`: 全ディスプレイをリンク
  - `TooltipLinkMonitors`: 有効にすると、すべてのディスプレイの設定が同時に連動して変更されます。
- [LocalizationService.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Services/LocalizationService.cs): 対応プロパティの追加。

---

## 検証計画

### 1. ビルド検証
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Debug`
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Release`
- `dotnet build FocusDimmer2/FocusDimmer.csproj -p:DefineConstants="PRO" -c Release`

### 2. 動作確認
- **タスクトレイ動作**:
  - 「閉じるボタンでタスクトレイに最小化」が ON の時、右上の × ボタンでトレイに最小化されることを確認。
  - OFF の時、右上の × ボタンでアプリが完全に終了することを確認。
- **ディスプレイ設定リンク動作**:
  - 🔗 チェーンリンクを ON にした状態で、スライダー（不透明度やマージン等）やカラーを変更した際、Display 2 等の別タブの設定も同時に同一値に更新されることを確認。
  - 🔗 チェーンリンクを OFF にした際、モニタごとの個別設定が独立して変更・保存できることを確認。
