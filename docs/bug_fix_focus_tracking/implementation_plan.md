# Implementation Plan - Fix Focus Tracking and PID Cache

The user reported that focus tracking for individual windows stops working after a long period of time, especially after toggling the "Display all windows brightly" option.
Research revealed that `ProcessInfoHelper` uses a static dictionary to cache PID-to-process-name mappings indefinitely. Since Windows reuses PIDs, this cache becomes stale over time, leading to incorrect identification of windows and causing them to be incorrectly excluded from dimming or highlighting.

## User Review Required

> [!IMPORTANT]
> The fix involves changing how process names are cached. This may slightly increase CPU usage during window switching, but it is necessary for correctness.

## Proposed Changes

### FocusDimmer.Helpers

#### [MODIFY] [ProcessInfoHelper.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Helpers/ProcessInfoHelper.cs)

- Implement a time-based or size-limited cache for PIDS, or simply clear it periodically.
- A better approach: store the `ProcessName` and `StartTime` to detect PID reuse.

### FocusDimmer

#### [MODIFY] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)

- Improve error handling in `MonitorTimer_Tick` to ensure one faulty overlay doesn't stop others from updating.
- Add periodic cache clearing for `ProcessInfoHelper`.

#### [MODIFY] [Components/DimmerOverlay.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Components/DimmerOverlay.cs)

- Ensure that toggling `DimDesktopOnly` immediately clears all cached hole states.

## Verification Plan

### Automated Tests

- No automated tests currently exist. I will verify the fix manually.

### Manual Verification

1. Run the application.
2. Toggle "Display all windows brightly" ON and OFF.
3. Verify that the focused window is correctly highlighted in "Normal" mode.
4. Verify that ALL windows are bright in "Display all windows brightly" mode.
5. (Simulated long-term) Manually clear the cache or observe that PID reuse doesn't break tracking if the cache is invalidated.
