# Walkthrough - Store URL Fix

The issue regarding the empty store page has been resolved by updating the fallback URL strategy.

## Changes

### MainWindow.xaml.cs

Updated the `OpenStore_Click` event handler handling the fallback scenario (when `RequestPurchaseAsync` fails or returns an error).
Previously, it attempted to open the Add-on's direct Store URL. Since the add-on is likely hidden, this resulted in an empty page.
Now, it redirects to the Main Application's Store URL, ensuring the user lands on a valid page.

```csharp
// Before
// try { Process.Start(new ProcessStartInfo(UpgradeStoreUrl) { UseShellExecute = true }); } catch { }

// After
// アドオンの直接リンクは非公開の場合に空ページになるため、メインのストアページを開く
try { Process.Start(new ProcessStartInfo(AppStoreUrl) { UseShellExecute = true }); } catch { }
```

## Verification Results

- Verified that the fallback logic now uses `AppStoreUrl` (Ends with `...79X`) instead of the Add-on ID.
- Confirmed that the code handles the `ServerError` or connection failures by opening this valid URL.
