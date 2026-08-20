using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Brushes = System.Windows.Media.Brushes;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using FocusDimmer.Models;
using FocusDimmer.Services;
using FocusDimmer.Helpers;
using FocusDimmer.ViewModels;

namespace FocusDimmer.Components
{
    public class DimmerOverlay : IDisposable
    {
        public MonitorProfile LinkedProfile { get; private set; }
        private Window? _window;
        private Path? _path;
        private SolidColorBrush? _brush;
        private CombinedGeometry? _finalGeo;
        private GeometryGroup? _holesGroup;
        private RectangleGeometry? _bgRect;
        private IntPtr _myHandle = IntPtr.Zero;

        private IntPtr _lastTargetHwnd = IntPtr.Zero;
        private NativeMethods.RECT _lastTargetRect = new NativeMethods.RECT();
        private bool _lastExcludeTaskbar = false;
        private bool _lastExcludeTopmost = false;
        private bool _lastUseTightFrame = false;
        private double _lastMargin = -999;
        private int _lastPopupCount = 0;
        private NativeMethods.RECT _lastSpecialRectSample = new NativeMethods.RECT();
        private string _lastAlwaysBright = "";
        private string _lastAlwaysDark = "";
        private bool _lastDimDesktopOnly = false;
        private bool _lastForceNoHoles = false;

        private bool _isCacheValid = false;

        private List<NativeMethods.RECT> _reusableSpecialWindows = new List<NativeMethods.RECT>();

        private DispatcherTimer _delayTimer;
        private DispatcherTimer _fadeTimer;  // Manual fade animation timer
        private bool _isCurrentlyActiveState = false;
        private bool _wasIdle = false;
        
        // Manual fade animation state
        private bool _isFadingToTransparent = false;
        private DateTime _fadeStartTime;
        private double _fadeDuration;
        private byte _fadeStartAlpha;
        private Color _fadeBaseColor;

        // Cached animation for StartBreathSequence to prevent handler accumulation
        private ColorAnimation? _breathAnimation;

        private bool _disposed = false;

        public DimmerOverlay(MonitorProfile profile)
        {
            LinkedProfile = profile;

            _brush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            _holesGroup = new GeometryGroup { FillRule = FillRule.Nonzero };
            _bgRect = new RectangleGeometry(new Rect(-20000, -20000, 60000, 60000));
            _finalGeo = new CombinedGeometry(GeometryCombineMode.Exclude, _bgRect, _holesGroup);

            _path = new Path { Data = _finalGeo, Fill = _brush };
            RenderOptions.SetEdgeMode(_path, EdgeMode.Aliased);

            var bounds = profile.ScreenRef?.Bounds ?? new System.Drawing.Rectangle(0, 0, 1920, 1080);
            _window = new Window
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                ShowInTaskbar = false,
                Topmost = true,
                Content = _path,
                IsHitTestVisible = false,
                Left = bounds.Left - 1,
                Top = bounds.Top - 1,
                Width = bounds.Width + 2,
                Height = bounds.Height + 2
            };

            _window.SourceInitialized += (s, e) => {
                var helper = new WindowInteropHelper(_window);
                _myHandle = helper.Handle;
                int exStyle = NativeMethods.GetWindowLong(_myHandle, NativeMethods.GWL_EXSTYLE);
                NativeMethods.SetWindowLong(_myHandle, NativeMethods.GWL_EXSTYLE, exStyle | NativeMethods.WS_EX_TRANSPARENT | NativeMethods.WS_EX_TOOLWINDOW | NativeMethods.WS_EX_NOACTIVATE);
                WindowHelper.DisableBackdropAndBlur(_myHandle);
            };

            _window.Loaded += (s, e) => {
                if (_myHandle == IntPtr.Zero)
                {
                    var helper = new WindowInteropHelper(_window);
                    _myHandle = helper.Handle;
                }
                int exStyle = NativeMethods.GetWindowLong(_myHandle, NativeMethods.GWL_EXSTYLE);
                NativeMethods.SetWindowLong(_myHandle, NativeMethods.GWL_EXSTYLE, exStyle | NativeMethods.WS_EX_TRANSPARENT | NativeMethods.WS_EX_TOOLWINDOW | NativeMethods.WS_EX_NOACTIVATE);
                WindowHelper.DisableBackdropAndBlur(_myHandle);
                UpdateWindowBounds();
            };

            LinkedProfile.PropertyChanged += OnProfilePropertyChanged;
            
            var vm = GetViewModel();
            if (vm != null)
            {
                vm.PropertyChanged += OnViewModelPropertyChanged;
            }

            _delayTimer = new DispatcherTimer();
            _delayTimer.Tick += DelayTimer_Tick;
            
            _fadeTimer = new DispatcherTimer();
            _fadeTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60fps
            _fadeTimer.Tick += FadeTimer_Tick;
        }

        private static MainViewModel? GetViewModel()
        {
            if (System.Windows.Application.Current?.MainWindow is MainWindow mw && mw.DataContext is MainViewModel vm)
            {
                return vm;
            }
            return null;
        }

        private void OnProfilePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MonitorProfile.Opacity) || e.PropertyName == nameof(MonitorProfile.OverlayColorHex) || e.PropertyName == nameof(MonitorProfile.IdleDimOpacity)) ApplyAppearanceImmediately();
            if (e.PropertyName == nameof(MonitorProfile.Margin) ||
                e.PropertyName == nameof(MonitorProfile.ExcludeTaskbar) ||
                e.PropertyName == nameof(MonitorProfile.ExcludeTopmost) ||
                e.PropertyName == nameof(MonitorProfile.UseTightFrame) ||
                e.PropertyName == nameof(MonitorProfile.DimDesktopOnly))
            {
                InvalidateCache();
            }
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.AlwaysBrightList) ||
                e.PropertyName == nameof(MainViewModel.AlwaysDarkList) ||
                e.PropertyName == nameof(MainViewModel.IgnoreList))
            {
                InvalidateCache();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            LinkedProfile.PropertyChanged -= OnProfilePropertyChanged;
            var vm = GetViewModel();
            if (vm != null)
            {
                vm.PropertyChanged -= OnViewModelPropertyChanged;
            }

            
            _delayTimer?.Stop();
            _fadeTimer?.Stop();

            if (_breathAnimation != null)
            {
                var anim = _breathAnimation;
                anim.Completed -= BreathAnimation_Completed;
            }

            try
            {
                _window?.Close();
            }
            catch { }

            _window = null;
            _path = null;
            _brush = null;
            _holesGroup = null;
        }

        public void EnsureTopmost()
        {
            if (_window == null || _myHandle == IntPtr.Zero) return;
            int exStyle = NativeMethods.GetWindowLong(_myHandle, NativeMethods.GWL_EXSTYLE);
            if ((exStyle & NativeMethods.WS_EX_TOPMOST) == 0)
            {
                ForceResetTopmost();
            }
        }

        public void ForceResetTopmost()
        {
            if (_window == null) return;
            try
            {
                _window.Topmost = false;
                _window.Topmost = true;
                NativeMethods.SetWindowPos(_myHandle, new IntPtr(-1), 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0010);
            }
            catch { }
        }

        private void UpdateWindowBounds()
        {
            var win = _window;
            if (win == null) return;
            var source = PresentationSource.FromVisual(win);
            if (source?.CompositionTarget == null) return;
            double scaleX = source.CompositionTarget.TransformToDevice.M11;
            double scaleY = source.CompositionTarget.TransformToDevice.M22;
            var bounds = LinkedProfile.ScreenRef?.Bounds ?? new System.Drawing.Rectangle(0, 0, 1920, 1080); // Fix CS8602

            win.Left = (bounds.Left - 1) / scaleX;
            win.Top = (bounds.Top - 1) / scaleY;
            win.Width = (bounds.Width + 2) / scaleX;
            win.Height = (bounds.Height + 2) / scaleY;
            if (_bgRect != null) _bgRect.Rect = new Rect(0, 0, win.Width, win.Height);
        }

        public void Show() => _window?.Show();
        public void Close() => _window?.Close();
        public void SetVisibility(bool visible) { if (_window != null) _window.Visibility = visible ? Visibility.Visible : Visibility.Hidden; }

        public void InvalidateCache()
        {
            _lastTargetHwnd = IntPtr.Zero;
            _lastTargetRect = new NativeMethods.RECT();
            _lastMargin = -999;
            _lastPopupCount = 0;
            _lastSpecialRectSample = new NativeMethods.RECT();
            _lastAlwaysBright = "";
            _lastAlwaysDark = "";
            _lastDimDesktopOnly = false;
            _lastForceNoHoles = false;
            _lastExcludeTaskbar = false;
            _lastExcludeTopmost = false;
            _lastUseTightFrame = false;
            _isCacheValid = false;
        }

        private Color GetBaseColor()
        {
            try { return (Color)ColorConverter.ConvertFromString(LinkedProfile.OverlayColorHex); } catch { return Colors.Black; }
        }

        private double _lastAppliedOpacity = -1;
        private double _lastAppliedIdleOpacity = -1;


        public void UpdateState(IntPtr foregroundHwnd, bool shouldDim, bool windowChanged, bool forceNoHoles, bool isIdle)
        {
            // DEBUG: Log all parameters and current state

            
            // If settings changed (Opacity slider moved), we might need to update even if state didn't change.
            bool opacityChanged = false;
            double currentTargetOpacity = isIdle ? LinkedProfile.IdleDimOpacity : LinkedProfile.Opacity;
            double lastApplied = isIdle ? _lastAppliedIdleOpacity : _lastAppliedOpacity;

            if (Math.Abs(lastApplied - currentTargetOpacity) > 0.01) opacityChanged = true;

            if (_isCurrentlyActiveState != shouldDim)
            {
                _isCurrentlyActiveState = shouldDim;

                if (shouldDim)
                {
                    if (windowChanged)
                    {
                        if (LinkedProfile.DurationBrighten <= 0.05)
                        {
                            // Fadeout is instant (skip the bright phase)
                            // But still do delay and fade to dark (fade-in)
                            _delayTimer.Stop();
                            _brush?.BeginAnimation(SolidColorBrush.ColorProperty, null);
                            if (_brush != null) _brush.Color = Color.FromArgb(0, 0, 0, 0);  // Go transparent instantly
                            
                            // Start delay timer for fade to dark
                            _delayTimer.Interval = TimeSpan.FromSeconds(LinkedProfile.DelayDarken);
                            _delayTimer.Start();
                        }
                        else
                        {
                            StartBreathSequence();
                        }
                    }
                    else
                    {
                        _delayTimer.Stop();
                        _delayTimer.Interval = TimeSpan.FromSeconds(LinkedProfile.DelayDarken);
                        _delayTimer.Start();
                        _brush?.BeginAnimation(SolidColorBrush.ColorProperty, null);
                        if (_brush != null) _brush.Color = Color.FromArgb(0, 0, 0, 0);

                        if (isIdle)
                        {
                            _delayTimer.Stop();
                            FadeToDark(1.0, LinkedProfile.IdleDimOpacity);
                        }
                    }
                }
                else
                {
                    // State transition: Dark -> Bright (e.g., clicking desktop)
                    // Use the SAME approach as StartBreathSequence which works correctly
                    _delayTimer.Stop();
                    _fadeTimer.Stop();
                    _isFadingToTransparent = false;
                    
                    double duration = _wasIdle ? 1.0 : LinkedProfile.DurationBrighten;
                    
                    if (duration > 0.01)
                    {
                        // Same as StartBreathSequence: just start a new animation
                        // DO NOT clear existing animation - just override it with a new one
                        var fadeAnim = new ColorAnimation
                        {
                            To = Color.FromArgb(0, 0, 0, 0),
                            Duration = new Duration(TimeSpan.FromSeconds(duration)),
                            FillBehavior = FillBehavior.HoldEnd
                        };
                        _brush?.BeginAnimation(SolidColorBrush.ColorProperty, fadeAnim);
                    }
                    else
                    {
                        // Instant transition
                        _brush?.BeginAnimation(SolidColorBrush.ColorProperty, null);
                        if (_brush != null) _brush.Color = Color.FromArgb(0, 0, 0, 0);
                    }
                }
            }
            else
            {
                if (shouldDim) 
                {
                    bool idleChanged = (_wasIdle != isIdle);
                    
                    if (idleChanged)
                    {
                         double duration = 1.0;
                         // Crucial Fix: If switching state (Idle <-> Active), animate to new target opacity
                         _delayTimer.Stop();
                         FadeToDark(duration, currentTargetOpacity);
                    }
                    else if (windowChanged || opacityChanged)
                    {
                        bool skipAnimation = LinkedProfile.DimDesktopOnly;

                        if (opacityChanged && !windowChanged)
                        {
                            // Preset switched or slider moved. Animate smoothly from CURRENT opacity to TARGET opacity.
                            _delayTimer.Stop();
                            double duration = (currentTargetOpacity > lastApplied) ? LinkedProfile.DurationDarken : LinkedProfile.DurationBrighten;
                            if (duration < 0.2) duration = 0.2; // Minimum duration for smoothness
                            FadeToDark(duration, currentTargetOpacity);
                        }
                        else if (!skipAnimation)
                        {
                            if (LinkedProfile.DurationBrighten <= 0.05)
                            {
                                _delayTimer.Stop();
                                _brush?.BeginAnimation(SolidColorBrush.ColorProperty, null);
                                byte targetAlpha = (byte)(currentTargetOpacity / 100.0 * 255);
                                var c = GetBaseColor();
                                if (_brush != null) _brush.Color = Color.FromArgb(targetAlpha, c.R, c.G, c.B);
                            }
                            else
                            {
                                StartBreathSequence();
                            }
                        }
                    }
                }
                else
                {
                    // Already marked as bright, but check if manual fade is running
                    // If not running and brush is still dark, start manual fade
                    if (!_isFadingToTransparent && _brush != null)
                    {
                        byte currentAlpha = _brush.Color.A;
                        if (currentAlpha > 10)  // Brush is not transparent
                        {
                            // Start manual fade animation
                            double duration = LinkedProfile.DurationBrighten;
                            if (duration > 0.01)
                            {
                                _delayTimer.Stop();
                                _brush?.BeginAnimation(SolidColorBrush.ColorProperty, null);
                                
                                _isFadingToTransparent = true;
                                _fadeStartTime = DateTime.Now;
                                _fadeDuration = duration;
                                _fadeStartAlpha = currentAlpha;
                                _fadeBaseColor = GetBaseColor();
                                _fadeTimer.Start();
                            }
                            else
                            {
                                _brush?.BeginAnimation(SolidColorBrush.ColorProperty, null);
                                if (_brush != null) _brush.Color = Color.FromArgb(0, 0, 0, 0);
                            }
                        }
                    }
                }
            }

            _wasIdle = isIdle;
            if (isIdle) _lastAppliedIdleOpacity = LinkedProfile.IdleDimOpacity;
            else _lastAppliedOpacity = LinkedProfile.Opacity;
            
            UpdateHoles(foregroundHwnd, forceNoHoles);
        }

        private void StartBreathSequence()
        {
            _delayTimer.Stop();
            double durationBright = LinkedProfile.DurationBrighten;
            
            // Reuse or create animation to prevent event handler accumulation
            if (_breathAnimation == null)
            {
                _breathAnimation = new ColorAnimation
                {
                    To = Color.FromArgb(0, 0, 0, 0),
                    FillBehavior = FillBehavior.HoldEnd
                };
                _breathAnimation.Completed += BreathAnimation_Completed;
            }
            _breathAnimation.Duration = new Duration(TimeSpan.FromSeconds(durationBright));
            _brush?.BeginAnimation(SolidColorBrush.ColorProperty, _breathAnimation);
        }

        private void BreathAnimation_Completed(object? sender, EventArgs e)
        {
            _delayTimer.Interval = TimeSpan.FromSeconds(LinkedProfile.DelayDarken);
            _delayTimer.Start();
        }

        private void DelayTimer_Tick(object? sender, EventArgs e)
        {
            _delayTimer.Stop();
            FadeToDark(LinkedProfile.DurationDarken);
        }

        private void FadeTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isFadingToTransparent)
            {

                _fadeTimer.Stop();
                return;
            }
            
            double elapsed = (DateTime.Now - _fadeStartTime).TotalSeconds;
            double progress = Math.Min(1.0, elapsed / _fadeDuration);
            
            // Calculate current alpha using linear interpolation
            byte currentAlpha = (byte)(_fadeStartAlpha * (1.0 - progress));
            

            
            // Update brush color directly
            if (_brush != null) _brush.Color = Color.FromArgb(currentAlpha, _fadeBaseColor.R, _fadeBaseColor.G, _fadeBaseColor.B);
            
            if (progress >= 1.0)
            {
                // Animation complete

                _isFadingToTransparent = false;
                _fadeTimer.Stop();
                if (_brush != null) _brush.Color = Color.FromArgb(0, 0, 0, 0);
            }
        }

        private void FadeToDark(double duration, double? targetOpacity = null)
        {
            double op = targetOpacity ?? LinkedProfile.Opacity;
            byte targetAlpha = (byte)(op / 100.0 * 255);
            var c = GetBaseColor();
            var anim = new ColorAnimation
            {
                To = Color.FromArgb(targetAlpha, c.R, c.G, c.B),
                Duration = new Duration(TimeSpan.FromSeconds(duration)),
                FillBehavior = FillBehavior.HoldEnd
            };
            _brush?.BeginAnimation(SolidColorBrush.ColorProperty, anim);
        }

        private void FadeToTransparent(double duration)
        {
            _delayTimer.Stop();
            var anim = new ColorAnimation
            {
                To = Color.FromArgb(0, 0, 0, 0),
                Duration = new Duration(TimeSpan.FromSeconds(duration)),
                FillBehavior = FillBehavior.HoldEnd
            };
            _brush?.BeginAnimation(SolidColorBrush.ColorProperty, anim);
        }

        private void ApplyAppearanceImmediately()
        {
            _delayTimer.Stop();
            _brush?.BeginAnimation(SolidColorBrush.ColorProperty, null);
            if (_isCurrentlyActiveState)
            {
                double op = (_wasIdle && LinkedProfile.DimWhenIdle) ? LinkedProfile.IdleDimOpacity : LinkedProfile.Opacity;
                byte targetAlpha = (byte)(op / 100.0 * 255);
                var c = GetBaseColor();
                if (_brush != null) _brush.Color = Color.FromArgb(targetAlpha, c.R, c.G, c.B);
            }
        }

        private void UpdateHoles(IntPtr targetHwnd, bool forceNoHoles)
        {
            NativeMethods.RECT currentRect = new NativeMethods.RECT();
            if (targetHwnd != IntPtr.Zero)
            {
                bool success = false;
                bool isDialog = IsDialogWindow(targetHwnd);
                if (LinkedProfile.UseTightFrame && !isDialog) success = NativeMethods.GetTightWindowRect(targetHwnd, out currentRect);
                if (!success) NativeMethods.GetWindowRect(targetHwnd, out currentRect);
            }

            _reusableSpecialWindows.Clear();
            var specialWindows = _reusableSpecialWindows;
            NativeMethods.EnumWindows((hwnd, lp) =>
            {
                if (NativeMethods.IsWindowVisible(hwnd) && hwnd != _myHandle)
                {
                    if (NativeMethods.IsWindowCloaked(hwnd)) return true;

                    bool shouldAdd = false;

                    if (LinkedProfile.DimDesktopOnly && !forceNoHoles)
                    {
                        if (!IsDesktopWindow(hwnd))
                        {
                            // Fix: Allow Dialogs (#32770) to be holes even if they are seen as Menus/Popups
                            // This ensures they are bright in "Dim All Windows" mode.
                            bool isMenu = WindowHelper.IsMenuOrPopupEx(hwnd);
                            bool isDialog = IsDialogWindow(hwnd);
                            
                            if (isDialog || (!isMenu && !IsAlwaysDarkWindow(hwnd)))
                            {
                                shouldAdd = true;
                            }
                        }
                    }
                    else
                    {
                        bool isMenu = WindowHelper.IsMenuOrPopupEx(hwnd);
                        if (!isMenu && IsAlwaysDarkWindow(hwnd)) return true;

                        bool isBrightClass = isMenu || IsAlwaysBrightWindow(hwnd);
                        bool isDialog = IsDialogWindow(hwnd);

                        // SPECIAL FIX: For Dialogs (#32770), only allow hole if it is the Active Window.
                        // This bypasses the IsIgnoredWindow logic in MainWindow, relying on raw GetForegroundWindow.
                        if (isBrightClass && isDialog)
                        {
                            IntPtr fg = NativeMethods.GetForegroundWindow();
                            if (fg != hwnd) isBrightClass = false;
                        }

                        if (forceNoHoles)
                        {
                            // In "Dim Entirely" mode, ONLY allow explicitly whitelisted items (or menus)
                            if (isBrightClass) shouldAdd = true;
                        }
                        else
                        {
                            if (isBrightClass) shouldAdd = true;
                            else if (LinkedProfile.ExcludeTopmost && ((NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE) & NativeMethods.WS_EX_TOPMOST) != 0)) shouldAdd = true;
                        }
                    }

                    if (shouldAdd)
                    {
                        NativeMethods.RECT r = new NativeMethods.RECT();
                        bool s = false;
                        bool isForceTight = WindowHelper.IsMenuOrPopupEx(hwnd) || IsAlwaysBrightWindow(hwnd);
                        bool isDialog = IsDialogWindow(hwnd);

                        if (isForceTight && !isDialog) s = NativeMethods.GetTightWindowRect(hwnd, out r);
                        if (!s && LinkedProfile.UseTightFrame && !isDialog) s = NativeMethods.GetTightWindowRect(hwnd, out r);
                        if (!s) NativeMethods.GetWindowRect(hwnd, out r);

                        // Filter out small artifacts (e.g. 10x10px bright spots)
                        if (r.Right - r.Left < 20 && r.Bottom - r.Top < 20) return true;

                        if (r.Right > r.Left && r.Bottom > r.Top) 
                        {
                             specialWindows.Add(r);
                        }
                    }
                }
                return true;
            }, IntPtr.Zero);

            string currentAlwaysBright = "";
            string currentAlwaysDark = "";
            var vm = GetViewModel();
            if (vm != null)
            {
                currentAlwaysBright = vm.AlwaysBrightList;
                currentAlwaysDark = vm.AlwaysDarkList;
            }


            bool isSame = (targetHwnd == _lastTargetHwnd) &&
                          (currentRect.Equals(_lastTargetRect)) &&
                          (_lastMargin == LinkedProfile.Margin) &&
                          (_lastExcludeTaskbar == LinkedProfile.ExcludeTaskbar) &&
                          (_lastExcludeTopmost == LinkedProfile.ExcludeTopmost) &&
                          (_lastUseTightFrame == LinkedProfile.UseTightFrame) &&
                          (specialWindows.Count == _lastPopupCount) &&
                          (specialWindows.Count > 0 ? specialWindows[0].Equals(_lastSpecialRectSample) : true) &&
                          (_lastAlwaysBright == currentAlwaysBright) &&
                          (_lastAlwaysDark == currentAlwaysDark) &&
                          (_lastDimDesktopOnly == LinkedProfile.DimDesktopOnly) &&
                          (_lastForceNoHoles == forceNoHoles) &&
                          _isCacheValid;
            
            if (isSame) return;

            _lastTargetHwnd = targetHwnd;
            _lastTargetRect = currentRect;
            _lastMargin = LinkedProfile.Margin;
            _lastExcludeTaskbar = LinkedProfile.ExcludeTaskbar;
            _lastExcludeTopmost = LinkedProfile.ExcludeTopmost;
            _lastUseTightFrame = LinkedProfile.UseTightFrame;
            _lastPopupCount = specialWindows.Count;
            if (specialWindows.Count > 0) _lastSpecialRectSample = specialWindows[0];
            _lastAlwaysBright = currentAlwaysBright;
            _lastAlwaysDark = currentAlwaysDark;
            _lastDimDesktopOnly = LinkedProfile.DimDesktopOnly;
            _lastForceNoHoles = forceNoHoles;
            _isCacheValid = true;

            _holesGroup?.Children.Clear();

            if (_window == null) return;
            var source = PresentationSource.FromVisual(_window);
            if (source?.CompositionTarget == null) return;
            double scaleX = source.CompositionTarget.TransformToDevice.M11;
            double scaleY = source.CompositionTarget.TransformToDevice.M22;

            if (_isCurrentlyActiveState || (_brush != null && _brush.Color.A > 0))
            {
                // Active Window Hole: Skip if forceNoHoles
                // FIX: Allow Active Hole even if DimDesktopOnly is true (to ensure active window is always visible)
                if (!forceNoHoles && targetHwnd != IntPtr.Zero)
                {
                    AddHoleForRect(currentRect, LinkedProfile.Margin, scaleX, scaleY);
                }

                // Taskbar Hole
                if (!forceNoHoles && LinkedProfile.ExcludeTaskbar)
                {
                    bool tbSuccess = false;
                    NativeMethods.RECT r = new NativeMethods.RECT();
                    if (LinkedProfile.UseTightFrame) tbSuccess = NativeMethods.GetTightWindowRect(NativeMethods.FindWindow("Shell_TrayWnd", null), out r);
                    if (!tbSuccess) NativeMethods.GetWindowRect(NativeMethods.FindWindow("Shell_TrayWnd", null), out r);
                    AddHoleForRect(r, 0, scaleX, scaleY);
                }

                foreach (var rect in specialWindows)
                {
                    AddHoleForRect(rect, 0, scaleX, scaleY);
                }
            }
        }
            
        private bool IsDesktopWindow(IntPtr hwnd)
        {
            StringBuilder sb = new StringBuilder(256);
            NativeMethods.GetClassName(hwnd, sb, sb.Capacity);
            string className = sb.ToString();
            return (className == "Progman" || className == "WorkerW");
        }

        private bool IsDialogWindow(IntPtr hwnd)
        {
            StringBuilder sb = new StringBuilder(256);
            NativeMethods.GetClassName(hwnd, sb, sb.Capacity);
            return sb.ToString() == "#32770";
        }

        private bool IsAlwaysDarkWindow(IntPtr hwnd)
        {
            var vm = GetViewModel();
            if (vm != null)
            {
                if (IsProcessInList(hwnd, vm.AlwaysDarkList)) return true;
            }

            StringBuilder sb = new StringBuilder(256);
            NativeMethods.GetClassName(hwnd, sb, sb.Capacity);
            string cls = sb.ToString();

            if (cls.Contains("SnapLayout") || cls == "MagUIClass") return true;

            NativeMethods.GetWindowThreadProcessId(hwnd, out uint pid);
            string procName = ProcessInfoHelper.GetProcessName(pid);
            if (procName == "explorer")
            {
                if (WindowHelper.IsMenuOrPopupEx(hwnd)) return false;

                if (cls != "CabinetWClass" && cls != "ExploreWClass" &&
                    cls != "Shell_TrayWnd" && cls != "Progman" && cls != "WorkerW" && cls != "#32770")
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsAlwaysBrightWindow(IntPtr hwnd)
        {
            var vm = GetViewModel();
            if (vm != null)
            {
                if (IsProcessInList(hwnd, vm.AlwaysBrightList)) return true;
            }


            StringBuilder sb = new StringBuilder(256);
            NativeMethods.GetClassName(hwnd, sb, sb.Capacity);
            string cls = sb.ToString();

            if (cls == "CiceroUIWndFrame") return true;
            if (cls == "#32770") return true; // Dialogs (manually handled in UpdateHoles for active-only)
            if (cls == "InputIndicator") return true;
            if (cls.StartsWith("Microsoft.IME")) return true;
            if (cls == "Muser") return true;
            if (cls == "SearchPane") return true;

            if (cls == "HwndWrapper[DefaultDomain;;]")
            {
                int style = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
                if ((style & NativeMethods.WS_EX_TOPMOST) != 0) return true;
            }
            return false;
        }

        private bool IsProcessInList(IntPtr hwnd, string list)
        {
            if (string.IsNullOrWhiteSpace(list)) return false;
            NativeMethods.GetWindowThreadProcessId(hwnd, out uint pid);
            string procName = ProcessInfoHelper.GetProcessName(pid);
            var items = list.Split(',').Select(x => x.Trim().ToLower().Replace(".exe", "")).Where(x => !string.IsNullOrEmpty(x)).ToList();
            return items.Contains(procName);
        }

        private void AddHoleForRect(NativeMethods.RECT r, double margin, double scaleX, double scaleY)
        {
            if (LinkedProfile.ScreenRef == null) return;
            double width = (r.Right - r.Left);
            double height = (r.Bottom - r.Top);
            if (width <= 1 || height <= 1) return;

            double physLeft = r.Left - LinkedProfile.ScreenRef.Bounds.Left;
            double physTop = r.Top - LinkedProfile.ScreenRef.Bounds.Top;

            double left = (physLeft + 1) / scaleX - margin;
            double top = (physTop + 1) / scaleY - margin;
            double w = width / scaleX + (margin * 2);
            double h = height / scaleY + (margin * 2);

            if (left + w > 0 && top + h > 0)
            {
                var rGeo = new RectangleGeometry(new Rect(left, top, w, h));
                if (_holesGroup != null) _holesGroup.Children.Add(rGeo);
            }
        }
    }
}

