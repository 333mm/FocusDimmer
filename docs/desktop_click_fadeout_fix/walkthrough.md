# Walkthrough - Desktop Fadeout Fix

デスクトップをクリックして明るくなるときに、フェードアウトアニメーションが適用されない問題を修正しました。

## 根本原因

デスクトップクリック時、Windowsのフォーカス遷移が `IntPtr.Zero` → `DesktopHwnd` のように複数段階で発生することがあります。元のコードでは `FadeToTransparent` が `From` を指定せず現在のブラシ色からのアニメーションを期待していましたが、中間状態での再呼び出しによりアニメーションの開始点が不正になる場合がありました。

## Changes

### [DimmerOverlay.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Components/DimmerOverlay.cs)

render_diffs(file:///d:/Dev/FocusDimmer2/FocusDimmer2/Components/DimmerOverlay.cs)

**修正内容:**

- 状態遷移（暗→明）時に `From` を明示的に指定した `ColorAnimation` を作成
- アニメーション開始色を設定値（`Opacity` or `IdleDimOpacity`）から計算
- 中間状態での再呼び出しでも一貫したアニメーション動作を保証

## Manual Verification

1. アプリを起動し、ウィンドウをアクティブにして画面を暗くする
2. デスクトップをクリックする
3. 画面が設定した時間をかけてフェードアウトすることを確認する
