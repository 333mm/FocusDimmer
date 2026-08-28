using System.Collections.Generic;

namespace FocusDimmer.Models
{
    public class AppSettings
    {
        public List<MonitorProfile> Profiles { get; set; } = new();
        public bool AutoStart { get; set; } = false;
        public bool RunAsAdmin { get; set; } = false;
        public bool CloseToTray { get; set; } = true;
        public bool AreMonitorsLinked { get; set; } = false;
        
        // Window persistence
        public double WindowWidth { get; set; } = 850;
        public double WindowHeight { get; set; } = 1000;
        public double WindowLeft { get; set; } = -10000; // Sentinel value
        public double WindowTop { get; set; } = -10000;  // Sentinel value

        // License migration & purchase persistence
        public bool IsProPurchased { get; set; } = false;
        public bool IsLegacyMigrated { get; set; } = false;
        public bool IsLegacyBannerDismissed { get; set; } = false;
        
        // Global Presets (applies to all monitors)
        public List<Preset> Presets { get; set; } = new();
        public string SelectedPresetId { get; set; } = "";
        public string DefaultPresetId { get; set; } = "";
        
        // Global Exclusion Lists
        public string IgnoreList { get; set; } = "";
        public string AlwaysBrightList { get; set; } = "";
        public string AlwaysDarkList { get; set; } = "amdow, NVIDIA Overlay";
    }
}
