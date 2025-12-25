# Legacy Detection Fix Plan

## Goal

Reliably detect the installed "Legacy Pro" application to unlock Pro features in the Unified app.

## Issue

The current implementation checks for a specific Package Family Name (PFN) `sanmiri.FocusDimmer_p3b9zhm3nac6p`.
The user reports detection fails even with the legacy app installed.
This suggests the PFN might be different or strict PFN matching is insufficient.

## Proposed Changes

### StoreService.cs

- Modify `CheckLegacyProInstalled` to:
    1. Retrieve all packages for the current user.
    2. Filter packages that match the publisher ID/Name (`sanmiri` or `CN=8D2E4F19...`).
    3. Look for packages that are NOT the current package.
    4. Specifically check for `sanmiri.FocusDimmerPro` or other variations.
    5. Allow partial matching or checking multiple known PFNs if necessary.
- Added debugging/logging (commented out or via `Debug.WriteLine`) could be useful, but since we can't see the user's debug output easily, relying on broader search logic is better.

## Verification

- Code review to ensure logic searches correctly.
- If possible, ask user to verify if it works.
