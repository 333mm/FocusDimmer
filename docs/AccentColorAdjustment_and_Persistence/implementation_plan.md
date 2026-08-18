# OSアクセントカラー調整とプリセット永続化の実装計画

OSのアクセントカラーが視認しにくい場合に自動調整する機能の追加と、プリセット設定（デフォルトプリセット、プロセス関連付け）の永続化バグを修正します。

## 提案される変更

### [Component] Core / Models

#### [MODIFY] [AppSettings.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Models/AppSettings.cs)

- `DefaultPresetId` が `SaveSettingsActual` で保存対象に含まれるように、モデルとしての整合性を確認します（既に存在しますが、保存処理で使われていません）。

### [Component] UI / Main Window

#### [MODIFY] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)

- `SaveSettingsActual` メソッドを修正し、`DefaultPresetId` も保存するようにします。
- `Loaded` イベントおよびシステム設定変更時に、OSのアクセントカラーを取得し、彩度 (Saturation) や明度 (Value/Brightness) が低い場合に引き上げる処理を追加します。
- `AdjustAccentColor` メソッドを実装し、`Application.Resources["AccentColor"]` を動的に更新します。

### [Component] Helpers

#### [NEW] [ColorHelper.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Helpers/ColorHelper.cs)

- RGBとHSVの相互変換を行うユーティリティ。アクセントカラーの調整（彩度・明度のブースト）に使用します。

---

## 検証計画

### 自動テスト

- なし（UIおよびシステム設定に依存するため）

### 手動確認

1. **アクセントカラーの調整**
   - Windowsの設定でアクセントカラーを非常に暗い色やグレーに近い色に変更する。
   - アプリを起動（または再起動）し、UI（Badgeやボタン、スライダーなど）のアクセントカラーが視認性の高い色に補正されていることを確認する。
2. **プリセットの永続化**
   - プリセットを複数作成する。
   - 特定のプリセットを「デフォルト」に設定する。
   - アプリを再起動し、デフォルト設定が維持されていることを確認する。
   - プロセス関連付け（Manage Process Rules）を設定し、再起動後もルールが残っていることを確認する。
