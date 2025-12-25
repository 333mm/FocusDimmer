# Verification Guide Plan

## Goal

Provide a clear guide on how to verify "Pro" features while debugging, since direct Store interaction can be tricky in debug mode (different identities, licenses, etc.).

## Content to Create

### `docs/debug_guide/verification_guide.md`

1. **Prerequisite**: Use `FocusDimmer.Package` as the startup project.
    - Essential because accessing `Package.Current` or `StoreContext` crashes in pure Desktop mode.
2. **Testing "Pro Purchase" State**:
    - Temporarily modifying line 74 of `StoreService.cs` to force `_isProSubscribed = true`.
3. **Testing "Legacy Pro" Detection**:
    - Temporarily modifying line 107 of `StoreService.cs` or adding a dummy checking logic to simulate finding the legacy app.
4. **Testing Actual Buying Flow**:
    - Note that this requires the developer account to be linked and might require a Release build or specific association in Visual Studio, which is hard to guarantee. Recommend "Forced Pro" for feature testing.

## Execution

- Create the markdown file.
- Notify the user.
