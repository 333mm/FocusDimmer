# ウォークスルー: FocusDimmer2 徹底コードレビュー & リファクタリング完了

FocusDimmer2 の全ソースコードに対する徹底レビューに基づき、God Class（1,697行）化していた `MainWindow.xaml.cs` の解体、MVVM パターンの導入、コア減光エンジンや各サービスの抽出・独立化リファクタリングがすべて完了しました。

---

## 🏗️ リファクタリングによるアーキテクチャの進化

### 1. 共通サービス層の新設
- [SettingsService.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Services/SettingsService.cs):
  - 設定ファイルの非同期読み込み・デバウンス保存・安全なフォールバック管理を一元化。
- [HotkeyService.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Services/HotkeyService.cs):
  - Win32 API（`RegisterHotKey` / `UnregisterHotKey`）と `HwndSource` メッセージフック（`WM_HOTKEY`）の登録・監視をカプセル化。
- [PresetService.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Services/PresetService.cs):
  - プリセットの全モニター適用、プロファイル変更時の同期、リンクモードでの伝播、およびアクティブプロセス名からのルールマッチングを独立ロジック化。

### 2. コア減光エンジンの分離
- [DimmerEngine.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Services/DimmerEngine.cs):
  - `MainWindow.xaml.cs` に埋め込まれていた約240行の長大なポーリング監視ループを独立クラスへ抽出。
  - ウィンドウ追跡、可変フレームレート制御（15ms/100ms）、除外判定、タスクバー・PiP判定、アイドル検出、オーバーレイ制御を純粋なサービスクラスとしてカプセル化。

### 3. MVVM パターンの完全導入
- [MainViewModel.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/ViewModels/MainViewModel.cs):
  - プロパティ（`IsPro`, `MonitorProfiles`, `GlobalPresets`, `Strings`, etc.）、コマンド（`SetAsDefaultCommand`, `RemoveProcessRuleCommand`）、多言語切り替え、バージョン管理を統括。
- [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs):
  - 1,697行の肥大化したコードから、純粋な View（ウィンドウライフサイクル・イベントルーティング・ダイアログ呼び出しのみ）へ大幅スリム化。

---

## 🧪 ビルド検証結果

| ビルド構成 | 結果 | エラー | 警告 |
| :--- | :--- | :--- | :--- |
| **Lite** (フリー版固定) | ✅ 成功 | 0 | 0 |
| **Debug** | ✅ 成功 | 0 | 0 |
| **Release** | ✅ 成功 | 0 | 0 |
| **Pro** (PRO版固定) | ✅ 成功 | 0 | 0 |
