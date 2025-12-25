# Purchase Debugging Plan

## Goal

Diagnose why `RequestPurchaseAsync` fails in the production environment.
Provide error feedback to the user instead of silently falling back to the store page.

## Proposed Changes

### StoreService.cs

- Update `RequestPurchaseAsync` to return a detailed result tuple `(StorePurchaseStatus Status, Exception Error)` instead of just `StorePurchaseStatus`.
- Add a new method `GetAddOnProductAsync()` to verify if the Add-on is visible to the user before attempting purchase.

### MainWindow.xaml.cs

- Update `OpenStore_Click`:
    1. Call `GetAddOnProductAsync`. If null, alert the user that the product info could not be fetched (likely Store config or Network issue).
    2. Call `RequestPurchaseAsync`.
    3. If status is `ServerError` or `SettingsNotSupported`, show the Exception message in a `MessageBox`.
    4. Only fallback to `Process.Start(AppStoreUrl)` if the user acknowledges the error or if it's a specific revocable error.

## Verification

- User will run the updated app.
- If it fails, they will see a popup with "Error: 0x..." or "Product not found".
- This allows distinguishing between "Add-on ID wrong", "Store Context missing", or "User not signed in".
