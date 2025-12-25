# 長時間放置時クラッシュ修正の実施結果

## 変更内容

### [DimmerOverlay.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Components/DimmerOverlay.cs)

**問題:** `StartBreathSequence` メソッド内で毎回新しい `ColorAnimation` を作成し、`Completed` イベントに匿名ハンドラを追加していました。ウィンドウ切り替えのたびにハンドラが蓄積し、長時間使用でクラッシュを引き起こしていました。

**修正:**

1. `_breathAnimation` フィールドを追加してアニメーションオブジェクトを再利用
2. 匿名ハンドラを `BreathAnimation_Completed` メソッドに置き換え
3. アニメーションは最初の1回のみ作成され、以降は再利用される

```diff
+        private ColorAnimation _breathAnimation;

         private void StartBreathSequence()
         {
             _delayTimer.Stop();
             double durationBright = LinkedProfile.DurationBrighten;
-            var animBright = new ColorAnimation { ... };
-            animBright.Completed += (s, e) => { ... };  // 毎回追加！
-            _brush.BeginAnimation(SolidColorBrush.ColorProperty, animBright);
+            if (_breathAnimation == null)
+            {
+                _breathAnimation = new ColorAnimation { ... };
+                _breathAnimation.Completed += BreathAnimation_Completed;  // 1回のみ
+            }
+            _breathAnimation.Duration = ...;
+            _brush.BeginAnimation(SolidColorBrush.ColorProperty, _breathAnimation);
         }

+        private void BreathAnimation_Completed(object? sender, EventArgs e)
+        {
+            _delayTimer.Interval = TimeSpan.FromSeconds(LinkedProfile.DelayDarken);
+            _delayTimer.Start();
+        }
```

## 効果

- イベントハンドラの蓄積によるメモリリークを防止
- 長時間稼働時の安定性が向上
