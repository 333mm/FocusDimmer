# Walkthrough - Legacy Detection Fix

The issue where the Legacy Pro version was not being detected has been addressed by making the detection logic more robust.

## Changes

### StoreService.cs

Modified `CheckLegacyProInstalled` to search for **any** package from the same publisher (sanmiri) that contains "FocusDimmer" in its name, excluding the current app itself.
This eliminates the reliance on a hardcoded and potentially incorrect Package Family Name (PFN).

```csharp
// Logic Overview:
// 1. Get all packages for the user.
// 2. Filter by PublisherId "p3b9zhm3nac6p" (sanmiri).
// 3. Ignore the current package.
// 4. If any remaining package has "FocusDimmer" in its name, consider it the Legacy Pro version.

var packages = manager.FindPackagesForUser("");
foreach (var pkg in packages)
{
    if (pkg.Id.PublisherId.ToLower() == "p3b9zhm3nac6p")
    {
        if (pkg.Id.FamilyName == currentPfn) continue;
        if (pkg.Id.Name.Contains("FocusDimmer"))
        {
            return true;
        }
    }
}
```

## Verification

- This logic will correctly identify `sanmiri.FocusDimmerPro`, `sanmiri.FocusDimmer`, or any other variation installed by the same publisher.
- Ensure that the app has the necessary capabilities (`runFullTrust` allows this) to query package information.
