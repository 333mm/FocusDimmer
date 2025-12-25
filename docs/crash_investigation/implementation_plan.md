# 長時間放置時クラッシュ問題の実装計画

## ゴール

長時間（数時間〜数日）放置するとアプリケーションがクラッシュする問題を修正します。

## 発見された問題点

> [!CAUTION]
> **重大な問題**: `StartBreathSequence` メソッド内の `ColorAnimation.Completed` イベントに毎回匿名ハンドラを追加しており、アニメーション完了のたびにハンドラが蓄積します。これがメモリリークとイベント多重発火によるクラッシュの原因と考えられます。

### 1. アニメーション完了ハンドラの蓄積 (`DimmerOverlay.cs` 358行目)

```csharp
animBright.Completed += (s, e) => { ... }; // 毎回新しいハンドラを追加
```

- **問題**: ウィンドウ切り替えのたびに新しいイベントハンドラが追加される
- **影響**: 長時間使用でハンドラが数千〜数万個蓄積し、メモリ不足やスタックオーバーフローを引き起こす

### 2. PropertyChangedハンドラの未解除 (`DimmerOverlay.cs` 93行目)

```csharp
LinkedProfile.PropertyChanged += (s, e) => { ... };
```

- **問題**: オーバーレイを閉じてもハンドラが残り、古いオーバーレイへの参照が保持される

## 変更内容

### [MODIFY] [DimmerOverlay.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Components/DimmerOverlay.cs)

1. `StartBreathSequence` のアニメーション完了処理を、匿名ハンドラではなく専用メソッドに変更
2. PropertyChangedハンドラを保持し、Close時に解除するよう修正

### [MODIFY] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)

1. `InitializeMonitors` 内のPropertyChangedハンドラを管理可能な形式に変更

## 検証計画

- コード変更後、長時間稼働テスト（数時間）でクラッシュが発生しないことを確認
