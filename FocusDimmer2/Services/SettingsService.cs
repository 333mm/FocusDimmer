using System;
using System.IO;
using System.Text.Json;
using System.Windows.Threading;
using FocusDimmer.Models;

namespace FocusDimmer.Services
{
    public class SettingsService
    {
        private readonly string _settingsPath;
        private readonly DispatcherTimer _saveTimer;
        private AppSettings? _pendingSettings;

        public SettingsService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appData, "FocusDimmer");
            Directory.CreateDirectory(appFolder);
            _settingsPath = Path.Combine(appFolder, "settings.json");

            _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _saveTimer.Tick += (s, e) => SavePendingSettings();
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    string json = File.ReadAllText(_settingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SettingsService] Load failed: {ex.Message}");
            }
            return new AppSettings();
        }

        public void RequestSave(AppSettings settings)
        {
            _pendingSettings = settings;
            _saveTimer.Stop();
            _saveTimer.Start();
        }

        public void SaveImmediately(AppSettings settings)
        {
            _saveTimer.Stop();
            _pendingSettings = settings;
            SavePendingSettings();
        }

        private void SavePendingSettings()
        {
            _saveTimer.Stop();
            if (_pendingSettings == null) return;

            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_pendingSettings, options);
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SettingsService] Save failed: {ex.Message}");
            }
        }
    }
}
