# ウォークスルー: Windows 11 設定アプリ風モダン Fluent UI デザイン刷新

Windows 11 の標準「設定」アプリ（Windows Settings）や WinUI 3 / Fluent Design System に準拠した、統一感のあるモダンでおしゃれな UI デザインへの刷新が完了しました。

---

## 主な刷新内容

### 1. Windows 11 Fluent カラーパレット ＆ レイヤード背景
- **ウィンドウ背景**: 落ち着いた深い Mica ダークトーン（`#1E1E1E`）に統一。
- **カードレイヤー**: WinUI 3 標準のレイヤードカード背景（`#0EFFFFFF`）、繊細な光彩ボーダー（`#18FFFFFF`）と角丸 `8px`。
- **サイドバー**: 透過トーン＋右側の繊細なセパレーター境界線。

---

### 2. Windows 11 風 ToggleSwitch（トグルスイッチ）スタイルの導入
- [App.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/App.xaml):
  - カプセル型の Windows 11 トグルスイッチ（`ToggleSwitchStyle`）を新設。
  - ON 時にシステムのアクセントカラー＋高コントラストな黒つまみ、OFF 時に透過＋繊細なアウトライン枠線＋グレーつまみ。
- [MainWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml):
  - **General ページ**: 「Windows 起動時に自動開始」「管理者権限で実行」「閉じる（×）でトレイに最小化」の各設定を行＋トグルスイッチのレイアウトに刷新。
  - **Monitors ページ**: 「デスクトップのみ減光」「非アクティブ時に全体を減光」「アイドル時に減光」を行＋トグルスイッチのレイアウトに刷新。

---

### 3. 各設定ページ（Exclusions, Shortcuts, Presets）の洗練
- **Exclusions ページ**: 角丸 `6px` のテキストボックスと操作ボタンを統一。
- **Shortcuts ページ**: キーバインド記録ボックス（`150x32`、角丸 `6px`、中央揃え）と行レイアウトの整列。
- **Presets ページ**: プロセスルールタグをモダンなピルバッジデザイン（角丸 `14px`）に統一。

---

## 検証結果

- **Lite 構成（フリー版固定）**: ビルド成功（エラー: 0, 警告: 0）
- **Release 構成**: ビルド成功（エラー: 0, 警告: 0）
- **Pro 構成（PRO版固定）**: ビルド成功（エラー: 0, 警告: 0）
