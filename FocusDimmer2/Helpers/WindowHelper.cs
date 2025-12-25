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

            // Windows 11 Build 22621+ (Mica Alt / Mica / Acrylic)
            if (IsWindows11_22H2OrGreater())
            {
                // Try DWMWA_SYSTEMBACKDROP_TYPE first for modern Win11 support (Main Window relied on this?)
                // 2 = DWMSBT_MICA, 3 = DWMSBT_ACRYLIC, 4 = DWMSBT_MICA_ALT
                int backdropType = 3; // Acrylic
                NativeMethods.DwmSetWindowAttribute(hwnd, 38, ref backdropType, sizeof(int));
                
                int darkMode = 1; 
                NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
            }
            else
            {
                // Fallback for Windows 10 or older Windows 11
                // Enable Blur Behind
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
        }

        private static bool IsWindows11_22H2OrGreater()
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 22621;
        }
    }
}
