using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Win32;
using FocusDimmer.Components;
using FocusDimmer.Helpers;
using FocusDimmer.Models;
using FocusDimmer.Services;
using FocusDimmer.ViewModels;
using FocusDimmer.Views;
using WinForms = System.Windows.Forms;
using TextBox = System.Windows.Controls.TextBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Button = System.Windows.Controls.Button;
using RadioButton = System.Windows.Controls.RadioButton;
using Orientation = System.Windows.Controls.Orientation;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Brushes = System.Windows.Media.Brushes;
using SystemColors = System.Windows.SystemColors;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Application = System.Windows.Application;

namespace FocusDimmer
{
    public partial class MainWindow : FluentWindow
    {
        private static Mutex? _mutex;
        private bool _reallyExit = false;

        private readonly SettingsService _settingsService = new();
        private readonly PresetService _presetService = new();
        private readonly StoreService _storeService = new();
        private readonly HotkeyService _hotkeyService = new();

        private DimmerEngine? _dimmerEngine;
        private readonly List<DimmerOverlay> _overlays = new();
        private WinForms.NotifyIcon? _notifyIcon;

        private TextBox? _focusingHotkeyBox = null;
        private bool _ignoreAutoStartEvents = false;
        private DebugInspector? _debugInspector;

        // App Store Page (for Migration)
        private const string AppStoreUrl = "ms-windows-store://pdp/?productid=9NXHXPNJL79X";

        public MainViewModel ViewModel { get; private set; } = null!;

        public MainWindow()
        {
            _mutex = new Mutex(true, "FocusDimmer_Unique_Instance_Mutex", out bool createdNew);
            if (!createdNew)
            {
                _reallyExit = true;
                Application.Current.Shutdown();
                return;
            }

            ViewModel = new MainViewModel(_settingsService, _presetService, _storeService);
            DataContext = ViewModel;

            InitializeComponent();

            RestoreWindowBounds();
            SetupTrayIcon();

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

            InitializeAppAsync();
        }

        #region Initialization

        private async void InitializeAppAsync()
        {
            var settings = ViewModel.GetCurrentSettings();

#if LITE_VERSION
            ViewModel.IsPro = false;
#elif DEBUG
#if LEGACY_PRO
            ViewModel.IsPro = true;
#elif PRO_VERSION
            ViewModel.IsPro = true;
#else
            if (await _storeService.InitializeAsync(settings))
            {
                _settingsService.SaveImmediately(settings);
            }
            ViewModel.IsPro = _storeService.IsPro;
#endif
#else
#if PRO_VERSION
            ViewModel.IsPro = true;
#else
            if (await _storeService.InitializeAsync(settings))
            {
                _settingsService.SaveImmediately(settings);
            }
            ViewModel.IsPro = _storeService.IsPro;
#endif
#endif

            InitializeMonitors();

            if (MonitorTabs.Items.Count > 0 && MonitorTabs.SelectedIndex < 0)
            {
                MonitorTabs.SelectedIndex = 0;
            }

            // DimmerEngine の初期化と起動
            _dimmerEngine = new DimmerEngine(_overlays);
            _dimmerEngine.ActiveProcessChanged += OnActiveProcessChanged;
            _dimmerEngine.Start();

            // Startup check
            _ignoreAutoStartEvents = true;
            AutoStartCheck.IsChecked = await StartupManager.IsStartupEnabledAsync();
            _ignoreAutoStartEvents = false;

            ApplyAccentColorFix();
        }

        private void InitializeMonitors()
        {
            foreach (var ov in _overlays) ov.Dispose();
            _overlays.Clear();
            ViewModel.MonitorProfiles.Clear();

            var settings = ViewModel.GetCurrentSettings();

            foreach (var screen in WinForms.Screen.AllScreens)
            {
                var profile = new MonitorProfile(screen);
                var saved = settings.Profiles.FirstOrDefault(p => p.DeviceName == screen.DeviceName);
                if (saved != null) profile.ApplySettings(saved);

                if (ViewModel.IsFreeVersion) profile.OverlayColorHex = "#000000";

                profile.PropertyChanged += (s, e) =>
                {
                    if (s is MonitorProfile mp && e.PropertyName != null)
                    {
                        _presetService.SyncProfileToPreset(mp, e.PropertyName, ViewModel.SelectedGlobalPreset, ViewModel.MonitorProfiles, ViewModel.AreMonitorsLinked);
                    }
                    RequestSaveSettings();
                };

                ViewModel.MonitorProfiles.Add(profile);

                // フリー版の場合はプライマリモニターのみ減光オーバーレイを生成
                if (!ViewModel.IsFreeVersion || screen.Primary)
                {
                    var overlay = new DimmerOverlay(profile);
                    _overlays.Add(overlay);
                    overlay.Show();
                }
            }

            ViewModel.LoadPresetsFromSettings();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            WindowHelper.ApplySystemBackdrop(this);

            var handle = new WindowInteropHelper(this).Handle;
            _hotkeyService.Initialize(handle);
            _hotkeyService.ToggleHotkeyPressed += OnToggleHotkey;
            _hotkeyService.DarkerHotkeyPressed += () => AdjustActiveMonitorOpacity(5);
            _hotkeyService.LighterHotkeyPressed += () => AdjustActiveMonitorOpacity(-5);
        }

        #endregion

        #region Window State & Lifecycle

        private void RestoreWindowBounds()
        {
            var settings = ViewModel.GetCurrentSettings();
            if (settings.WindowWidth > 0 && settings.WindowHeight > 0)
            {
                this.Width = settings.WindowWidth;
                this.Height = settings.WindowHeight;
            }

            if (settings.WindowLeft > -9000 && settings.WindowTop > -9000)
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = settings.WindowLeft;
                this.Top = settings.WindowTop;

                if (this.Left < SystemParameters.VirtualScreenLeft - this.Width + 50) this.Left = SystemParameters.VirtualScreenLeft;
                if (this.Top < SystemParameters.VirtualScreenTop - this.Height + 50) this.Top = SystemParameters.VirtualScreenTop;
            }
        }

        private void RequestSaveSettings()
        {
            double w = (ActualWidth > 0) ? ActualWidth : 0;
            double h = (ActualHeight > 0) ? ActualHeight : 0;
            double l = (ActualWidth > 0) ? Left : -10000;
            double t = (ActualWidth > 0) ? Top : -10000;

            ViewModel.RequestSave(w, h, l, t);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_reallyExit && ViewModel.CloseToTray)
            {
                e.Cancel = true;
                this.Hide();
                return;
            }

            double w = (ActualWidth > 0) ? ActualWidth : 0;
            double h = (ActualHeight > 0) ? ActualHeight : 0;
            double l = (ActualWidth > 0) ? Left : -10000;
            double t = (ActualWidth > 0) ? Top : -10000;
            ViewModel.SaveImmediately(w, h, l, t);

            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }

            _dimmerEngine?.Dispose();
            _hotkeyService.Dispose();

            foreach (var overlay in _overlays)
            {
                overlay.Dispose();
            }
            _overlays.Clear();

            _mutex?.ReleaseMutex();
            _mutex?.Dispose();

            base.OnClosing(e);
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();

        #endregion

        #region Hotkey & Process Event Handlers

        private void OnToggleHotkey()
        {
            if (_dimmerEngine == null) return;
            _dimmerEngine.IsEnabled = !_dimmerEngine.IsEnabled;
            if (_dimmerEngine.IsEnabled)
            {
                System.Media.SystemSounds.Asterisk.Play();
            }
            else
            {
                System.Media.SystemSounds.Hand.Play();
            }
        }

        private void AdjustActiveMonitorOpacity(double delta)
        {
            if (MonitorTabs.SelectedItem is MonitorProfile profile)
            {
                profile.Opacity = Math.Clamp(profile.Opacity + delta, 0, 95);
            }
        }

        private void OnActiveProcessChanged(string processName)
        {
            if (!ViewModel.IsPro || ViewModel.GlobalPresets.Count == 0) return;

            string? matchingId = _presetService.FindMatchingPresetId(
                processName,
                ViewModel.GlobalPresets,
                ViewModel.DefaultPresetId,
                ViewModel.SelectedGlobalPresetId
            );

            if (matchingId != null && matchingId != ViewModel.SelectedGlobalPresetId)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    ViewModel.SelectedGlobalPresetId = matchingId;
                });
            }
        }

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
            string? tag = _focusingHotkeyBox.Tag?.ToString();
            if (string.IsNullOrEmpty(tag)) return;

            _hotkeyService.UpdateHotkey(tag, modifiers, KeyInterop.VirtualKeyFromKey(realKey));
            Keyboard.ClearFocus();
            _focusingHotkeyBox = null;
        }

        #endregion

        #region Navigation & UI Handlers

        private void NavButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string tag)
            {
                if (PageMonitors != null) PageMonitors.Visibility = (tag == "Monitors") ? Visibility.Visible : Visibility.Collapsed;
                if (PageExclusions != null) PageExclusions.Visibility = (tag == "Exclusions") ? Visibility.Visible : Visibility.Collapsed;
                if (PageHotkeys != null) PageHotkeys.Visibility = (tag == "Hotkeys") ? Visibility.Visible : Visibility.Collapsed;
                if (PagePresets != null) PagePresets.Visibility = (tag == "Presets") ? Visibility.Visible : Visibility.Collapsed;
                if (PageGeneral != null) PageGeneral.Visibility = (tag == "General") ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void LinkMonitors_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.AreMonitorsLinked && MonitorTabs.SelectedItem is MonitorProfile current)
            {
                _presetService.PropagateAllToLinkedProfiles(current, ViewModel.MonitorProfiles);
            }
            RequestSaveSettings();
        }


        private void PickColor_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsFreeVersion) { ShowProPromo(); return; }
            if (sender is Button btn && btn.DataContext is MonitorProfile profile)
            {
                try
                {
                    Color current = (Color)ColorConverter.ConvertFromString(profile.OverlayColorHex ?? "#000000");
                    var picker = new ColorPickerWindow(current) { Owner = this };
                    if (picker.ShowDialog() == true)
                    {
                        var c = picker.SelectedColor;
                        profile.OverlayColorHex = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
                    }
                }
                catch { }
            }
        }

        private void BrowseApp_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsFreeVersion) { ShowProPromo(); return; }
            AddProcessToProfile(sender, vm => vm.IgnoreList, (vm, v) => vm.IgnoreList = v);
        }

        private void BrowseBright_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsFreeVersion) { ShowProPromo(); return; }
            AddProcessToProfile(sender, vm => vm.AlwaysBrightList, (vm, v) => vm.AlwaysBrightList = v);
        }

        private void BrowseDark_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsFreeVersion) { ShowProPromo(); return; }
            AddProcessToProfile(sender, vm => vm.AlwaysDarkList, (vm, v) => vm.AlwaysDarkList = v);
        }

        private void AddProcessToProfile(object sender, Func<MainViewModel, string> getter, Action<MainViewModel, string> setter)
        {
            var dialog = new ProcessSelectionWindow(ViewModel.Strings, sender as Button);
            if (dialog.ShowDialog() == true)
            {
                string processName = dialog.SelectedProcessName;
                if (string.IsNullOrEmpty(processName)) return;

                var currentList = getter(ViewModel) ?? "";
                var items = currentList.Split(',').Select(x => x.Trim().ToLower()).ToList();
                if (!items.Contains(processName.ToLower()))
                {
                    if (string.IsNullOrWhiteSpace(currentList)) setter(ViewModel, processName);
                    else setter(ViewModel, currentList.Trim() + ", " + processName);
                }
            }
        }

        private void AddProcessToList(Func<MainViewModel, string> getter, Action<MainViewModel, string> setter, string processName)
        {
            if (string.IsNullOrEmpty(processName)) return;

            var currentList = getter(ViewModel) ?? "";
            var items = currentList.Split(',').Select(x => x.Trim().ToLower()).ToList();
            if (!items.Contains(processName.ToLower()))
            {
                if (string.IsNullOrWhiteSpace(currentList)) setter(ViewModel, processName);
                else setter(ViewModel, currentList.Trim() + ", " + processName);
                RequestSaveSettings();
            }
        }


        private async void AutoStart_Checked(object sender, RoutedEventArgs e)
        {
            if (!_ignoreAutoStartEvents)
            {
                var result = await StartupManager.EnableStartupAsync();
                if (result != StartupEnableResult.Success)
                {
                    _ignoreAutoStartEvents = true;
                    AutoStartCheck.IsChecked = false;
                    _ignoreAutoStartEvents = false;

                    if (result == StartupEnableResult.DisabledByUser)
                    {
                        System.Windows.MessageBox.Show(ViewModel.Strings.MsgStartupDisabledByUser, "Startup Disabled", MessageBoxButton.OK, MessageBoxImage.Warning);
                        try { Process.Start(new ProcessStartInfo("ms-settings:startupapps") { UseShellExecute = true }); } catch { }
                    }
                }
            }
        }

        private async void AutoStart_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_ignoreAutoStartEvents)
            {
                await StartupManager.DisableStartupAsync();
            }
        }

        private void AdminRun_Changed(object sender, RoutedEventArgs e) { }

        private void DebugInspector_Click(object sender, RoutedEventArgs e)
        {
            if (_debugInspector == null)
            {
                _debugInspector = new DebugInspector(ViewModel.Strings);
                _debugInspector.SelectedWindowCaptured += (s, data) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        var actionDialog = new InspectorActionDialog(data.ProcessName, data.Title, ViewModel.Strings);
                        actionDialog.Owner = this;
                        if (actionDialog.ShowDialog() == true)
                        {
                            if (string.Equals(actionDialog.ActionType, "Ignore", StringComparison.OrdinalIgnoreCase))
                                AddProcessToList(vm => vm.IgnoreList, (vm, v) => vm.IgnoreList = v, data.ProcessName);
                            else if (string.Equals(actionDialog.ActionType, "Bright", StringComparison.OrdinalIgnoreCase))
                                AddProcessToList(vm => vm.AlwaysBrightList, (vm, v) => vm.AlwaysBrightList = v, data.ProcessName);
                            else if (string.Equals(actionDialog.ActionType, "Dark", StringComparison.OrdinalIgnoreCase))
                                AddProcessToList(vm => vm.AlwaysDarkList, (vm, v) => vm.AlwaysDarkList = v, data.ProcessName);
                        }
                    });
                };
            }
            _debugInspector.Toggle();
        }


        private void MigrationInfo_Click(object sender, RoutedEventArgs e)
        {
            var guide = new MigrationGuideWindow(ViewModel.Strings) { Owner = this };
            guide.ShowDialog();
        }

        private void CloseBanner_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.IsLegacyBannerDismissed = true;
            e.Handled = true;
        }

        private void ShowProPromo()
        {
            OpenStore_Click(this, new RoutedEventArgs());
        }

        private async void OpenStore_Click(object sender, RoutedEventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            var (status, error) = await _storeService.RequestPurchaseAsync(handle);

            if (status == Windows.Services.Store.StorePurchaseStatus.Succeeded ||
                status == Windows.Services.Store.StorePurchaseStatus.AlreadyPurchased)
            {
                ViewModel.IsPro = _storeService.IsPro;
                if (ViewModel.IsPro)
                {
                    ViewModel.Strings.AppTitle = ViewModel.Strings.AppTitle.Replace(" (Free Lite)", "");
                    InitializeMonitors();
                    System.Windows.MessageBox.Show("Thank you for your purchase!\nPro features unlocked.", "Success");
                }
            }
            else if (status != Windows.Services.Store.StorePurchaseStatus.NotPurchased)
            {
                string errorMsg = error != null ? error.Message : "Unknown Error";
                string fullMsg = $"Purchase Failed.\nStatus: {status}\nError: {errorMsg}\n\nDo you want to open the Store Page manually?";

                var res = System.Windows.MessageBox.Show(fullMsg, "Purchase Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (res == MessageBoxResult.Yes)
                {
                    try { Process.Start(new ProcessStartInfo(AppStoreUrl) { UseShellExecute = true }); } catch { }
                }
            }
        }

        #endregion

        #region Presets UI Handlers

        private void GlobalPreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.SelectedGlobalPreset != null && !_presetService.IsApplyingPreset)
            {
                _presetService.ApplyPresetToProfiles(ViewModel.SelectedGlobalPreset, ViewModel.MonitorProfiles);
            }
        }

        private void AddGlobalPreset_Click(object sender, RoutedEventArgs e)
        {
            string name = ShowInputDialog(ViewModel.Strings.MsgEnterPresetName, ViewModel.Strings.NewPresetName, ViewModel.Strings.NewPresetName);
            if (string.IsNullOrWhiteSpace(name)) return;

            var primaryProfile = ViewModel.MonitorProfiles.FirstOrDefault(p => p.ScreenRef?.Primary == true) ?? ViewModel.MonitorProfiles.FirstOrDefault();
            var newPreset = primaryProfile != null ? Preset.FromProfile(primaryProfile, name) : new Preset { Name = name };

            ViewModel.GlobalPresets.Add(newPreset);
            ViewModel.SelectedGlobalPresetId = newPreset.Id;
            RequestSaveSettings();
        }

        private void EditGlobalPresetName_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedGlobalPreset != null)
            {
                RenamePreset(ViewModel.SelectedGlobalPreset);
            }
        }

        private void RenamePreset(Preset preset)
        {
            string newName = ShowInputDialog(ViewModel.Strings.MsgEnterPresetName, preset.Name, ViewModel.Strings.HeaderPreset);
            if (!string.IsNullOrWhiteSpace(newName) && newName != preset.Name)
            {
                preset.Name = newName;
                int idx = ViewModel.GlobalPresets.IndexOf(preset);
                if (idx >= 0)
                {
                    ViewModel.GlobalPresets[idx] = preset;
                    ViewModel.SelectedGlobalPresetId = preset.Id;
                }
                RequestSaveSettings();
            }
        }

        private void DeleteGlobalPreset_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedGlobalPreset != null)
            {
                if (ViewModel.GlobalPresets.Count <= 1) return;

                var result = System.Windows.MessageBox.Show(ViewModel.Strings.MsgConfirmDeletePreset, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var toDelete = ViewModel.SelectedGlobalPreset;
                    int idx = ViewModel.GlobalPresets.IndexOf(toDelete);
                    ViewModel.GlobalPresets.Remove(toDelete);

                    if (ViewModel.GlobalPresets.Count > 0)
                    {
                        int newIdx = Math.Min(idx, ViewModel.GlobalPresets.Count - 1);
                        ViewModel.SelectedGlobalPresetId = ViewModel.GlobalPresets[newIdx].Id;
                    }
                    RequestSaveSettings();
                }
            }
        }

        private void ManageProcessRules_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedGlobalPreset != null)
            {
                var dialog = new ProcessSelectionWindow(ViewModel.Strings, sender as Button);
                if (dialog.ShowDialog() == true)
                {
                    string processName = dialog.SelectedProcessName;
                    if (!string.IsNullOrEmpty(processName))
                    {
                        if (!ViewModel.SelectedGlobalPreset.ProcessRules.Any(r => r.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase)))
                        {
                            ViewModel.SelectedGlobalPreset.ProcessRules.Add(new ProcessPresetRule { ProcessName = processName });
                            RequestSaveSettings();
                        }
                    }
                }
            }
        }

        private string ShowInputDialog(string prompt, string defaultValue, string titleText)
        {
            var dialog = new Window
            {
                Title = titleText,
                Width = 380,
                Height = 170,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                ShowInTaskbar = false
            };

            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(32, 32, 32)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20)
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var title = new TextBlock
            {
                Text = prompt,
                Foreground = Brushes.White,
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(title, 0);

            var textBox = new TextBox
            {
                Text = defaultValue,
                FontSize = 13,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 45)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(70, 70, 70)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(8, 6, 8, 6),
                CaretBrush = Brushes.White
            };
            Grid.SetRow(textBox, 1);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0)
            };
            Grid.SetRow(buttonPanel, 2);

            var cancelBtn = new Button
            {
                Content = ViewModel.Strings.BtnCancel,
                Width = 75,
                Height = 30,
                Margin = new Thickness(0, 0, 8, 0),
                Background = new SolidColorBrush(Color.FromRgb(55, 55, 55)),
                Foreground = Brushes.White
            };
            cancelBtn.Click += (s, a) => dialog.DialogResult = false;

            var okBtn = new Button
            {
                Content = "OK",
                Width = 75,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(96, 205, 255)),
                Foreground = Brushes.Black,
                FontWeight = FontWeights.SemiBold
            };
            okBtn.Click += (s, a) => dialog.DialogResult = true;

            buttonPanel.Children.Add(cancelBtn);
            buttonPanel.Children.Add(okBtn);

            grid.Children.Add(title);
            grid.Children.Add(textBox);
            grid.Children.Add(buttonPanel);
            border.Child = grid;
            dialog.Content = border;

            textBox.KeyDown += (s, a) => { if (a.Key == Key.Enter) dialog.DialogResult = true; };
            dialog.Loaded += (s, a) => { textBox.Focus(); textBox.SelectAll(); };

            if (dialog.ShowDialog() == true) return textBox.Text;
            return defaultValue;
        }

        #endregion

        #region System Events & Helpers

        private async void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                await Task.Delay(2000);
                _dimmerEngine?.ResetState();

                foreach (var overlay in _overlays)
                {
                    overlay.InvalidateCache();
                    overlay.ForceResetTopmost();
                }

                _hotkeyService.RegisterAll();
                _dimmerEngine?.ForceTick();
            }
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General || e.Category == UserPreferenceCategory.Color)
            {
                ApplyAccentColorFix();
            }
        }

        private void ApplyAccentColorFix()
        {
            try
            {
                Color sysAccent = SystemColors.HighlightColor;
                ColorHelper.ColorToHsv(sysAccent, out double h, out double s, out double v);
                s = Math.Clamp(s * 0.8, 0, 1.0);
                v = Math.Clamp(v * 1.15, 0, 1.0);
                Color hoverColor = ColorHelper.ColorFromHsv(h, s, v);

                Application.Current.Resources["AccentColor"] = new SolidColorBrush(sysAccent);
                Application.Current.Resources["AccentHover"] = new SolidColorBrush(hoverColor);
            }
            catch { }
        }

        private void SetupTrayIcon()
        {
            var menu = new WinForms.ContextMenuStrip();

            var openItem = new WinForms.ToolStripMenuItem("Focus Dimmer", null, (s, e) =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();
            })
            {
                Font = new System.Drawing.Font(menu.Font, System.Drawing.FontStyle.Bold)
            };

            var exitItem = new WinForms.ToolStripMenuItem(ViewModel.Strings.BtnClose, null, (s, e) =>
            {
                _reallyExit = true;
                this.Close();
            });

            menu.Items.Add(openItem);
            menu.Items.Add(new WinForms.ToolStripSeparator());
            menu.Items.Add(exitItem);

            _notifyIcon = new WinForms.NotifyIcon
            {
                Text = ViewModel.Strings.AppTitle,
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location),
                Visible = true,
                ContextMenuStrip = menu
            };

            _notifyIcon.DoubleClick += (s, e) =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();
            };
        }

        public void ShowStartupNotification()
        {
            if (_notifyIcon != null && _notifyIcon.Visible)
            {
                string msg = ViewModel.SelectedLanguageIndex == 1 ? "バックグラウンドで起動しました。" : "Started in background.";
                _notifyIcon.ShowBalloonTip(3000, ViewModel.Strings.AppTitle, msg, WinForms.ToolTipIcon.Info);
            }
        }

        #endregion
    }
}