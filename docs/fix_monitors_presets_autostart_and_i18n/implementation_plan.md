# モニタ個別設定・プリセット反映・全言語ローカライズ・自動起動の修正 実装計画

ご指摘いただいた以下の4つの課題について、根本原因の特定と改修を行います。

---

## 1. モニタごとの設定変更が複数モニタで同時に変更される問題の修正

### 原因
- `MainWindow.xaml.cs` の `SyncProfileToPreset` メソッド内で、あるモニタの設定（不透明度やマージン等）が変更された際に `PropagateToOtherMonitors(...)` が呼び出され、他の全モニタへ同一設定値が強制上書きされていました。

### 改修内容
- モニタタブからの設定変更時は、そのモニタ（`MonitorProfile`）のみのプロパティを更新・保存するように修正し、`PropagateToOtherMonitors` による不正同期を排除します。
- 各モニタが独立した不透明度・マージン・減光色・動作設定を保持できるようにします。

---

## 2. プリセットが反映されていない問題の修正

### 原因
- `SelectedGlobalPresetId` の変更が ComboBox の UI イベント頼みになっており、コード上でのプリセット選択時やプロセス検知タイマーからの切替時に `ApplyGlobalPresetToAllMonitors` が確実に発火していませんでした。
- プリセット適用フラグ（`_isApplyingPreset`）とプロパティ変更通知の競合。

### 改修内容
- `SelectedGlobalPresetId` の setter で即座に `ApplyGlobalPresetToAllMonitors` を呼ぶように一本化。
- プリセットの切替およびプロセス連動ルールによる自動切替が全モニタの表示・オーバーレイに即時反映されるよう修正します。

---

## 3. ローカライズが行われていない箇所の修正（全言語）

### 現状
- `MainWindow.xaml` にハードコードされた英語文字列が残存（"Advanced Behavior", "Configure keyboard shortcuts...", "Language / 言語", "Choose display language", "App startup, behavior and language settings.", "Migration Info" 等）。

### 改修内容
- 不足している以下のキーを全 7 言語ファイル（`ja.json`, `en.json`, `zh.json`, `de.json`, `es.json`, `fr.json`, `pt.json`）に追加：
  - `HeaderAdvancedBehavior`
  - `SubHeaderHotkeys`
  - `HeaderLanguage`
  - `SubHeaderLanguage`
  - `SubHeaderGeneral`
  - `BtnMigrationInfo`
  - `MsgStartupDisabledByUser` (自動起動が無効化されている際の案内メッセージ)
- `MainWindow.xaml` のハードコードをすべて `{Binding Strings.KeyName}` に置き換え、言語切り替えで即座に反映されるようにします。

---

## 4. 自動起動設定の最適化（Windows Store アプリ用）

### 現状と課題
- `Package.appxmanifest` には `desktop:StartupTask TaskId="FocusDimmerStartup"` が定義されているものの、ユーザーがタスクマネージャー等で無効化した（`StartupTaskState.DisabledByUser`）場合に、アプリ側でチェックを入れても無言で元に戻るため理由が不明瞭でした。

### 改修内容
- `StartupManager.cs`:
  - `StartupTaskState.DisabledByUser` や `DisabledByPolicy` の状態を詳細に検知。
  - ユーザーが無効化している場合は「Windowsの設定（スタートアップアプリ）で有効化してください」という案内ダイアログを表示し、必要に応じて設定画面（`ms-settings:startupapps`）を開くように改善します。

---

## 変更予定ファイル一覧

1. [FocusDimmer2/MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)
   - `PropagateToOtherMonitors` の廃止、モニタ個別設定の独立性確保
   - `SelectedGlobalPresetId` のプリセット反映強化
2. [FocusDimmer2/MainWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml)
   - ハードコード文字列の `{Binding Strings.*}` 置換
3. [FocusDimmer2/Languages/*.json](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Languages/) (ja, en, zh, de, es, fr, pt)
   - 新規多言語キーの追加
4. [FocusDimmer2/Services/StartupManager.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Services/StartupManager.cs)
   - MSIX StartupTask 状態判定とエラーハンドリングの強化

---

## 検証計画

### 1. ビルド検証
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Debug`
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Release`
- `dotnet build FocusDimmer2/FocusDimmer.csproj -p:DefineConstants="PRO" -c Release`

### 2. 動作・機能検証
- モニタタブを切り替えて設定を変更した際、選択中モニタのみが変更され、他のモニタが勝手に書き換わらないことを確認。
- プリセットを変更した際、各モニタの不透明度・設定が即座に切り替わることを確認。
- 言語を英語、日本語、中国語等に切り替えた際、全画面のすべての項目が翻訳されることを確認。
- 自動起動の切り替え処理が安全・確実に動作することを確認。
