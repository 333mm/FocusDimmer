using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FocusDimmer.Services
{
    public enum StartupEnableResult
    {
        Success,
        DisabledByUser,
        DisabledByPolicy,
        Error
    }

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

        public static async Task<StartupEnableResult> EnableStartupAsync()
        {
            if (IsPackaged)
            {
                try
                {
                    var startupTask = await Windows.ApplicationModel.StartupTask.GetAsync(StartupTaskId);
                    switch (startupTask.State)
                    {
                        case Windows.ApplicationModel.StartupTaskState.DisabledByUser:
                            return StartupEnableResult.DisabledByUser;
                        case Windows.ApplicationModel.StartupTaskState.DisabledByPolicy:
                            return StartupEnableResult.DisabledByPolicy;
                        case Windows.ApplicationModel.StartupTaskState.Disabled:
                            var state = await startupTask.RequestEnableAsync();
                            return state == Windows.ApplicationModel.StartupTaskState.Enabled ? StartupEnableResult.Success : StartupEnableResult.DisabledByUser;
                        case Windows.ApplicationModel.StartupTaskState.Enabled:
                            return StartupEnableResult.Success;
                        default:
                            return StartupEnableResult.Error;
                    }
                }
                catch { return StartupEnableResult.Error; }
            }
            else
            {
                try 
                { 
                    using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true); 
                    key?.SetValue(AppName, $"\"{Process.GetCurrentProcess().MainModule?.FileName ?? ""}\" /autostart");
                    return StartupEnableResult.Success;
                } 
                catch { return StartupEnableResult.Error; }
            }
        }

        public static async Task<bool> TryEnableStartupAsync()
        {
            var result = await EnableStartupAsync();
            return result == StartupEnableResult.Success;
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
