using System.Collections.Generic;

namespace FocusDimmer.Models
{
    public class AppSettings
    {
        public List<MonitorProfile> Profiles { get; set; } = new();
        public bool AutoStart { get; set; } = false;
        public bool RunAsAdmin { get; set; } = false;
        
        // Window persistence
        public double WindowWidth { get; set; } = 750;
        public double WindowHeight { get; set; } = 1000;
        public double WindowLeft { get; set; } = -10000; // Sentinel value
        public double WindowTop { get; set; } = -10000;  // Sentinel value

        // License migration persistence
        public bool IsLegacyMigrated { get; set; } = false;
        public bool IsLegacyBannerDismissed { get; set; } = false;
    }
}
