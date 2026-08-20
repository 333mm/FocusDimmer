using System;
using System.Collections.Generic;
using System.Windows.Interop;

namespace FocusDimmer.Services
{
    public class HotkeyService : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private IntPtr _windowHandle = IntPtr.Zero;
        private HwndSource? _hwndSource;

        public Dictionary<string, (int modifier, int key, int id)> Hotkeys { get; } = new()
        {
            { "Toggle",  (3, 0x24, 9000) }, // Ctrl+Alt+Home
            { "Darker",  (3, 0x21, 9001) }, // Ctrl+Alt+PageUp
            { "Lighter", (3, 0x22, 9002) }  // Ctrl+Alt+PageDown
        };

        public event Action? ToggleHotkeyPressed;
        public event Action? DarkerHotkeyPressed;
        public event Action? LighterHotkeyPressed;

        public void Initialize(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
            if (_windowHandle == IntPtr.Zero) return;

            _hwndSource = HwndSource.FromHwnd(_windowHandle);
            _hwndSource?.AddHook(WndProc);

            RegisterAll();
        }

        public void UpdateHotkey(string name, int modifier, int key)
        {
            if (Hotkeys.TryGetValue(name, out var current))
            {
                Hotkeys[name] = (modifier, key, current.id);
                RegisterAll();
            }
        }

        public void RegisterAll()
        {
            if (_windowHandle == IntPtr.Zero) return;

            foreach (var kvp in Hotkeys)
            {
                NativeMethods.UnregisterHotKey(_windowHandle, kvp.Value.id);
            }

            foreach (var kvp in Hotkeys)
            {
                NativeMethods.RegisterHotKey(_windowHandle, kvp.Value.id, (uint)kvp.Value.modifier, (uint)kvp.Value.key);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (id == Hotkeys["Toggle"].id)
                {
                    ToggleHotkeyPressed?.Invoke();
                    handled = true;
                }
                else if (id == Hotkeys["Darker"].id)
                {
                    DarkerHotkeyPressed?.Invoke();
                    handled = true;
                }
                else if (id == Hotkeys["Lighter"].id)
                {
                    LighterHotkeyPressed?.Invoke();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            if (_windowHandle != IntPtr.Zero)
            {
                foreach (var kvp in Hotkeys)
                {
                    NativeMethods.UnregisterHotKey(_windowHandle, kvp.Value.id);
                }
            }
            _hwndSource?.RemoveHook(WndProc);
            _hwndSource = null;
        }
    }
}
