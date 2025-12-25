# Task: Debug Store Purchase

- [x] Create error reporting mechanism
  - [x] Modify `StoreService.RequestPurchaseAsync` to return error details (Exception message) if possible, or handle it in MainWindow. <!-- id: 0 -->
- [ ] Update `MainWindow.xaml.cs` <!-- id: 1 -->
  - [ ] Show MessageBox with error details when purchase fails. <!-- id: 2 -->
  - [ ] Perform `GetStoreProduct` check before purchasing to verify Add-on visibility. <!-- id: 3 -->
- [ ] Verify changes <!-- id: 4 -->
