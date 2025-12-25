# Final Cleanup Plan

## Goal

Simplify the purchase flow by removing the error-prone "Pre-check" logic.
Since `RequestPurchaseAsync` works correctly (as confirmed by the user), we don't need to call `GetStoreProductsAsync` beforehand.

## Proposed Changes

### MainWindow.xaml.cs

- Remove the call to `_storeService.GetAddOnProductAsync`.
- Remove the diagnostic MessageBox.
- Directly call `_storeService.RequestPurchaseAsync` when the button is clicked.
- Keep the error handling for `RequestPurchaseAsync` (displaying the error if the *purchase* fails), but simpler.

### StoreService.cs

- Remove `GetAddOnProductAsync` method.
- Remove `StoreDiagnosticInfo` class.
- Keep `RequestPurchaseAsync` using `WinRT.Interop.InitializeWithWindow`.

## Verification

- User will click the button.
- No "Store Debug Info" popup will appear.
- The Microsoft Store purchase dialog will open immediately.
- If the user cancels or it fails really, a simple error message will appear (optional, or just silent as before if preferred, but keeping rudimentary error visibility is good).
