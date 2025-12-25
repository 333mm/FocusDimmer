# ストアアドオン挙動の確認と修正

## 実施した変更

### 1. ライセンス検証ロジックの修正 (`StoreService.cs`)

- **問題**: アドオンの検証において、Store ID (`9MWHG48NMCV0`) を誤って `InAppOfferToken` プロパティと比較していました。
- **修正**: `StoreAppLicense.AddOnLicenses` コレクションのキー（Store ID）を使用してライセンスを確認するように変更しました。

```csharp
// Before
if (_appLicense.AddOnLicenses.Any(l => l.Value.InAppOfferToken == ProUpgradeAddOnId && l.Value.IsActive))

// After
if (_appLicense.AddOnLicenses.TryGetValue(ProUpgradeAddOnId, out var license) && license.IsActive)
```

### 2. 購入フローのUX改善 (`MainWindow.xaml.cs`, `StoreService.cs`)

- **問題**: ユーザーが購入ダイアログで「キャンセル」を選択した場合でも、強制的にブラウザでMicrosoft Storeの製品ページが開かれていました。
- **修正**:
  - `RequestPurchaseAsync` の戻り値を `bool` から `StorePurchaseStatus` に変更し、詳細な結果ステータスを返却するようにしました。
  - キャンセル (`NotPurchased`) の場合はストアページを開かないように条件分岐を追加しました。

```csharp
// MainWindow.xaml.cs
var status = await _storeService.RequestPurchaseAsync(handle);
// ...
else if (status != Windows.Services.Store.StorePurchaseStatus.NotPurchased)
{
    // 購入キャンセル以外の場合のみストアページを開く
    try { Process.Start(new ProcessStartInfo(UpgradeStoreUrl) { UseShellExecute = true }); } catch { }
}
```

## 検証結果

- **ビルド確認**: `FocusDimmer2` プロジェクトのビルドが成功することを確認しました。
- **ロジック確認**: `StorePurchaseStatus` を使用した制御フローが正しいことをコードレビューで確認済みです。
