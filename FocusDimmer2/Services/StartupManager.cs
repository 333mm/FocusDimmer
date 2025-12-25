using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FocusDimmer.Services
{
    public static class StartupManager
    {
        private const string AppName = "FocusDimmer";
        private const string StartupTaskId = "FocusDimmerStartup";

        public static bool IsPackaged
        {
            get
            {
                try
                {
                    return Windows.ApplicationModel.Package.Current != null;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static async Task<bool> TryEnableStartupAsync()
        {
            if (IsPackaged)
            {
                try
                {
                    var startupTask = await Windows.ApplicationModel.StartupTask.GetAsync(StartupTaskId);
                    if (startupTask.State != Windows.ApplicationModel.StartupTaskState.Enabled)
                    {
                        var state = await startupTask.RequestEnableAsync();
                        return state == Windows.ApplicationModel.StartupTaskState.Enabled;
                    }
                    return true;
                }
                catch { return false; }
            }
            else
            {
                try 
                { 
                    using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true); 
                    key?.SetValue(AppName, $"\"{Process.GetCurrentProcess().MainModule.FileName}\" /autostart");
                    return true;
                } 
                catch { return false; }
            }
        }

        public static async Task<bool> DisableStartupAsync()
        {
            if (IsPackaged)
            {
                try
                {
                    var startupTask = await Windows.ApplicationModel.StartupTask.GetAsync(StartupTaskId);
                    if (startupTask.State == Windows.ApplicationModel.StartupTaskState.Enabled)
                    {
                        startupTask.Disable();
                    }
                    return true;
                }
                catch { return false; }
            }
            else
            {
                try 
                { 
                    using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true); 
                    key?.DeleteValue(AppName, false);
                    return true;
                } 
                catch { return false; }
            }
        }

        public static async Task<bool> IsStartupEnabledAsync()
        {
            if (IsPackaged)
            {
                try
                {
                    var startupTask = await Windows.ApplicationModel.StartupTask.GetAsync(StartupTaskId);
                    return startupTask.State == Windows.ApplicationModel.StartupTaskState.Enabled;
                }
                catch { return false; }
            }
            else
            {
                try { using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false); return key?.GetValue(AppName) != null; } catch { return false; }
            }
        }
    }
}
