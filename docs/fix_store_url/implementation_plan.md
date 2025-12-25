# Store URL Fix Plan

## Goal

Fix the issue where clicking the purchase button opens an empty store page.

## Issue

The application attempts to open a specific Add-on Product Details Page (PDP) using the URL `ms-windows-store://pdp/?productid=9MWHG48NMCV0`.
However, if the add-on is configured as "Hidden in Store" (common for IAPs), this link resolves to an empty page or an error, providing a poor user experience when the in-app purchase flow fails (process logic fallback).

## Proposed Changes

### MainWindow.xaml.cs

- Modify the fallback logic in `OpenStore_Click`.
- Change the target URL from `UpgradeStoreUrl` (Add-on PDP) to `AppStoreUrl` (Main App PDP).
- The Main App PDP is reliable and allows users to access the app's store listing, where they might find the add-on or at least verify store connectivity.

## Verification

- Review code to ensure `AppStoreUrl` is used in the fallback block.
- Verify that `UpgradeStoreUrl` is no longer used in the fallback logic.
