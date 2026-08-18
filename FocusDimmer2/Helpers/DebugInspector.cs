using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using FocusDimmer.Services;
using FocusDimmer.Views;

namespace FocusDimmer.Helpers
{
    public class DebugInspector
    {
        private DispatcherTimer _timer;
        private DebugInspectorWindow? _window;
        private LocalizationService _strings;
        private bool _isTracking;

        public DebugInspector(LocalizationService strings)
        {
            _strings = strings;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += Timer_Tick;
        }

        public event EventHandler? StopRequested;
        public event EventHandler<WindowData>? SelectedWindowCaptured;
        
        private IntPtr _hwndSelf;
        private IntPtr _hookId = IntPtr.Zero;
        private NativeMethods.LowLevelMouseProc? _mouseProc;
        private List<WindowData> _currentWindows = new();
        private bool _isSelectionMode = false;

        public void Start()
        {
            if (_isTracking) return;
            _isTracking = true;
            _isSelectionMode = false;
            _window = new DebugInspectorWindow();
            _window.WindowSelected += (s, data) => 
            {
                SelectedWindowCaptured?.Invoke(this, data);
            };
            
            var helper = new System.Windows.Interop.WindowInteropHelper(_window);
            _hwndSelf = helper.EnsureHandle();

            NativeMethods.SetProp(_hwndSelf, "FocusDimmerInspector", new IntPtr(1));

            // Setup Mouse Hook
            _mouseProc = HookCallback;
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                if (curModule != null && curModule.ModuleName != null)
                {
                    _hookId = NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, _mouseProc,
                        NativeMethods. GetModuleHandle(curModule.ModuleName), 0);
                }
            }

            _window.Show();
            _timer.Start();
        }

        public void Stop()
        {
            if (!_isTracking) return;
            _isTracking = false;
            _isSelectionMode = false;
            _timer.Stop();

            if (_hookId != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }

            if (_hwndSelf != IntPtr.Zero)
            {
                NativeMethods.RemoveProp(_hwndSelf, "FocusDimmerInspector");
            }

            _window?.Close();
            _window = null;
            _hwndSelf = IntPtr.Zero;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)NativeMethods.WM_LBUTTONDOWN)
            {
                if (!_isSelectionMode && _currentWindows.Count > 0)
                {
                    // Freeze and enter selection mode
                    _isSelectionMode = true;
                    _timer.Stop();
                    if (_hookId != IntPtr.Zero)
                    {
                        NativeMethods.UnhookWindowsHookEx(_hookId);
                        _hookId = IntPtr.Zero;
                    }
                    
                    _window?.Dispatcher.Invoke(() => 
                    {
                        if (_window == null) return;
                        _window.UpdateStatus(GetString("DebugStatusSelect"));
                        _window.IsHitTestVisible = true;
                        _window.Topmost = true;
                        _window.Activate();
                    });
                    
                    return (IntPtr)1; // Consume
                }
            }
            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_window == null || _isSelectionMode) return;
            
            // Close on ESC
            if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.Escape))
            {
                StopRequested?.Invoke(this, EventArgs.Empty);
                return;
            }

            UpdateInspection();
        }

        private void UpdateInspection()
        {
            if (_window == null) return;
            if (!NativeMethods.GetCursorPos(out NativeMethods.POINT pt)) return;

            // DPI Handling
            double dpiScaleX = 1.0;
            double dpiScaleY = 1.0;
            var presentationSource = System.Windows.PresentationSource.FromVisual(_window);
            if (presentationSource != null && presentationSource.CompositionTarget != null)
            {
                dpiScaleX = presentationSource.CompositionTarget.TransformToDevice.M11;
                dpiScaleY = presentationSource.CompositionTarget.TransformToDevice.M22;
            }

            // Move inspector window near cursor
            double left = (pt.X / dpiScaleX) + 20;
            double top = (pt.Y / dpiScaleY) + 20;

            // Boundary Handle
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            
            // Use estimated size if not yet rendered, but after first few ticks ActualWidth/Height will be set
            double winWidth = _window.ActualWidth > 0 ? _window.ActualWidth : 350;
            double winHeight = _window.ActualHeight > 0 ? _window.ActualHeight : 300;

            if (left + winWidth > screenWidth) left = (pt.X / dpiScaleX) - winWidth - 20;
            if (top + winHeight > screenHeight) top = (pt.Y / dpiScaleY) - winHeight - 20;

            _window.Left = Math.Max(0, left);
            _window.Top = Math.Max(0, top);

            _window.UpdateStatus(GetString("DebugStatusClick"));

            var newWindows = new List<WindowData>();
            int count = 0;

            NativeMethods.EnumWindows((hwnd, lParam) =>
            {
                if (NativeMethods.GetProp(hwnd, "FocusDimmerInspector") != IntPtr.Zero) return true;
                if (!NativeMethods.IsWindowVisible(hwnd)) return true;

                if (NativeMethods.GetWindowRect(hwnd, out NativeMethods.RECT r))
                {
                    if (pt.X >= r.Left && pt.X < r.Right && pt.Y >= r.Top && pt.Y < r.Bottom)
                    {
                        if (count < 10)
                        {
                            NativeMethods.GetWindowThreadProcessId(hwnd, out uint pid);
                            string processName = ProcessInfoHelper.GetProcessName(pid);
                            
                            StringBuilder className = new StringBuilder(256);
                            NativeMethods.GetClassName(hwnd, className, className.Capacity);

                            StringBuilder title = new StringBuilder(256);
                            NativeMethods.GetWindowText(hwnd, title, title.Capacity);
                            
                            int exStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);

                            var flags = new List<string>();
                            if ((exStyle & NativeMethods.WS_EX_TRANSPARENT) != 0) flags.Add(GetString("DebugTransparent"));
                            if ((exStyle & 0x80000) != 0) flags.Add(GetString("DebugLayered"));
                            if ((exStyle & NativeMethods.WS_EX_NOACTIVATE) != 0) flags.Add(GetString("DebugNoActivate"));
                            if ((exStyle & NativeMethods.WS_EX_TOOLWINDOW) != 0) flags.Add(GetString("DebugToolWindow"));

                            newWindows.Add(new WindowData
                            {
                                Index = count,
                                Hwnd = hwnd,
                                ProcessName = processName,
                                Title = title.ToString(),
                                ClassName = className.ToString(),
                                RectString = $"{r.Left},{r.Top} - {r.Right},{r.Bottom} ({r.Right - r.Left}x{r.Bottom - r.Top})",
                                Flags = string.Join(", ", flags)
                            });
                        }
                        count++;
                    }
                }
                return true;
            }, IntPtr.Zero);

            _currentWindows = newWindows;
            _window.UpdateList(_currentWindows);
        }

        private string GetString(string key)
        {
            // Simple fallback if key missing, though LocalizationService usually handles it
            var val = _strings[key];
            return string.IsNullOrEmpty(val) ? key : val;
        }
    }
}
