# 実装計画：デバッグオーバーレイとフェード修正

## 概要

- デスクトップクリック時にフェードアウトが効かない問題を修正。
- ユーザーが特定のアプリで画面が明るくなる原因を特定できるよう、デバッグ用の情報表示機能を追加。

## 変更内容

### [DimmerOverlay.cs]

- `UpdateState` で `DurationBrighten` が小さい場合の処理を調整し、フェードインがスキップされないように修正。
- ログにモニターデバイス名を追加し、デバッグ性を向上。

### [DebugOverlay.cs] (新規)

- マウス位置のウィンドウ情報を取得する透明な最前面ウィンドウ。
- `WindowFromPoint`, `GetClassName`, `GetWindowThreadProcessId` 等を使用。

### [MainWindow.xaml / MainWindow.xaml.cs]

- 設定画面に「デバッグ」トグルボタンを追加。
- ツールチップ用のTipsを UI に追加。

### [各国語.json]

- `TooltipAlwaysDark`, `BtnDebug`, `TooltipDebugMode` のキーを追加（7言語）。

## 検証計画

- デスクトップをクリックした際、滑らかに明るくなることを確認。
- デバッグモードをONにし、マウスを動かすとウィンドウ情報が追従して表示されることを確認。
- 他のモニターでも正常に動作することを確認。
