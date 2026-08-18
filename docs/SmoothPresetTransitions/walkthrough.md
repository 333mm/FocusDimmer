# プリセット切替時のスムーズなフェード実装

プリセットを切り替えた際、変更後のアニメーション設定に従って、現在の不透明度から新しい不透明度へスムーズにフェードするように修正しました。

## 実施内容

### プリセット間フェードの実装

従来は、プリセットを切り替えた際、あるいはスライダーを動かした際、一度透明に戻るなどの不自然な挙動や、アニメーション設定が反映されない場合がありました。

- **修正箇所:** [DimmerOverlay.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Components/DimmerOverlay.cs) の `UpdateState` メソッド。
- **挙動の改善:**
  - プリセット切替（または不透明度変更）を検知した際、一度0%に戻すなどの処理を挟まず、現在の値から目標値まで直接アニメーションさせます。
  - 切り替え先プリセットの `DurationDarken`（暗くする場合）または `DurationBrighten`（明るくする場合）をフェード時間として使用します。
  - 最低 0.2秒のフェード時間を確保し、急激な変化にならずスムーズに見えるように調整しました。

## 検証結果

### ビルド確認

- [x] `dotnet build` にてエラーなく正常にビルドできることを確認しました。

### ロジックの確認

```csharp
// DimmerOverlay.cs 内の修正ロジック
if (opacityChanged && !windowChanged)
{
    // プリセット切替または不透明度変更
    _delayTimer.Stop();
    // 暗くなる場合は DurationDarken、明るくなる場合は DurationBrighten を使用
    double duration = (currentTargetOpacity > lastApplied) ? LinkedProfile.DurationDarken : LinkedProfile.DurationBrighten;
    if (duration < 0.2) duration = 0.2; // 最小限の滑らかさを確保
    FadeToDark(duration, currentTargetOpacity); // 目標値まで直接フェード
}
```
