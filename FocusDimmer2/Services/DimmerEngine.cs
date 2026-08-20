using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Threading;
using FocusDimmer.Components;
using FocusDimmer.Helpers;
using FocusDimmer.Models;

namespace FocusDimmer.Services
{
    public class DimmerEngine : IDisposable
    {
        private readonly List<DimmerOverlay> _overlays;
        private readonly DispatcherTimer _monitorTimer;

        private bool _isEnabled = true;
        private IntPtr _lastForegroundWindow = IntPtr.Zero;
        private NativeMethods.RECT _lastRectForMotion = new NativeMethods.RECT();
        private int _highSpeedFrames = 0;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                if (!_isEnabled)
                {
                    foreach (var ov in _overlays) ov.SetVisibility(false);
                }
            }
        }

        public event Action<string>? ActiveProcessChanged;

        public DimmerEngine(List<DimmerOverlay> overlays)
        {
            _overlays = overlays;

            _monitorTimer = new DispatcherTimer();
            _monitorTimer.Interval = TimeSpan.FromMilliseconds(100);
            _monitorTimer.Tick += MonitorTimer_Tick;
        }

        public void Start() => _monitorTimer.Start();
        public void Stop() => _monitorTimer.Stop();

        public void ForceTick() => MonitorTimer_Tick(null, EventArgs.Empty);

        public void ResetState()
        {
            _lastForegroundWindow = IntPtr.Zero;
            _lastRectForMotion = new NativeMethods.RECT();
            ProcessInfoHelper.ClearCache();
            _monitorTimer.Stop();
            _monitorTimer.Start();
        }

        private void MonitorTimer_Tick(object? sender, EventArgs? e)
        {
            if (!_isEnabled)
            {
                foreach (var ov in _overlays) ov.SetVisibility(false);
                return;
            }

            try
            {
                foreach (var overlay in _overlays)
                {
                    overlay.EnsureTopmost();
                }

                // アイドル時間の取得
                uint idleMs = GetIdleTimeMs();
                double idleSec = idleMs / 1000.0;

                IntPtr foregroundWindow = NativeMethods.GetForegroundWindow();

                NativeMethods.RECT currentRect = new NativeMethods.RECT();
                bool hasRect = false;
                if (foregroundWindow != IntPtr.Zero)
                {
                    hasRect = NativeMethods.GetWindowRect(foregroundWindow, out currentRect);
                }

                bool isMoving = (foregroundWindow != _lastForegroundWindow) || (hasRect && !currentRect.Equals(_lastRectForMotion));

                if (isMoving)
                {
                    _lastRectForMotion = currentRect;
                    _monitorTimer.Interval = TimeSpan.FromMilliseconds(15);
                    _highSpeedFrames = 20;
                }
                else
                {
                    if (_highSpeedFrames > 0) _highSpeedFrames--;
                    else if (_monitorTimer.Interval.TotalMilliseconds < 100) _monitorTimer.Interval = TimeSpan.FromMilliseconds(100);
                }

                if (foregroundWindow != IntPtr.Zero && (!NativeMethods.IsWindowVisible(foregroundWindow) || NativeMethods.IsIconic(foregroundWindow)))
                {
                    foregroundWindow = IntPtr.Zero;
                }

                if (IsIgnoredWindow(foregroundWindow))
                {
                    if (_lastForegroundWindow != IntPtr.Zero && NativeMethods.IsWindowVisible(_lastForegroundWindow) && !NativeMethods.IsIconic(_lastForegroundWindow))
                        foregroundWindow = _lastForegroundWindow;
                    else
                    {
                        _lastForegroundWindow = IntPtr.Zero;
                        foregroundWindow = IntPtr.Zero;
                    }
                }

                bool globalWindowChanged = (foregroundWindow != _lastForegroundWindow);
                if (globalWindowChanged) _lastForegroundWindow = foregroundWindow;

                bool isDesktopOrNull = (foregroundWindow == IntPtr.Zero) || IsDesktopWindow(foregroundWindow);

                string activeDeviceName = "";
                if (!isDesktopOrNull && foregroundWindow != IntPtr.Zero)
                {
                    var center = new Point((currentRect.Left + currentRect.Right) / 2, (currentRect.Top + currentRect.Bottom) / 2);
                    var activeScreen = Screen.FromPoint(center);
                    if (activeScreen != null) activeDeviceName = activeScreen.DeviceName;
                }

                bool isCurrentWindowExcluded = false;
                if (!isDesktopOrNull)
                {
                    var activeOverlay = _overlays.FirstOrDefault(o => o.LinkedProfile.DeviceName == activeDeviceName);
                    if (activeOverlay != null)
                    {
                        isCurrentWindowExcluded = CheckIfExcluded(foregroundWindow, activeOverlay.LinkedProfile);
                    }
                }

                // プロセス変更イベントの発火
                if (globalWindowChanged && foregroundWindow != IntPtr.Zero && !isDesktopOrNull)
                {
                    NativeMethods.GetWindowThreadProcessId(foregroundWindow, out uint pid);
                    string activeProcessName = ProcessInfoHelper.GetProcessName(pid);
                    if (!string.IsNullOrEmpty(activeProcessName))
                    {
                        ActiveProcessChanged?.Invoke(activeProcessName);
                    }
                }

                foreach (var overlay in _overlays)
                {
                    if (overlay.LinkedProfile.Opacity <= 0)
                    {
                        // 0%でもアイドル時減光が有効なら続行する
                        bool allowIdle = overlay.LinkedProfile.DimWhenIdle && overlay.LinkedProfile.IdleDimOpacity > 0;
                        if (!allowIdle)
                        {
                            overlay.SetVisibility(false);
                            continue;
                        }
                    }
                    overlay.SetVisibility(true);

                    bool isActiveMonitor = (overlay.LinkedProfile.DeviceName == activeDeviceName);

                    // アイドル設定は分単位なので秒に換算して比較
                    bool isIdle = overlay.LinkedProfile.DimWhenIdle && (idleSec > (overlay.LinkedProfile.IdleTimeout * 60));
                    bool dimEntirelyInactive = !isActiveMonitor && !isDesktopOrNull && overlay.LinkedProfile.DimEntirelyWhenInactive;

                    // アイドル または 非アクティブモニターの全画面暗転 が有効な場合
                    bool forceFullDim = isIdle || dimEntirelyInactive;

                    if (forceFullDim)
                    {
                        // 完全に暗くする (forceNoHoles = true)
                        overlay.UpdateState(IntPtr.Zero, true, false, true, isIdle);
                    }
                    else if (overlay.LinkedProfile.DimDesktopOnly)
                    {
                        // デスクトップのみ暗くする
                        bool shouldDim = !isDesktopOrNull;
                        overlay.UpdateState(foregroundWindow, shouldDim, globalWindowChanged, false, false);
                    }
                    else
                    {
                        // 通常モード
                        if (isDesktopOrNull)
                        {
                            overlay.UpdateState(IntPtr.Zero, false, globalWindowChanged, false, false);
                        }
                        else
                        {
                            bool shouldDim = isActiveMonitor && !isCurrentWindowExcluded;
                            overlay.UpdateState(foregroundWindow, shouldDim, globalWindowChanged, false, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DimmerEngine] Tick error: {ex.Message}");
            }
        }

        private static uint GetIdleTimeMs()
        {
            NativeMethods.LASTINPUTINFO lii = new NativeMethods.LASTINPUTINFO();
            lii.cbSize = (uint)Marshal.SizeOf(lii);
            lii.dwTime = 0;
            if (NativeMethods.GetLastInputInfo(ref lii))
            {
                return (uint)Environment.TickCount - lii.dwTime;
            }
            return 0;
        }

        private static bool IsIgnoredWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return false;
            if (WindowHelper.IsMenuOrPopupEx(hwnd)) return false;

            StringBuilder sb = new StringBuilder(256);
            NativeMethods.GetClassName(hwnd, sb, sb.Capacity);
            string className = sb.ToString();

            if (className == "CiceroUIWndFrame") return true;
            if (className == "InputIndicator") return true;
            if (className.Contains("SnapLayout")) return true;
            if (className == "MagUIClass") return true;

            int exStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
            if ((exStyle & NativeMethods.WS_EX_TRANSPARENT) != 0) return true;

            NativeMethods.GetWindowThreadProcessId(hwnd, out uint pid);
            string procName = ProcessInfoHelper.GetProcessName(pid);

            if (procName == "explorer")
            {
                bool isFolder = (className == "CabinetWClass" || className == "ExploreWClass");
                bool isDesktop = (className == "Progman" || className == "WorkerW");
                bool isDialog = (className == "#32770");

                if (WindowHelper.IsMenuOrPopupEx(hwnd)) return false;
                if (!isFolder && !isDesktop && !isDialog) return true;
            }

            if (procName == "shellexperiencehost" ||
                procName == "startmenuexperiencehost" ||
                procName == "searchhost")
                return true;

            return false;
        }

        private static bool IsDesktopWindow(IntPtr hwnd)
        {
            StringBuilder sb = new StringBuilder(256);
            NativeMethods.GetClassName(hwnd, sb, sb.Capacity);
            string className = sb.ToString();
            return (className == "Progman" || className == "WorkerW");
        }

        private static bool CheckIfExcluded(IntPtr hwnd, MonitorProfile profile)
        {
            if (hwnd == IntPtr.Zero) return true;
            NativeMethods.GetWindowThreadProcessId(hwnd, out uint pid);
            string procName = ProcessInfoHelper.GetProcessName(pid);
            var userList = profile.IgnoreList.Split(',')
                .Select(x => x.Trim().ToLower().Replace(".exe", ""))
                .Where(x => !string.IsNullOrEmpty(x)).ToList();
            if (userList.Contains(procName)) return true;

            if (profile.ExcludeTopmost)
            {
                int exStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
                if ((exStyle & NativeMethods.WS_EX_TOPMOST) != 0) return true;
            }

            {
                NativeMethods.WINDOWPLACEMENT placement = new NativeMethods.WINDOWPLACEMENT();
                placement.length = Marshal.SizeOf(placement);
                if (NativeMethods.GetWindowPlacement(hwnd, ref placement))
                {
                    if (placement.showCmd == 3) return true; // SW_SHOWMAXIMIZED
                }
            }
            return false;
        }

        public void Dispose()
        {
            _monitorTimer.Stop();
        }
    }
}
