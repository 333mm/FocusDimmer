# Implementation Plan - Fix Desktop Fadeout Animation (Refined)

前回の修正では、デスクトップ（または`IntPtr.Zero`）への遷移時に `BeginAnimation` のHandoff動作（ SnapshotAndReplace ）が期待通りに動作せず、ベース値（透明）が採用されて即座に明るくなってしまう現象が疑われます。
これを防ぐため、デスクトップへの遷移時（`windowChanged`検出時）には明示的に「現在の設定された不透明度（Dark）」から「透明」へのアニメーションを開始するようにします。

## Problem Check

- `FadeToTransparent` は `From` を指定せず、現在のプロパティ値からの遷移を期待している。
- しかし、暗転アニメーション（`FadeToDark`）が完了または中断された際、`SolidColorBrush.Color` のベース値が `#00000000` (Transparent) になっている（`UpdateState`内の初期化ロジック等による）可能性がある。
- このため、デスクトップ遷移時（暗→明）に手動でアニメーションをトリガーしても、透明→透明 のアニメーションとなり、視覚的には「即座に明るく」なってしまう。

## Proposed Changes

### FocusDimmer2

#### [MODIFY] [DimmerOverlay.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Components/DimmerOverlay.cs)

- 前回追加した修正ブロック内の `FadeToTransparent` 呼び出しを、`From` を明示的に指定したアニメーション生成ロジックに置き換えます。
- `From` の色は、その時点でのターゲット不透明度（`Opacity` または `IdleDimOpacity`）に基づいて算出します。

```csharp
                    // 修正: 既に明るい状態と判定されていても、ウィンドウが切り替わった場合は
                    // 強制的にフェードアウトアニメーションを要求する。
                    if (windowChanged && !isIdle && LinkedProfile.DurationBrighten > 0)
                    {
                         // 現在の不透明度設定を取得（アイドル状態からの復帰ならIdleOpacity、そうでなければ通常Opacity）
                         double fromOp = (_wasIdle && LinkedProfile.DimWhenIdle) ? LinkedProfile.IdleDimOpacity : LinkedProfile.Opacity;
                         byte fromAlpha = (byte)(fromOp / 100.0 * 255);
                         var c = GetBaseColor();
                         Color fromColor = Color.FromArgb(fromAlpha, c.R, c.G, c.B);

                         // 明示的に From を指定してアニメーションを開始する
                         var anim = new ColorAnimation
                         {
                             From = fromColor,
                             To = Color.FromArgb(0, 0, 0, 0),
                             Duration = new Duration(TimeSpan.FromSeconds(LinkedProfile.DurationBrighten)),
                             FillBehavior = FillBehavior.HoldEnd
                         };
                         _brush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
                    }
```

## Verification Plan

### Manual Verification

1. アプリを起動し、ウィンドウをアクティブにして画面を暗くする。
2. デスクトップをクリックする。
3. 画面が「パッと」明るくなるのではなく、設定した時間をかけてフェードアウトすることを確認する。
4. `IntPtr.Zero` 経由での遷移による「アニメーションの再始動（瞬き）」が許容範囲内かどうかも併せて確認する（ロジック上、多少の再始動が発生しうるが、スキップされるよりは良い）。
