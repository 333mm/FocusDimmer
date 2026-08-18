using System;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace FocusDimmer.Helpers
{
    public static class ColorHelper
    {
        public static void ColorToHsv(Color color, out double h, out double s, out double v)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double min = Math.Min(r, Math.Min(g, b));
            double max = Math.Max(r, Math.Max(g, b));
            double delta = max - min;

            v = max;

            if (max == 0)
            {
                s = 0;
                h = -1;
                return;
            }

            s = delta / max;

            if (r == max)
                h = (g - b) / delta;
            else if (g == max)
                h = 2 + (b - r) / delta;
            else
                h = 4 + (r - g) / delta;

            h *= 60;
            if (h < 0) h += 360;
        }

        public static Color ColorFromHsv(double h, double s, double v, byte a = 255)
        {
            if (s == 0)
            {
                byte val = (byte)(v * 255);
                return Color.FromArgb(a, val, val, val);
            }

            if (h == 360) h = 0;
            h /= 60;
            int i = (int)Math.Floor(h);
            double f = h - i;
            double p = v * (1 - s);
            double q = v * (1 - s * f);
            double t = v * (1 - s * (1 - f));

            double r, g, b;
            switch (i)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                default: r = v; g = p; b = q; break;
            }

            return Color.FromArgb(a, (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        public static Color EnsureVisibleAccentColor(Color baseColor)
        {
            ColorToHsv(baseColor, out double h, out double s, out double v);

            // 彩度が低い場合は引き上げる (最低 0.75)
            if (s < 0.75) s = 0.75;
            
            // 明度が低い場合は引き上げる (最低 0.8)
            if (v < 0.8) v = 0.8;

            return ColorFromHsv(h, s, v);
        }
    }
}
