using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

// Aliases to resolve ambiguities
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Point = System.Windows.Point;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;


namespace FocusDimmer.Views
{
    public partial class ColorPickerWindow : Window, INotifyPropertyChanged
    {
        public Color SelectedColor { get; private set; }

        private double _hue = 0;
        private double _sat = 1;
        private double _val = 1;

        private bool _isUpdating = false;

        public ColorPickerWindow(Color initialColor)
        {
            InitializeComponent();
            SelectedColor = initialColor;
            
            // Extract HSV from initialColor (Simple conversion)
            System.Drawing.Color c = System.Drawing.Color.FromArgb(initialColor.A, initialColor.R, initialColor.G, initialColor.B);
            _hue = c.GetHue();
            _sat = c.GetSaturation() == 0 ? 0 : c.GetSaturation(); // Note: System.Drawing uses HSL, not HSV. Adjusting manually.
            
            // Re-calc to pure HSV for the implementation
            ColorToHSV(initialColor, out _hue, out _sat, out _val);

            UpdateUIFromHSV();
        }

        public SolidColorBrush CurrentColorBrush => new SolidColorBrush(SelectedColor);

        private byte _r; public byte R { get => _r; set { if (_r != value) { _r = value; OnRGBChanged(); NotifyPropertyChanged(); } } }
        private byte _g; public byte G { get => _g; set { if (_g != value) { _g = value; OnRGBChanged(); NotifyPropertyChanged(); } } }
        private byte _b; public byte B { get => _b; set { if (_b != value) { _b = value; OnRGBChanged(); NotifyPropertyChanged(); } } }

        private string _hexString;
        public string HexString 
        { 
            get => _hexString; 
            set 
            { 
                if (_hexString != value) 
                { 
                    _hexString = value; 
                    NotifyPropertyChanged(); 
                } 
            } 
        }

        private void OnRGBChanged()
        {
            if (_isUpdating) return;
            _isUpdating = true;
            SelectedColor = Color.FromRgb(R, G, B);
            ColorToHSV(SelectedColor, out _hue, out _sat, out _val);
            UpdateHex();
            UpdateVisuals();
            _isUpdating = false;
        }

        private void UpdateUIFromHSV()
        {
            _isUpdating = true;
            SelectedColor = HSVToColor(_hue, _sat, _val);
            _r = SelectedColor.R;
            _g = SelectedColor.G;
            _b = SelectedColor.B;
            UpdateHex();
            UpdateVisuals();
            NotifyPropertyChanged(nameof(R));
            NotifyPropertyChanged(nameof(G));
            NotifyPropertyChanged(nameof(B));
            _isUpdating = false;
        }

        private void UpdateVisuals()
        {
            ColorArea.Background = new SolidColorBrush(HSVToColor(_hue, 1, 1));
            Canvas.SetLeft(HueThumb, (_hue / 360.0) * HueSlider.ActualWidth - 2);
            Canvas.SetLeft(ColorThumb, _sat * ColorArea.Width - 5);
            Canvas.SetTop(ColorThumb, (1 - _val) * ColorArea.Height - 5);
            Canvas.SetLeft(ColorThumbBlack, _sat * ColorArea.Width - 6);
            Canvas.SetTop(ColorThumbBlack, (1 - _val) * ColorArea.Height - 6);

            NotifyPropertyChanged(nameof(CurrentColorBrush));
        }

        private void UpdateHex()
        {
            _hexString = $"#{SelectedColor.R:X2}{SelectedColor.G:X2}{SelectedColor.B:X2}";
            NotifyPropertyChanged(nameof(HexString));
        }

        // HSV Logic
        private Color HSVToColor(double h, double s, double v)
        {
            double c = v * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = v - c;

            double r = 0, g = 0, b = 0;
            if (h < 60) { r = c; g = x; b = 0; }
            else if (h < 120) { r = x; g = c; b = 0; }
            else if (h < 180) { r = 0; g = c; b = x; }
            else if (h < 240) { r = 0; g = x; b = c; }
            else if (h < 300) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            return Color.FromRgb((byte)((r + m) * 255), (byte)((g + m) * 255), (byte)((b + m) * 255));
        }

        private void ColorToHSV(Color color, out double h, out double s, out double v)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            v = max;
            s = max == 0 ? 0 : delta / max;

            if (delta == 0) h = 0;
            else if (max == r) h = 60 * (((g - b) / delta) % 6);
            else if (max == g) h = 60 * (((b - r) / delta) + 2);
            else h = 60 * (((r - g) / delta) + 4);

            if (h < 0) h += 360;
        }

        // --- Event Handlers ---

        private bool _isDraggingHue = false;
        private bool _isDraggingSV = false;

        private void HueSlider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDraggingHue = true;
            HueSlider.CaptureMouse();
            UpdateHue(e.GetPosition(HueSlider));
        }
        private void HueSlider_MouseUp(object sender, MouseButtonEventArgs e) { _isDraggingHue = false; HueSlider.ReleaseMouseCapture(); }
        private void HueSlider_MouseMove(object sender, MouseEventArgs e) { if (_isDraggingHue) UpdateHue(e.GetPosition(HueSlider)); }

        private void UpdateHue(Point p)
        {
            double w = HueSlider.ActualWidth;
            if (w == 0) return;
            double percent = Math.Clamp(p.X / w, 0, 1);
            _hue = percent * 360;
            UpdateUIFromHSV();
        }

        private void ColorArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDraggingSV = true;
            ColorArea.CaptureMouse();
            UpdateSV(e.GetPosition(ColorArea));
        }
        private void ColorArea_MouseUp(object sender, MouseButtonEventArgs e) { _isDraggingSV = false; ColorArea.ReleaseMouseCapture(); }
        private void ColorArea_MouseMove(object sender, MouseEventArgs e) { if (_isDraggingSV) UpdateSV(e.GetPosition(ColorArea)); }

        private void UpdateSV(Point p)
        {
            double w = ColorArea.ActualWidth;
            double h = ColorArea.ActualHeight;
            if (w == 0 || h == 0) return;
            
            _sat = Math.Clamp(p.X / w, 0, 1);
            _val = Math.Clamp(1 - (p.Y / h), 0, 1);
            UpdateUIFromHSV();
        }

        private void HexInput_LostFocus(object sender, RoutedEventArgs e) => ApplyHex();
        private void HexInput_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) ApplyHex(); }
        private void ApplyHex()
        {
            try
            {
                var c = (Color)ColorConverter.ConvertFromString(HexString);
                if (_isUpdating) return;
                _isUpdating = true;
                SelectedColor = c;
                ColorToHSV(SelectedColor, out _hue, out _sat, out _val);
                _r = SelectedColor.R; _g = SelectedColor.G; _b = SelectedColor.B;
                UpdateVisuals();
                NotifyPropertyChanged(nameof(R)); NotifyPropertyChanged(nameof(G)); NotifyPropertyChanged(nameof(B));
                _isUpdating = false;
            }
            catch { UpdateHex(); }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) { DialogResult = true; Close(); }
        private void CloseButton_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
