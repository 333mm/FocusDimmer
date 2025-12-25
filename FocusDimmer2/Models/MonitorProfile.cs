using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Forms; // Check if this is needed or if alias is used
using WinForms = System.Windows.Forms;

namespace FocusDimmer.Models
{
    public class MonitorProfile : INotifyPropertyChanged
    {
        [JsonIgnore] public string MonitorName { get; private set; }
        public string DeviceName { get; set; }
        [JsonIgnore] public WinForms.Screen ScreenRef { get; private set; }

        private double _opacity = 65; public double Opacity { get => _opacity; set { _opacity = value; NotifyPropertyChanged(); } }
        private double _margin = 0; public double Margin { get => _margin; set { _margin = value; NotifyPropertyChanged(); } }

        private double _delayDarken = 0; public double DelayDarken { get => _delayDarken; set { _delayDarken = value; NotifyPropertyChanged(); } }
        private double _durationDarken = 0; public double DurationDarken { get => _durationDarken; set { _durationDarken = value; NotifyPropertyChanged(); } }
        private double _durationBrighten = 0; public double DurationBrighten { get => _durationBrighten; set { _durationBrighten = value; NotifyPropertyChanged(); } }

        private bool _excludeTaskbar = true; public bool ExcludeTaskbar { get => _excludeTaskbar; set { _excludeTaskbar = value; NotifyPropertyChanged(); } }
        private bool _excludeTopmost = false; public bool ExcludeTopmost { get => _excludeTopmost; set { _excludeTopmost = value; NotifyPropertyChanged(); } }
        private bool _excludeMaximized = true; public bool ExcludeMaximized { get => _excludeMaximized; set { _excludeMaximized = value; NotifyPropertyChanged(); } }
        private bool _useTightFrame = true; public bool UseTightFrame { get => _useTightFrame; set { _useTightFrame = value; NotifyPropertyChanged(); } }

        private string _ignoreList = ""; public string IgnoreList { get => _ignoreList; set { _ignoreList = value; NotifyPropertyChanged(); } }
        private string _alwaysBrightList = ""; public string AlwaysBrightList { get => _alwaysBrightList; set { _alwaysBrightList = value; NotifyPropertyChanged(); } }
        private string _alwaysDarkList = "amdow, NVIDIA Overlay"; public string AlwaysDarkList { get => _alwaysDarkList; set { _alwaysDarkList = value; NotifyPropertyChanged(); } }

        private bool _dimEntirelyWhenInactive = false;
        public bool DimEntirelyWhenInactive { get => _dimEntirelyWhenInactive; set { _dimEntirelyWhenInactive = value; NotifyPropertyChanged(); } }

        private bool? _prevExcludeTaskbar;
        private bool? _prevExcludeTopmost;
        private bool? _prevUseTightFrame;
        private bool _dimDesktopOnly = false;
        public bool DimDesktopOnly
        {
            get => _dimDesktopOnly;
            set
            {
                if (_dimDesktopOnly != value)
                {
                    _dimDesktopOnly = value;
                    NotifyPropertyChanged();
                    if (_dimDesktopOnly)
                    {
                        // Save current state
                        _prevExcludeTaskbar = ExcludeTaskbar;
                        _prevExcludeTopmost = ExcludeTopmost;
                        _prevUseTightFrame = UseTightFrame;

                        // Force enable
                        ExcludeTaskbar = true;
                        ExcludeTopmost = true;
                        UseTightFrame = true;
                    }
                    else
                    {
                        // Restore state
                        if (_prevExcludeTaskbar.HasValue) ExcludeTaskbar = _prevExcludeTaskbar.Value;
                        if (_prevExcludeTopmost.HasValue) ExcludeTopmost = _prevExcludeTopmost.Value;
                        if (_prevUseTightFrame.HasValue) UseTightFrame = _prevUseTightFrame.Value;

                        _prevExcludeTaskbar = null;
                        _prevExcludeTopmost = null;
                        _prevUseTightFrame = null;
                    }
                }
            }
        }

        private bool _dimWhenIdle = false;
        public bool DimWhenIdle { get => _dimWhenIdle; set { _dimWhenIdle = value; NotifyPropertyChanged(); } }

        private int _idleTimeout = 30;
        public int IdleTimeout { get => _idleTimeout; set { _idleTimeout = value; NotifyPropertyChanged(); } }

        private string _overlayColorHex = "#000000";
        public string OverlayColorHex { get => _overlayColorHex; set { _overlayColorHex = value; NotifyPropertyChanged(); } }

        private double _idleDimOpacity = 80;
        public double IdleDimOpacity { get => _idleDimOpacity; set { _idleDimOpacity = value; NotifyPropertyChanged(); } }

        public MonitorProfile() { }
        public MonitorProfile(WinForms.Screen screen)
        {
            ScreenRef = screen;
            DeviceName = screen.DeviceName;
            MonitorName = screen.Primary ? $"Main ({screen.DeviceName})" : $"Sub ({screen.DeviceName})";
        }

        public void ApplySettings(MonitorProfile saved)
        {
            this.Opacity = saved.Opacity;
            this.Margin = saved.Margin;
            this.DelayDarken = saved.DelayDarken;
            this.DurationDarken = saved.DurationDarken;
            this.DurationBrighten = saved.DurationBrighten;
            this.ExcludeTaskbar = saved.ExcludeTaskbar;
            this.ExcludeTopmost = saved.ExcludeTopmost;
            this.ExcludeMaximized = true; // Hardcoded in original
            this.UseTightFrame = saved.UseTightFrame;
            this.IgnoreList = saved.IgnoreList;
            this.AlwaysBrightList = saved.AlwaysBrightList;
            this.AlwaysDarkList = saved.AlwaysDarkList;
            this.DimEntirelyWhenInactive = saved.DimEntirelyWhenInactive;
            this.DimDesktopOnly = saved.DimDesktopOnly;
            this.DimWhenIdle = saved.DimWhenIdle;
            this.IdleTimeout = saved.IdleTimeout == 0 ? 30 : saved.IdleTimeout;
            this.IdleDimOpacity = saved.IdleDimOpacity == 0 ? 80 : saved.IdleDimOpacity;
            this.OverlayColorHex = saved.OverlayColorHex ?? "#000000";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
