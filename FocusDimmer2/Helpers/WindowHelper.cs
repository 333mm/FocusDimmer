using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using FocusDimmer.Services;

namespace FocusDimmer.Helpers
{
    public static class WindowHelper
    {
        public static bool IsMenuOrPopupEx(IntPtr hwnd)
        {
            if (!NativeMethods.GetWindowRect(hwnd, out NativeMethods.RECT r)) return false;
            if (r.Right - r.Left <= 1 || r.Bottom - r.Top <= 1) return false;

            StringBuilder sb = new StringBuilder(256);
            NativeMethods.GetClassName(hwnd, sb, sb.Capacity);
            string cls = sb.ToString();

            if (cls.Contains("SnapLayout") || cls == "MagUIClass") return false;

            if (NativeMethods.IsSystemMenuOrPopupClass(cls))
            {
                if (cls == "Windows.UI.Core.CoreWindow")
                {
                    double w = r.Right - r.Left;
                    double h = r.Bottom - r.Top;
                    double screenW = SystemParameters.PrimaryScreenWidth;
                    double screenH = SystemParameters.PrimaryScreenHeight;
                    if (w > screenW * 0.8 && h > screenH * 0.8) return false;
                    return true;
                }
                return true;
            }

            NativeMethods.GetWindowThreadProcessId(hwnd, out uint pid);
            string procName = ProcessInfoHelper.GetProcessName(pid);
            if (procName == "explorer")
            {
                int style = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_STYLE);
                if ((style & NativeMethods.WS_POPUP) != 0)
                {
                    if (cls == "Progman" || cls == "WorkerW" || cls == "Shell_TrayWnd" || cls == "Shell_SecondaryTrayWnd") return false;

                    // Windows Forms references are problematic in WPF projects without explicit ref. 
                    // Using generic Screen logic or WPF logic is better, but here keeping existing logic logic is simpler if we assume WinForms ref exists.
                    // However, to be safe and cleaner in WPF, let's just stick to the existing logic if it compiles, 
                    // or remove the WinForms dependency if possible.
                    // The original code used WinForms.Screen.FromHandle(hwnd) so we assume it's available.
                     try {
                        var screen = System.Windows.Forms.Screen.FromHandle(hwnd);
                        if (r.Top <= screen.Bounds.Top + 1) return false;
                    } catch { } // Fallback

                    return true;
                }
            }
            return false;
        }

        public static void ApplySystemBackdrop(Window window)
        {
            if (window == null) return;

            var windowInteropHelper = new WindowInteropHelper(window);
            var hwnd = windowInteropHelper.Handle;
            if (hwnd == IntPtr.Zero) return;

            // Always enable immersive dark mode for title bar / system buttons
            int darkMode = 1;
            NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));

            // Windows 11 Build 22621+ (22H2 / 23H2 / 24H2)
            if (IsWindows11_22H2OrGreater())
            {
                // DWMWA_SYSTEMBACKDROP_TYPE: 3 = Acrylic (DWMSBT_TRANSIENTWINDOW)
                int backdropType = NativeMethods.DWMSBT_TRANSIENTWINDOW;
                NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));

                // Rounded corners
                int cornerPreference = NativeMethods.DWMWCP_ROUND;
                NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));
            }
            // Windows 11 Build 22000 (21H2)
            else if (IsWindows11OrGreater())
            {
                int micaVal = 1;
                NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_MICA_EFFECT, ref micaVal, sizeof(int));

                int cornerPreference = NativeMethods.DWMWCP_ROUND;
                NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));
            }
            else
            {
                // Windows 10 fallback:
                // Never apply SetWindowCompositionAttribute on Win11 to avoid full-screen blur bugs.
                // On Windows 10, ACCENT_ENABLE_BLURBEHIND can be applied safely.
                try
                {
                    var accent = new NativeMethods.AccentPolicy();
                    accent.AccentState = NativeMethods.AccentState.ACCENT_ENABLE_BLURBEHIND;

                    var accentStructSize = Marshal.SizeOf(accent);
                    var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                    Marshal.StructureToPtr(accent, accentPtr, false);

                    var data = new NativeMethods.WindowCompositionAttributeData();
                    data.Attribute = NativeMethods.WindowCompositionAttribute.WCA_ACCENT_POLICY;
                    data.SizeOfData = accentStructSize;
                    data.Data = accentPtr;

                    NativeMethods.SetWindowCompositionAttribute(hwnd, ref data);

                    Marshal.FreeHGlobal(accentPtr);
                }
                catch
                {
                    // Ignore fallback errors on non-supported platforms
                }
            }
        }

        public static void DisableBackdropAndBlur(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return;

            try
            {
                // Disable Win11 System Backdrop (Mica / Acrylic)
                if (IsWindows11_22H2OrGreater())
                {
                    int backdropType = NativeMethods.DWMSBT_NONE; // 1 = None
                    NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));
                }
                else if (IsWindows11OrGreater())
                {
                    int micaVal = 0;
                    NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_MICA_EFFECT, ref micaVal, sizeof(int));
                }

                // Reset DWM Glass margin extension
                var margins = new NativeMethods.MARGINS { cxLeftWidth = 0, cxRightWidth = 0, cyTopHeight = 0, cyBottomHeight = 0 };
                NativeMethods.DwmExtendFrameIntoClientArea(hwnd, ref margins);

                // Disable AccentPolicy Blur / Acrylic
                var accent = new NativeMethods.AccentPolicy { AccentState = NativeMethods.AccentState.ACCENT_DISABLED };
                var accentStructSize = Marshal.SizeOf(accent);
                var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                Marshal.StructureToPtr(accent, accentPtr, false);

                var data = new NativeMethods.WindowCompositionAttributeData
                {
                    Attribute = NativeMethods.WindowCompositionAttribute.WCA_ACCENT_POLICY,
                    SizeOfData = accentStructSize,
                    Data = accentPtr
                };

                NativeMethods.SetWindowCompositionAttribute(hwnd, ref data);
                Marshal.FreeHGlobal(accentPtr);
            }
            catch
            {
                // Fallback ignore
            }
        }

        public static bool IsWindows11_22H2OrGreater()
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 22621;
        }

        public static bool IsWindows11OrGreater()
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 22000;
        }
    }
}
