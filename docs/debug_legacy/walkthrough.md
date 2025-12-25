# Walkthrough - Legacy Pro Detection Fix (Debug)

We have enabled the legacy detection logic to run in Debug mode for verification.

## Problem

Previously, `InitializeAsync` (which contains the legacy check logic) was skipped in Debug builds because `IsPro` was being hardcoded based on build configurations.

## Fix

Modified `MainWindow.xaml.cs` to execute `InitializeAsync` even in the standard Debug (Lite) configuration.

- It logs `[Debug] Force running InitializeAsync for testing...`.
- If a legacy app is detected, it will correctly upgrade the status to `IsPro = true`.

## Verification Steps

1. **Start Debugging**: Run `FocusDimmer.Package` (F5).
2. **Check Output**: Look at the Output window for logs starting with `[StoreService]`.
    - You should see `InitializeAsync called`.
    - Then `Checking legacy installation...`.
    - Finally `[LegacyCheck] ...` logs indicating if it found the app.
3. **Verify UI**: If `Legacy Pro Detected!` appears in the log, the app title should lose the "(Free Lite)" suffix (or the Pro badge should appear).

## Note

This change is safe for Debug builds but ensure `IsPro` logic is correct for Release builds (which was already correct in the `#else` block).
