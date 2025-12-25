# 不具合修正: Dialogウィンドウのアクティブ時限定表示

## 現状の分析

- `MonitorTimer` 内の `IsIgnoredWindow` 判定が `#32770` を正しく認識できていない（または何らかの理由で無視されている）ため、フォーカス追従による穴開けが機能していない。
- `IsAlwaysBrightWindow` に追加すると「常に明るい（背景でも明るい）」状態になるが、ユーザーはこれを望んでいない。

## 解決策

- `#32770` を `IsAlwaysBrightWindow` (ホワイトリスト) に再度追加します。これにより、監視ループからの認識対象とします。
- **副作用の防止**: `UpdateHoles` メソッド内で、 `#32770` ウィンドウに対しては「現在のアクティブウィンドウ (`GetForegroundWindow`) と一致する場合のみ」穴を開けるように制限します。
- これにより、`IsIgnoredWindow` の複雑なフィルタリングをバイパスしつつ、「アクティブな時だけ明るくする」という通常のウィンドウと同じ挙動を実現します。

## 変更内容

### [MODIFY] [Components/DimmerOverlay.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Components/DimmerOverlay.cs)

- `IsAlwaysBrightWindow` に `#32770` を追加。
- `UpdateHoles` 内のループで、`isBrightClass` が true でも、`IsDialogWindow(hwnd)` が true の場合は、`NativeMethods.GetForegroundWindow() != hwnd` なら `isBrightClass = false` に強制変更するロジックを追加。

## 検証計画

- プロパティ画面をクリック（アクティブ化）すると明るくなること。
- 別のウィンドウ（ブラウザなど）をアクティブにすると、プロパティ画面が暗くなること。
