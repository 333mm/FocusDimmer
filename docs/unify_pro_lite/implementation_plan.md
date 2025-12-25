# Pro版とLite版の統合とIAP（アプリ内購入）の実装計画

Pro版（有料）とLite版（無料）を1つのアプリケーションに統合し、Microsoft Storeのアドオン機能（IAP: In-App Purchase）を利用してプロ版機能をアンロックする仕組みを導入します。また、既存のPro版購入者が引き続きプロ機能を利用できるような仕組み（救済策）として、同一パブリッシャーの旧Pro版のインストール状態を照会します。

## ユーザー提供情報（旧Pro版）

- **Microsoft Store ID**: `9N62D3G899G7`
- **Package Family Name (PFN)**: `sanmiri.FocusDimmer_p3b9zhm3nac6p`
- **Publisher**: `CN=8D2E4F19-4148-46A8-A15D-9F67D407A628`

## ユーザーレビューが必要な事項

- **IAPのプロダクトID**: ストア管理画面で作成されるアドオンのID（例: "pro_upgrade"）が必要です。現時点では仮のIDを使用します。

> [!IMPORTANT]
> この実装をテストするには、Microsoft Store に関連付けられたパッケージ ID が必要であり、開発中のローカル実行ではダミーデータやシミュレーターを使用する必要があります。

## 提案される変更

### [Core] 構成の整理

`FREE_VERSION` 定数に基づいたコンパイル時の切り分けを廃止し、実行時のフラグによって機能を制御するように変更します。

#### [MODIFY] [FocusDimmer.csproj](file:///d:/Dev/FocusDimmer2/FocusDimmer2/FocusDimmer.csproj)

- `Lite` および `Pro` 構成から `DefineConstants` (FREE_VERSION) を削除または整理
- マニフェストファイルを1つに統合（Lite版のIDをベースにするのが一般的）

### [Services] ストア連携

Microsoft Store と通信して購入状態を確認・更新するサービスを新規作成します。

#### [NEW] [StoreService.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Services/StoreService.cs)

- `Windows.Services.Store` を使用した購入状態の照会
- アドオン購入処理（`RequestPurchaseAsync`）の実装
- 旧Pro版アプリの所有権チェック（`PackageManager`等を使用したパッケージ検知）

### [UI] メインウィンドウ

購入状態に応じてUIを動的に変更し、未購入ユーザーにはアップグレードを促すUIを表示します。

#### [MODIFY] [MainWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml)

- プロ版機能をアンロックするためのボタン/バナーを追加
- 「Free Lite」などの固定表示を動的な表示に変更

#### [MODIFY] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)

- `IsFreeVersion` プロパティを `StoreService` の結果に基づく計算プロパティに変更
- プロ版機能（複数モニター対応、カスタムカラー、除外リスト等）の制限ロジックを非同期の購入確認後に適用

## 検証プラン

### 自動テスト / ストア検証

- `Windows.Services.Store` のシミュレーターを使用して、購入成功/失敗/キャンセル時の動作を確認

### 手動検証

1. アプリ起動時に「Lite版」として動作することを確認
2. 「Pro版にアップグレード」をクリックし、購入ダイアログが表示されることを確認
3. 購入後（模擬）、即座に複数モニター設定やカラー変更が有効になることを確認
4. （可能であれば）旧版アプリがインストールされている場合に自動でPro版として認識されるか確認
