using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using FocusDimmer.Models;
using FocusDimmer.Services;
using FocusDimmer.Helpers;
using FocusDimmer.Components;
using FocusDimmer.Converters;
using FocusDimmer.Views;

// Alias definitions
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TextBox = System.Windows.Controls.TextBox;
using Button = System.Windows.Controls.Button;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Path = System.Windows.Shapes.Path;
using Brushes = System.Windows.Media.Brushes;
using WinForms = System.Windows.Forms;

namespace FocusDimmer
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly StoreService _storeService = new StoreService();
        private bool _isPro = false;
        public bool IsLegacyPro =>
#if LEGACY_PRO
            true;
#else
            false;
#endif
        public bool IsPro
        {
            get => _isPro;
            private set
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
        
        public bool IsLegacyBannerDismissed
        {
            get => _appSettings?.IsLegacyBannerDismissed ?? false;
            set
            {
                if (_appSettings != null)
                {
                    _appSettings.IsLegacyBannerDismissed = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(LegacyBannerVisibility));
                    NotifyPropertyChanged(nameof(ProBadgeWithIconVisibility));
                    RequestSave();
                }
            }
        }

        public Visibility LegacyBannerVisibility => (IsLegacyPro && !IsLegacyBannerDismissed) ? Visibility.Visible : Visibility.Collapsed;
        // Show megaphone icon inside Pro badge if Legacy Pro is detected AND banner is dismissed or hidden
        public Visibility ProBadgeWithIconVisibility => (IsLegacyPro && IsLegacyBannerDismissed) ? Visibility.Visible : Visibility.Collapsed;
        
        public Visibility ProBadgeVisibility => IsPro ? Visibility.Visible : Visibility.Collapsed;
        
        // App Store Page (for Migration)
        private const string AppStoreUrl = "ms-windows-store://pdp/?productid=9NXHXPNJL79X";
        // Add-on Store Page (for Upgrade)
        private const string UpgradeStoreUrl = "ms-windows-store://pdp/?productid=9MWHG48NMCV0";

        public ObservableCollection<MonitorProfile> MonitorProfiles { get; set; } = new();
        public LocalizationService Strings { get; set; } = new LocalizationService();

        public string AppVersion { get; private set; }

        private int _selectedLanguageIndex;
        public int SelectedLanguageIndex
        {
            get => _selectedLanguageIndex;
            set
            {
                _selectedLanguageIndex = value;
                string code = "en";
                switch (value)
                {
                    case 0: code = "en"; break;
                    case 1: code = "ja"; break;
                    case 2: code = "zh"; break;
                    case 3: code = "de"; break;
                    case 4: code = "es"; break;
                    case 5: code = "pt"; break;
                    case 6: code = "fr"; break;
                }
                Strings.UpdateLanguage(code);
                NotifyPropertyChanged();
            }
        }

        private List<DimmerOverlay> _overlays = new();
        private WinForms.NotifyIcon _notifyIcon;
        private DispatcherTimer _monitorTimer;
        private bool _isDimmerEnabled = true;
        private bool _reallyExit = false;
        private AppSettings _appSettings;

        private static Mutex _mutex;
        private bool _isInitialized = false;

        private DispatcherTimer _saveTimer;
        private string _settingsPath;

        private Dictionary<string, (int modifier, int key, int id)> _hotkeys = new()
        {
            { "Toggle",  (3, 0x24, 9000) },
            { "Darker",  (3, 0x21, 9001) },
            { "Lighter", (3, 0x22, 9002) }
        };
        private TextBox _focusingHotkeyBox = null;
        private bool _ignoreAutoStartEvents = false;
        private DebugInspector _debugInspector;


        public MainWindow()
        {
            bool createdNew;
            _mutex = new Mutex(true, "FocusDimmer_Unique_Instance_Mutex", out createdNew);
            if (!createdNew)
            {
                _reallyExit = true;
                Application.Current.Shutdown();
                return;
            }

            InitializeComponent();
            DataContext = this;

            // Title will be updated after store check

            var culture = CultureInfo.CurrentUICulture.Name;
            var twoLetter = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            if (twoLetter == "ja") SelectedLanguageIndex = 1;
            else if (twoLetter == "zh") SelectedLanguageIndex = 2;
            else if (twoLetter == "de") SelectedLanguageIndex = 3;
            else if (twoLetter == "es") SelectedLanguageIndex = 4;
            else if (twoLetter == "pt") SelectedLanguageIndex = 5;
            else if (twoLetter == "fr") SelectedLanguageIndex = 6;
            else SelectedLanguageIndex = 0;

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = System.IO.Path.Combine(appData, "FocusDimmer");
            Directory.CreateDirectory(appFolder);
            _settingsPath = System.IO.Path.Combine(appFolder, "settings.json");

            _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _saveTimer.Tick += (s, e) => SaveSettingsActual();

            try
            {
                var v = Windows.ApplicationModel.Package.Current.Id.Version;
                AppVersion = $"v{v.Major}.{v.Minor}.{v.Build}";
            }
            catch (Exception)
            {
                var v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                if (v != null) AppVersion = $"v{v.Major}.{v.Minor}.{v.Build}";
                else AppVersion = "v0.0.0";
            }
            
            _appSettings = LoadSettings();
            
            // Restore Window Size/Position
            if (_appSettings.WindowWidth > 0 && _appSettings.WindowHeight > 0)
            {
                this.Width = _appSettings.WindowWidth;
                this.Height = _appSettings.WindowHeight;
            }

            if (_appSettings.WindowLeft > -9000 && _appSettings.WindowTop > -9000)
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = _appSettings.WindowLeft;
                this.Top = _appSettings.WindowTop;
                
                // Basic off-screen check (optional, but good practice)
                if (this.Left < SystemParameters.VirtualScreenLeft - this.Width + 50) this.Left = SystemParameters.VirtualScreenLeft;
                if (this.Top < SystemParameters.VirtualScreenTop - this.Height + 50) this.Top = SystemParameters.VirtualScreenTop;
            }

            // Monitor initialization moved to InitializeMonitors() called in Loaded or after upgrade

            // Startup check moved to Loaded event to be async
            // bool isReg = StartupManager.IsRegisteredInRegistry(); 
            // AutoStartCheck.IsChecked = isReg;

            AdminRunCheck.IsChecked = false;
            AdminRunCheck.IsEnabled = false;
            AdminRunCheck.Visibility = Visibility.Collapsed;

            SetupTrayIcon();

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

            this.Loaded += async (s, e) =>
            {
#if DEBUG
                // デバッグ時は構成に応じて状態を強制設定
#if LEGACY_PRO
                IsPro = true;
#elif PRO_VERSION
                IsPro = true;
#else
                // DEBUGでもレガシー検出ロジックをテストするために呼び出す
                if (await _storeService.InitializeAsync(_appSettings))
                {
                    SaveSettingsActual();
                }
                IsPro = _storeService.IsPro; 
#endif
#else
                // リリース時はストア通信を行う
#if PRO_VERSION
                IsPro = true;
#else
                if (await _storeService.InitializeAsync(_appSettings))
                {
                    SaveSettingsActual();
                }
                IsPro = _storeService.IsPro;
#endif
#endif
                NotifyPropertyChanged(nameof(Strings));

                InitializeMonitors();

                // フリー版の場合は必ずメインモニターを選択状態にする。
                // Pro版でも、何も選択されていない状態(不具合)を防ぐため、デフォルトで0番目を選択する。
                if (MonitorTabs.Items.Count > 0)
                {
                    // フリー版は強制的に0(Primary)に戻す
                    if (IsFreeVersion)
                    {
                        MonitorTabs.SelectedIndex = 0;
                    }
                    // 何も選択されていない場合は0を選択
                    else if (MonitorTabs.SelectedIndex < 0)
                    {
                        MonitorTabs.SelectedIndex = 0;
                    }
                }

                RegisterAllHotkeys();
                _isInitialized = true;

                // Async startup check
                _ignoreAutoStartEvents = true;
                AutoStartCheck.IsChecked = await StartupManager.IsStartupEnabledAsync();
                _ignoreAutoStartEvents = false;
            };

            _monitorTimer = new DispatcherTimer();
            _monitorTimer.Interval = TimeSpan.FromMilliseconds(100);
            _monitorTimer.Tick += MonitorTimer_Tick;
            _monitorTimer.Start();
        }

        private void InitializeMonitors()
        {
            // Clear existing
            foreach (var ov in _overlays) ov.Dispose();
            _overlays.Clear();
            MonitorProfiles.Clear();

            foreach (var screen in WinForms.Screen.AllScreens)
            {
                if (IsFreeVersion && !screen.Primary) continue;

                var profile = new MonitorProfile(screen);
                var saved = _appSettings.Profiles.FirstOrDefault(p => p.DeviceName == screen.DeviceName);
                if (saved != null) profile.ApplySettings(saved);

                if (IsFreeVersion) profile.OverlayColorHex = "#000000";

                profile.PropertyChanged += (s, e) => RequestSave();

                MonitorProfiles.Add(profile);
                var overlay = new DimmerOverlay(profile);
                _overlays.Add(overlay);
                overlay.Show();
            }
        }

        private async void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                await Task.Delay(2000);
                
                // Reset internal state
                _lastForegroundWindow = IntPtr.Zero;
                _lastRectForMotion = new NativeMethods.RECT();
                ProcessInfoHelper.ClearCache();

                if (_monitorTimer != null) { _monitorTimer.Stop(); _monitorTimer.Start(); }
                foreach (var overlay in _overlays) 
                {
                    overlay.InvalidateCache();
                    overlay.ForceResetTopmost();
                }

                RegisterAllHotkeys();
                // Ensure state is updated immediately
                MonitorTimer_Tick(null, null);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void ShowProPromo()
        {
            var result = System.Windows.MessageBox.Show(Strings.MsgProRequired, "Pro Version Required", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes)
            {
                OpenStore_Click(this, new RoutedEventArgs());
            }
        }

        private void MigrationInfo_Click(object sender, RoutedEventArgs e)
        {
            // Banner clicked or Icon clicked
            // Do NOT dismiss banner here. Only close button dismisses it.

            var guide = new Views.MigrationGuideWindow(Strings);
            guide.Owner = this;
            guide.ShowDialog();
        }

        private void CloseBanner_Click(object sender, RoutedEventArgs e)
        {
            IsLegacyBannerDismissed = true;
            e.Handled = true;
        }

        private async void OpenStore_Click(object sender, RoutedEventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;

            // ---------------------------------------------------------
            // 購入処理の実行
            // ---------------------------------------------------------
            var (status, error) = await _storeService.RequestPurchaseAsync(handle);
            
            if (status == Windows.Services.Store.StorePurchaseStatus.Succeeded || 
                status == Windows.Services.Store.StorePurchaseStatus.AlreadyPurchased)
            {
                IsPro = _storeService.IsPro;
                if (IsPro)
                {
                    Strings.AppTitle = Strings.AppTitle.Replace(" (Free Lite)", "");
                    NotifyPropertyChanged(nameof(Strings));
                    InitializeMonitors();
                    System.Windows.MessageBox.Show("Thank you for your purchase!\nPro features unlocked.", "Success");
                }
            }
            else if (status != Windows.Services.Store.StorePurchaseStatus.NotPurchased)
            {
                // 購入キャンセル(NotPurchased) 以外で失敗した場合
                string errorMsg = error != null ? error.Message : "Unknown Error";
                string fullMsg = $"Purchase Failed.\nStatus: {status}\nError: {errorMsg}\n\nDo you want to open the Store Page manually?";

                var res = System.Windows.MessageBox.Show(fullMsg, "Purchase Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                
                if (res == MessageBoxResult.Yes)
                {
                    // アドオンの直接リンクは非公開の場合に空ページになるため、メインのストアページを開く
                    try { Process.Start(new ProcessStartInfo(AppStoreUrl) { UseShellExecute = true }); } catch { }
                }
            }
        }

        private void RequestSave() { _saveTimer.Stop(); _saveTimer.Start(); }
        private void SaveSettingsActual()
        {
            if (_saveTimer == null) return;
            _saveTimer.Stop();
            try
            {
                // Only update window dimensions if the window is actually visible/loaded (ActualWidth > 0)
                // Otherwise, keep the existing values from _appSettings (or don't update them in the new object)
                
                double w = (this.ActualWidth > 0) ? this.ActualWidth : _appSettings.WindowWidth;
                double h = (this.ActualHeight > 0) ? this.ActualHeight : _appSettings.WindowHeight;
                double l = (this.ActualWidth > 0) ? this.Left : _appSettings.WindowLeft;
                double t = (this.ActualWidth > 0) ? this.Top : _appSettings.WindowTop;

                var settings = new AppSettings 
                { 
                    Profiles = new List<MonitorProfile>(MonitorProfiles), 
                    AutoStart = AutoStartCheck.IsChecked == true, 
                    RunAsAdmin = false,
                    // Save window state
                    WindowWidth = w,
                    WindowHeight = h,
                    WindowLeft = l,
                    WindowTop = t
                };
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(_settingsPath, json);
            }
            catch { }
        }
        private AppSettings LoadSettings()
        {
            try { if (File.Exists(_settingsPath)) { string json = File.ReadAllText(_settingsPath); return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings(); } } catch { }
            return new AppSettings();
        }

        private async void AutoStart_Checked(object sender, RoutedEventArgs e) 
        { 
            if (_isInitialized && !_ignoreAutoStartEvents) 
            { 
                 bool success = await StartupManager.TryEnableStartupAsync();
                 if (!success) 
                 {
                     // Revert if failed
                     _ignoreAutoStartEvents = true;
                     AutoStartCheck.IsChecked = false;
                     _ignoreAutoStartEvents = false;
                 }
                 RequestSave(); 
            } 
        }
        private async void AutoStart_Unchecked(object sender, RoutedEventArgs e) 
        { 
            if (_isInitialized && !_ignoreAutoStartEvents) 
            { 
                await StartupManager.DisableStartupAsync();
                RequestSave(); 
            } 
        }
        private void AdminRun_Changed(object sender, RoutedEventArgs e) { }

        private System.Windows.Controls.Primitives.ToggleButton _activeInspectorButton;

        private void DebugInspector_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Primitives.ToggleButton;
            _activeInspectorButton = button;

            if (button?.IsChecked == true)
            {
                // Start debug inspector
                if (_debugInspector == null)
                {
                    _debugInspector = new DebugInspector(Strings);
                    _debugInspector.StopRequested += (s, args) => 
                    {
                        if (_activeInspectorButton != null)
                        {
                            _activeInspectorButton.IsChecked = false;
                        }
                        _debugInspector.Stop();
                    };

                    _debugInspector.SelectedWindowCaptured += (s, data) =>
                    {
                        Dispatcher.Invoke(() => 
                        {
                            var profile = MonitorTabs.SelectedItem as MonitorProfile;
                            if (profile == null) return;

                            var dialog = new InspectorActionDialog(data.ProcessName, data.Title, Strings);
                            dialog.Owner = this;
                            if (dialog.ShowDialog() == true)
                            {
                                switch (dialog.ActionType)
                                {
                                    case "Ignore":
                                        AddProcessToList(profile, p => p.IgnoreList, (p, v) => p.IgnoreList = v, data.ProcessName);
                                        break;
                                    case "Bright":
                                        AddProcessToList(profile, p => p.AlwaysBrightList, (p, v) => p.AlwaysBrightList = v, data.ProcessName);
                                        break;
                                    case "Dark":
                                        AddProcessToList(profile, p => p.AlwaysDarkList, (p, v) => p.AlwaysDarkList = v, data.ProcessName);
                                        break;
                                }
                            }
                            
                            // Always stop inspector after a selection or cancellation
                            _debugInspector.Stop();
                            if (_activeInspectorButton != null) _activeInspectorButton.IsChecked = false;
                        });
                    };
                }
                _debugInspector.Start();
            }
            else
            {
                // Stop debug inspector
                _debugInspector?.Stop();
            }
        }



        // UpdateStartup removed as logic is now in handlers

        private void BrowseApp_Click(object sender, RoutedEventArgs e)
        {
            if (IsFreeVersion) { ShowProPromo(); return; }
            AddProcessToProfile(sender, p => p.IgnoreList, (p, v) => p.IgnoreList = v);
        }
        private void BrowseBright_Click(object sender, RoutedEventArgs e)
        {
            if (IsFreeVersion) { ShowProPromo(); return; }
            AddProcessToProfile(sender, p => p.AlwaysBrightList, (p, v) => p.AlwaysBrightList = v);
        }
        private void BrowseDark_Click(object sender, RoutedEventArgs e)
        {
            if (IsFreeVersion) { ShowProPromo(); return; }
            AddProcessToProfile(sender, p => p.AlwaysDarkList, (p, v) => p.AlwaysDarkList = v);
        }

        private void AddProcessToProfile(object sender, Func<MonitorProfile, string> getter, Action<MonitorProfile, string> setter)
        {
            if (sender is Button btn && btn.DataContext is MonitorProfile profile)
            {
                var dialog = new ProcessSelectionWindow(Strings, btn);
                if (dialog.ShowDialog() == true)
                {
                    string processName = dialog.SelectedProcessName;
                    AddProcessToList(profile, getter, setter, processName);
                }
            }
        }

        private void AddProcessToList(MonitorProfile profile, Func<MonitorProfile, string> getter, Action<MonitorProfile, string> setter, string processName)
        {
            if (string.IsNullOrEmpty(processName)) return;

            var currentList = getter(profile) ?? "";
            var items = currentList.Split(',').Select(x => x.Trim().ToLower()).ToList();
            if (!items.Contains(processName.ToLower()))
            {
                if (string.IsNullOrWhiteSpace(currentList)) setter(profile, processName);
                else setter(profile, currentList.Trim() + ", " + processName);
                RequestSave();
            }
        }

        private void PickColor_Click(object sender, RoutedEventArgs e)
        {
            if (IsFreeVersion) { ShowProPromo(); return; }
            if (sender is Button btn && btn.DataContext is MonitorProfile profile)
            {
                try
                {
                    Color current = (Color)ColorConverter.ConvertFromString(profile.OverlayColorHex ?? "#000000");
                    var picker = new Views.ColorPickerWindow(current);
                    picker.Owner = this;
                    if (picker.ShowDialog() == true)
                    {
                        var c = picker.SelectedColor;
                        profile.OverlayColorHex = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
                    }
                }
                catch { }
            }
        }

        private IntPtr _lastForegroundWindow = IntPtr.Zero;
        private NativeMethods.RECT _lastRectForMotion = new NativeMethods.RECT();
        private int _highSpeedFrames = 0;

        // アイドル時間計測用
        private uint GetIdleTimeMs()
        {
            NativeMethods.LASTINPUTINFO lii = new NativeMethods.LASTINPUTINFO();
            lii.cbSize = (uint)Marshal.SizeOf(lii);
            lii.dwTime = 0;
            if (NativeMethods.GetLastInputInfo(ref lii))
            {
                return (uint)Environment.TickCount - lii.dwTime;
            }
            return 0;
        }

        private void MonitorTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isDimmerEnabled)
            {
                foreach (var ov in _overlays) ov.SetVisibility(false);
                return;
            }

            try
            {
                foreach (var overlay in _overlays)
                {
                    overlay.EnsureTopmost();
                }

                // アイドル時間の取得
                uint idleMs = GetIdleTimeMs();
                double idleSec = idleMs / 1000.0;

                IntPtr foregroundWindow = NativeMethods.GetForegroundWindow();

                NativeMethods.RECT currentRect = new NativeMethods.RECT();
                bool hasRect = false;
                if (foregroundWindow != IntPtr.Zero)
                {
                    hasRect = NativeMethods.GetWindowRect(foregroundWindow, out currentRect);
                }

                bool isMoving = (foregroundWindow != _lastForegroundWindow) || (hasRect && !currentRect.Equals(_lastRectForMotion));

                if (isMoving)
                {
                    _lastRectForMotion = currentRect;
                    _monitorTimer.Interval = TimeSpan.FromMilliseconds(15);
                    _highSpeedFrames = 20;
                }
                else
                {
                    if (_highSpeedFrames > 0) _highSpeedFrames--;
                    else if (_monitorTimer.Interval.TotalMilliseconds < 100) _monitorTimer.Interval = TimeSpan.FromMilliseconds(100);
                }

                if (foregroundWindow != IntPtr.Zero && (!NativeMethods.IsWindowVisible(foregroundWindow) || NativeMethods.IsIconic(foregroundWindow)))
                {
                    foregroundWindow = IntPtr.Zero;
                }

                if (IsIgnoredWindow(foregroundWindow))
                {
                    if (_lastForegroundWindow != IntPtr.Zero && NativeMethods.IsWindowVisible(_lastForegroundWindow) && !NativeMethods.IsIconic(_lastForegroundWindow))
                        foregroundWindow = _lastForegroundWindow;
                    else
                    {
                        _lastForegroundWindow = IntPtr.Zero;
                        foregroundWindow = IntPtr.Zero;
                    }
                }

                bool globalWindowChanged = (foregroundWindow != _lastForegroundWindow);
                if (globalWindowChanged) _lastForegroundWindow = foregroundWindow;

                bool isDesktopOrNull = (foregroundWindow == IntPtr.Zero) || IsDesktopWindow(foregroundWindow);

                string activeDeviceName = "";
                if (!isDesktopOrNull && foregroundWindow != IntPtr.Zero)
                {
                    var center = new System.Drawing.Point((currentRect.Left + currentRect.Right) / 2, (currentRect.Top + currentRect.Bottom) / 2);
                    var activeScreen = WinForms.Screen.FromPoint(center);
                    if (activeScreen != null) activeDeviceName = activeScreen.DeviceName;
                }

                bool isCurrentWindowExcluded = false;
                if (!isDesktopOrNull)
                {
                    var activeOverlay = _overlays.FirstOrDefault(o => o.LinkedProfile.DeviceName == activeDeviceName);
                    if (activeOverlay != null)
                    {
                        isCurrentWindowExcluded = CheckIfExcluded(foregroundWindow, activeOverlay.LinkedProfile);
                    }
                }

                foreach (var overlay in _overlays)
                {
                    if (overlay.LinkedProfile.Opacity <= 0)
                    {
                        // 0%でもアイドル時減光が有効なら続行する
                        bool allowIdle = overlay.LinkedProfile.DimWhenIdle && overlay.LinkedProfile.IdleDimOpacity > 0;
                        if (!allowIdle)
                        {
                            overlay.SetVisibility(false);
                            continue;
                        }
                    }
                    overlay.SetVisibility(true);

                    bool isActiveMonitor = (overlay.LinkedProfile.DeviceName == activeDeviceName);

                    // アイドル設定は分単位なので秒に換算して比較
                    bool isIdle = overlay.LinkedProfile.DimWhenIdle && (idleSec > (overlay.LinkedProfile.IdleTimeout * 60));

                    bool dimEntirelyInactive = !isActiveMonitor && !isDesktopOrNull && overlay.LinkedProfile.DimEntirelyWhenInactive;

                    // アイドル または 非アクティブモニターの全画面暗転 が有効な場合
                    bool forceFullDim = isIdle || dimEntirelyInactive;

                    if (forceFullDim)
                    {
                        // 完全に暗くする (forceNoHoles = true)
                        overlay.UpdateState(IntPtr.Zero, true, false, true, isIdle);
                    }
                    else if (overlay.LinkedProfile.DimDesktopOnly)
                    {
                        // デスクトップのみ暗くする
                        bool shouldDim = !isDesktopOrNull;
                        overlay.UpdateState(foregroundWindow, shouldDim, globalWindowChanged, false, false);
                    }
                    else
                    {
                        // 通常モード
                        if (isDesktopOrNull)
                        {
                            overlay.UpdateState(IntPtr.Zero, false, globalWindowChanged, false, false);
                        }
                        else
                        {
                            bool shouldDim = isActiveMonitor && !isCurrentWindowExcluded;
                            overlay.UpdateState(foregroundWindow, shouldDim, globalWindowChanged, false, false);
                        }
                    }
                }
            }
            catch { }
        }

        private bool IsIgnoredWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return false;
            if (WindowHelper.IsMenuOrPopupEx(hwnd)) return false;

            StringBuilder sb = new StringBuilder(256);
            NativeMethods.GetClassName(hwnd, sb, sb.Capacity);
            string className = sb.ToString();

            if (className == "CiceroUIWndFrame") return true;
            if (className == "InputIndicator") return true;
            if (className.Contains("SnapLayout")) return true;
            if (className == "MagUIClass") return true;

            int exStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
            if ((exStyle & NativeMethods.WS_EX_TRANSPARENT) != 0) return true;

            NativeMethods.GetWindowThreadProcessId(hwnd, out uint pid);
            string procName = ProcessInfoHelper.GetProcessName(pid);

            if (procName == "explorer")
            {
                bool isFolder = (className == "CabinetWClass" || className == "ExploreWClass");
                bool isDesktop = (className == "Progman" || className == "WorkerW");
                bool isDialog = (className == "#32770");

                if (WindowHelper.IsMenuOrPopupEx(hwnd)) return false;
                if (!isFolder && !isDesktop && !isDialog) return true;
            }

            if (procName == "shellexperiencehost" ||
                procName == "startmenuexperiencehost" ||
                procName == "searchhost")
                return true;

            return false;
        }

        private bool IsDesktopWindow(IntPtr hwnd)
        {
            StringBuilder sb = new StringBuilder(256);
            NativeMethods.GetClassName(hwnd, sb, sb.Capacity);
            string className = sb.ToString();
            return (className == "Progman" || className == "WorkerW");
        }

        private bool CheckIfExcluded(IntPtr hwnd, MonitorProfile profile)
        {
            if (hwnd == IntPtr.Zero) return true;
            NativeMethods.GetWindowThreadProcessId(hwnd, out uint pid);
            string procName = ProcessInfoHelper.GetProcessName(pid);
            var userList = profile.IgnoreList.Split(',')
                .Select(x => x.Trim().ToLower().Replace(".exe", ""))
                .Where(x => !string.IsNullOrEmpty(x)).ToList();
            if (userList.Contains(procName)) return true;

            if (profile.ExcludeTopmost)
            {
                int exStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
                if ((exStyle & NativeMethods.WS_EX_TOPMOST) != 0) return true;
            }

            {
                NativeMethods.WINDOWPLACEMENT placement = new NativeMethods.WINDOWPLACEMENT();
                placement.length = Marshal.SizeOf(placement);
                if (NativeMethods.GetWindowPlacement(hwnd, ref placement))
                {
                    if (placement.showCmd == 3) return true; // SW_SHOWMAXIMIZED
                }
            }
            return false;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_reallyExit) { e.Cancel = true; this.Hide(); }
            else
            {
                SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
                SaveSettingsActual();
                if (_notifyIcon != null) _notifyIcon.Dispose();
                foreach (var ov in _overlays) ov.Dispose();
                if (this.IsLoaded)
                {
                    var handle = new WindowInteropHelper(this).Handle;
                    if (handle != IntPtr.Zero)
                    {
                        foreach (var kvp in _hotkeys) NativeMethods.UnregisterHotKey(handle, kvp.Value.id);
                    }
                }
                base.OnClosing(e);
                Application.Current.Shutdown();
            }
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => this.Hide();
        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();

        private void HotkeyBox_GotFocus(object sender, RoutedEventArgs e) => _focusingHotkeyBox = sender as TextBox;
        private void HotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_focusingHotkeyBox == null) return;
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.LeftAlt || e.Key == Key.RightAlt || e.Key == Key.System) return;
            e.Handled = true;
            int modifiers = 0;
            var textParts = new List<string>();
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0) { modifiers += 2; textParts.Add("Ctrl"); }
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) { modifiers += 4; textParts.Add("Shift"); }
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0) { modifiers += 1; textParts.Add("Alt"); }
            Key realKey = (e.Key == Key.System) ? e.SystemKey : e.Key;
            textParts.Add(realKey.ToString());
            _focusingHotkeyBox.Text = string.Join(" + ", textParts);
            string tag = _focusingHotkeyBox.Tag.ToString();
            int currentId = _hotkeys[tag].id;
            _hotkeys[tag] = (modifiers, KeyInterop.VirtualKeyFromKey(realKey), currentId);
            RegisterAllHotkeys();
            Keyboard.ClearFocus(); _focusingHotkeyBox = null;
        }
        private void RegisterAllHotkeys()
        {
            var handle = new WindowInteropHelper(this).Handle;
            if (handle == IntPtr.Zero) return;
            foreach (var kvp in _hotkeys) NativeMethods.UnregisterHotKey(handle, kvp.Value.id);
            foreach (var kvp in _hotkeys) NativeMethods.RegisterHotKey(handle, kvp.Value.id, (uint)kvp.Value.modifier, (uint)kvp.Value.key);
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            Helpers.WindowHelper.ApplySystemBackdrop(this);
            var helper = new WindowInteropHelper(this);
            HwndSource.FromHwnd(helper.Handle).AddHook(WndProc);
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0312)
            {
                int id = wParam.ToInt32();
                if (id == _hotkeys["Toggle"].id)
                {
                    _isDimmerEnabled = !_isDimmerEnabled;
                    if (_isDimmerEnabled) System.Media.SystemSounds.Asterisk.Play(); else System.Media.SystemSounds.Hand.Play();
                }
                else if (id == _hotkeys["Darker"].id || id == _hotkeys["Lighter"].id)
                {
                    double delta = (id == _hotkeys["Darker"].id) ? 5 : -5;
                    if (MonitorTabs.SelectedItem is MonitorProfile profile) profile.Opacity = Math.Clamp(profile.Opacity + delta, 0, 95);
                }
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void ShowStartupNotification()
        {
            if (_notifyIcon != null && _notifyIcon.Visible)
            {
                string msg = "Started in background.";
                if (SelectedLanguageIndex == 1) msg = "バックグラウンドで起動しました。";
                
                _notifyIcon.ShowBalloonTip(3000, Strings.AppTitle, msg, WinForms.ToolTipIcon.Info);
            }
        }

        private void SetupTrayIcon()
        {
            string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
            System.Drawing.Icon appIcon = System.Drawing.SystemIcons.Application;
            if (File.Exists(iconPath)) { try { appIcon = new System.Drawing.Icon(iconPath); } catch { } }
            _notifyIcon = new WinForms.NotifyIcon { Icon = appIcon, Visible = true, Text = "Focus Dimmer" };
            var menu = new WinForms.ContextMenuStrip();
            menu.Items.Add("設定", null, (s, e) => { Show(); ShowInTaskbar = true; WindowState = WindowState.Normal; Activate(); });
            menu.Items.Add("終了", null, (s, e) => { _reallyExit = true; Close(); });
            _notifyIcon.ContextMenuStrip = menu;
            _notifyIcon.DoubleClick += (s, e) => { Show(); ShowInTaskbar = true; WindowState = WindowState.Normal; Activate(); };
        }
    }
}