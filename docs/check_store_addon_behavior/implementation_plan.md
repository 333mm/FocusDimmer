# Microsoft Store アドオン挙動の確認と修正

## 目標

Pro版アドオンの検証ロジックを修正し、購入時のユーザー体験を改善します。
現在の実装では、Microsoft Store ID (`9MWHG48NMCV0`) を `InAppOfferToken` プロパティと比較していますが、これは誤りです。`StoreId` と比較するか、ライセンスコレクションのキーとして使用する必要があります。
また、現在の購入フローでは、ユーザーがダイアログで「キャンセル」した場合でも外部のMicrosoft Storeページが開いてしまうため、これを修正します。

## ユーザーレビューが必要な事項
>
> [!IMPORTANT]
> `StoreService.RequestPurchaseAsync` の戻り値を `bool` から `StorePurchaseStatus` に変更します。これにより、UI側で「ユーザーによるキャンセル (`NotPurchased`)」と「システムエラー」を区別できるようになります。

## 変更内容

### FocusDimmer.Services

#### [MODIFY] [StoreService.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Services/StoreService.cs)

- `InitializeAsync` メソッドを修正し、`AddOnLicenses` のチェック時に `InAppOfferToken` ではなく `StoreId` (キーまたはプロパティ) を使用するように変更します。
- `RequestPurchaseAsync` の戻り値を `Task<bool>` から `Task<StorePurchaseStatus>` に変更します。
- 例外発生時は `StorePurchaseStatus.ServerError` 等のフォールバック値を返すようにします。

### FocusDimmer

#### [MODIFY] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)

- `OpenStore_Click` メソッドを修正し、`StorePurchaseStatus` の戻り値を処理するように変更します。
- ステータスが `Succeeded` (成功) または `AlreadyPurchased` (購入済み) でない場合のうち、`NotPurchased` (キャンセル) **以外** の場合のみ、外部ストアURL (`UpgradeStoreUrl`) を開くようにします。
- ユーザーがキャンセルした場合 (`NotPurchased`) は、何もせずにダイアログを閉じます（ブラウザを開きません）。

## 検証計画

### 手動検証

1. **コードロジックの確認**:
   - `StoreService.cs` の変更が正しいプロパティ (`StoreId`) を参照しているか確認します。
   - `MainWindow.xaml.cs` が `StorePurchaseStatus` を正しくハンドリングしているか確認します。
   - ※実際のストア非公開APIの動作確認には署名済みパッケージが必要なため、開発環境ではビルドが通り、ロジックとして正しいことを主眼に検証します。
