using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using FocusDimmer.Helpers;
using FocusDimmer.Models;
using FocusDimmer.Services;

namespace FocusDimmer.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly SettingsService _settingsService;
        private readonly PresetService _presetService;
        private readonly StoreService _storeService;

        private AppSettings _appSettings;
        private bool _isPro = false;
        private string _selectedGlobalPresetId = "";
        private string _defaultPresetId = "";
        private int _selectedLanguageIndex;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public bool IsLegacyPro =>
#if LITE_VERSION
            false;
#elif LEGACY_PRO
            true;
#else
            false;
#endif

        public bool IsPro
        {
            get => _isPro;
            internal set
            {
                _isPro = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsFreeVersion));
                NotifyPropertyChanged(nameof(FreeBannerVisibility));
                NotifyPropertyChanged(nameof(LegacyBannerVisibility));
                NotifyPropertyChanged(nameof(ProBadgeVisibility));
            }
        }

        public bool IsFreeVersion => !IsPro;
        public Visibility FreeBannerVisibility => (IsFreeVersion && !IsLegacyPro) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LegacyBannerVisibility => (IsLegacyPro && !IsLegacyBannerDismissed) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ProBadgeWithIconVisibility => (IsLegacyPro && IsLegacyBannerDismissed) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ProBadgeVisibility => IsPro ? Visibility.Visible : Visibility.Collapsed;

        public bool IsLegacyBannerDismissed
        {
            get => _appSettings.IsLegacyBannerDismissed;
            set
            {
                _appSettings.IsLegacyBannerDismissed = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(LegacyBannerVisibility));
                NotifyPropertyChanged(nameof(ProBadgeWithIconVisibility));
                RequestSave();
            }
        }

        public ObservableCollection<MonitorProfile> MonitorProfiles { get; set; } = new();
        public ObservableCollection<Preset> GlobalPresets { get; set; } = new();
        public LocalizationService Strings { get; set; } = new();

        public string SelectedGlobalPresetId
        {
            get => _selectedGlobalPresetId;
            set
            {
                if (_selectedGlobalPresetId != value)
                {
                    _selectedGlobalPresetId = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(SelectedGlobalPreset));
                    if (SelectedGlobalPreset != null && !_presetService.IsApplyingPreset)
                    {
                        _presetService.ApplyPresetToProfiles(SelectedGlobalPreset, MonitorProfiles);
                    }
                    _appSettings.SelectedPresetId = _selectedGlobalPresetId;
                }
            }
        }

        public Preset? SelectedGlobalPreset => GlobalPresets.FirstOrDefault(p => p.Id == SelectedGlobalPresetId);

        public string DefaultPresetId
        {
            get => _defaultPresetId;
            set
            {
                _defaultPresetId = value;
                _appSettings.DefaultPresetId = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(GlobalPresets));
            }
        }

        public string IgnoreList
        {
            get => _appSettings.IgnoreList;
            set { _appSettings.IgnoreList = value; NotifyPropertyChanged(); RequestSave(); }
        }

        public string AlwaysBrightList
        {
            get => _appSettings.AlwaysBrightList;
            set { _appSettings.AlwaysBrightList = value; NotifyPropertyChanged(); RequestSave(); }
        }

        public string AlwaysDarkList
        {
            get => _appSettings.AlwaysDarkList;
            set { _appSettings.AlwaysDarkList = value; NotifyPropertyChanged(); RequestSave(); }
        }

        public bool CloseToTray
        {
            get => _appSettings.CloseToTray;
            set { _appSettings.CloseToTray = value; NotifyPropertyChanged(); RequestSave(); }
        }

        public bool AreMonitorsLinked
        {
            get => _appSettings.AreMonitorsLinked;
            set { _appSettings.AreMonitorsLinked = value; NotifyPropertyChanged(); RequestSave(); }
        }

        public string AppVersion { get; private set; } = "";

        public int SelectedLanguageIndex
        {
            get => _selectedLanguageIndex;
            set
            {
                _selectedLanguageIndex = value;
                string code = value switch
                {
                    1 => "ja",
                    2 => "zh",
                    3 => "de",
                    4 => "es",
                    5 => "pt",
                    6 => "fr",
                    _ => "en"
                };
                Strings.UpdateLanguage(code);
                NotifyPropertyChanged();
            }
        }

        public ICommand SetAsDefaultCommand { get; }
        public ICommand RemoveProcessRuleCommand { get; }

        public MainViewModel(SettingsService settingsService, PresetService presetService, StoreService storeService)
        {
            _settingsService = settingsService;
            _presetService = presetService;
            _storeService = storeService;

            _appSettings = _settingsService.LoadSettings();

            SetAsDefaultCommand = new RelayCommand(_ =>
            {
                if (!string.IsNullOrEmpty(SelectedGlobalPresetId))
                {
                    DefaultPresetId = SelectedGlobalPresetId;
                    RequestSave();
                }
            });

            RemoveProcessRuleCommand = new RelayCommand(param =>
            {
                if (param is ProcessPresetRule rule && SelectedGlobalPreset != null)
                {
                    SelectedGlobalPreset.ProcessRules.Remove(rule);
                    _appSettings.Presets = new List<Preset>(GlobalPresets);
                    RequestSave();
                }
            });

            InitializeLanguage();
            InitializeVersion();
            LoadPresetsFromSettings();
        }

        private void InitializeLanguage()
        {
            var twoLetter = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            SelectedLanguageIndex = twoLetter switch
            {
                "ja" => 1,
                "zh" => 2,
                "de" => 3,
                "es" => 4,
                "pt" => 5,
                "fr" => 6,
                _ => 0
            };
        }

        private void InitializeVersion()
        {
            try
            {
                var v = Windows.ApplicationModel.Package.Current.Id.Version;
                AppVersion = $"v{v.Major}.{v.Minor}.{v.Build}";
            }
            catch
            {
                var v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                AppVersion = v != null ? $"v{v.Major}.{v.Minor}.{v.Build}" : "v0.0.0";
            }
        }

        public void LoadPresetsFromSettings()
        {
            GlobalPresets.Clear();
            if (_appSettings.Presets != null)
            {
                foreach (var p in _appSettings.Presets) GlobalPresets.Add(p);
            }

            string targetId = _appSettings.SelectedPresetId ?? "";
            if (!GlobalPresets.Any(p => p.Id == targetId))
            {
                if (!string.IsNullOrEmpty(_appSettings.DefaultPresetId) && GlobalPresets.Any(p => p.Id == _appSettings.DefaultPresetId))
                {
                    targetId = _appSettings.DefaultPresetId;
                }
                else if (GlobalPresets.Count > 0)
                {
                    targetId = GlobalPresets[0].Id;
                }
            }
            SelectedGlobalPresetId = targetId;
            DefaultPresetId = _appSettings.DefaultPresetId;
        }

        public void RequestSave(double windowWidth = 0, double windowHeight = 0, double windowLeft = -10000, double windowTop = -10000)
        {
            if (windowWidth > 0) _appSettings.WindowWidth = windowWidth;
            if (windowHeight > 0) _appSettings.WindowHeight = windowHeight;
            if (windowLeft > -9000) _appSettings.WindowLeft = windowLeft;
            if (windowTop > -9000) _appSettings.WindowTop = windowTop;

            _appSettings.Profiles = new List<MonitorProfile>(MonitorProfiles);
            _appSettings.Presets = new List<Preset>(GlobalPresets);
            _appSettings.SelectedPresetId = SelectedGlobalPresetId;
            _appSettings.DefaultPresetId = DefaultPresetId;
            _appSettings.IgnoreList = IgnoreList;
            _appSettings.AlwaysBrightList = AlwaysBrightList;
            _appSettings.AlwaysDarkList = AlwaysDarkList;
            _appSettings.CloseToTray = CloseToTray;
            _appSettings.AreMonitorsLinked = AreMonitorsLinked;

            _settingsService.RequestSave(_appSettings);
        }

        public void SaveImmediately(double windowWidth = 0, double windowHeight = 0, double windowLeft = -10000, double windowTop = -10000)
        {
            if (windowWidth > 0) _appSettings.WindowWidth = windowWidth;
            if (windowHeight > 0) _appSettings.WindowHeight = windowHeight;
            if (windowLeft > -9000) _appSettings.WindowLeft = windowLeft;
            if (windowTop > -9000) _appSettings.WindowTop = windowTop;

            _appSettings.Profiles = new List<MonitorProfile>(MonitorProfiles);
            _appSettings.Presets = new List<Preset>(GlobalPresets);
            _appSettings.SelectedPresetId = SelectedGlobalPresetId;
            _appSettings.DefaultPresetId = DefaultPresetId;
            _appSettings.IgnoreList = IgnoreList;
            _appSettings.AlwaysBrightList = AlwaysBrightList;
            _appSettings.AlwaysDarkList = AlwaysDarkList;
            _appSettings.CloseToTray = CloseToTray;
            _appSettings.AreMonitorsLinked = AreMonitorsLinked;

            _settingsService.SaveImmediately(_appSettings);
        }

        public AppSettings GetCurrentSettings() => _appSettings;
    }
}
