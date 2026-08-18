# フェードアウトアニメーション動作不具合の調査と修正 実装計画

ウィンドウ切り替え時や非減光状態への移行時に、フェードアウト（暗い状態から明るく抜けるアニメーション: `DurationBrighten`）が正常に動作しない問題の調査と修正計画です。

---

## 原因の特定

`Components/DimmerOverlay.cs` のアニメーション制御において、以下の複数の問題が重なって発生していました：

1. **`ColorAnimation.From` の未指定による補間失敗**:
   - `StartBreathSequence` および `UpdateState` で `ColorAnimation` を生成する際、`To = Color.FromArgb(0, 0, 0, 0)` のみが指定され、`From`（現在のブラシカラー）が指定されていませんでした。
   - WPF のアニメーション仕様により、直前の `FillBehavior.HoldEnd` が残っている場合、`From` が未指定だと現在値からのスムーズな減衰が行われず、一瞬で透明になるかアニメーションがスキップされていました。

2. **`shouldDim == true` 継続時の `windowChanged` 判定バグ**:
   - ウィンドウが別のウィンドウに切り替わった際、`DurationBrighten <= 0.05` だと即座に `targetAlpha`（真っ暗な色）を直接ブラシに代入しており、フェードアウトもディレイもフェードインもすべて無視されて真っ暗になっていました。

3. **手動タイマー（`FadeTimer_Tick`）と WPF アニメーション（`BeginAnimation`）の競合**:
   - 一部で手動 `DispatcherTimer` によるフェード処理が残っており、`BeginAnimation` と競合してアニメーションが中断されていました。

---

## 修正方針

1. **`ColorAnimation` に現在のブラシカラー（`From`）を常に明示**:
   - フェードアウト開始時、現在表示されている不透明度から `0`（完全透明）への補間を確実に実行します。
2. **アニメーション遷移（ブレスシーケンス）の正常化**:
   - **ウィンドウ切替時**:
     1. `DurationBrighten > 0` の場合: 現在の暗さから `DurationBrighten` 秒かけて完全透明へフェードアウト。
     2. フェードアウト完了後、`DelayDarken` 秒待機。
     3. `DurationDarken` 秒かけて目標不透明度へフェードイン。
   - **非減光移行時（デスクトップ選択・除外アプリ選択）**:
     - 現在の暗さから `DurationBrighten` 秒かけて完全透明へフェードアウト。
3. **不要な手動タイマーの廃止と WPF アニメーションパイプラインの一元化**:
   - アニメーションの競合やフレーム飛びを解消し、滑らかで美しいフェード効果を実現します。

---

## 変更対象ファイル

- [FocusDimmer2/Components/DimmerOverlay.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Components/DimmerOverlay.cs)

---

## 検証計画

### 1. ビルド検証
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Debug`
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Release`
- `dotnet build FocusDimmer2/FocusDimmer.csproj -p:DefineConstants="PRO" -c Release`

### 2. 動作確認
- フェードアウト時間（`DurationBrighten`）を 1.0秒などに設定し、ウィンドウを切り替えた際、滑らかに明るくなってから待機後、滑らかに暗くなることを確認。
- デスクトップをクリックした際、滑らかに明るく抜けることを確認。
