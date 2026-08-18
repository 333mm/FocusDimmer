using System;
using System.Collections.Generic;

namespace FocusDimmer.Models
{
    public class Preset
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "New Preset";
        
        // MonitorProfileから複製する設定値
        public double Opacity { get; set; } = 65;
        public double Margin { get; set; } = 0;
        public double DelayDarken { get; set; } = 0;
        public double DurationDarken { get; set; } = 0;
        public double DurationBrighten { get; set; } = 0;
        public bool ExcludeTaskbar { get; set; } = true;
        public bool ExcludeTopmost { get; set; } = false;
        public bool UseTightFrame { get; set; } = true;
        public bool DimEntirelyWhenInactive { get; set; } = false;
        public bool DimDesktopOnly { get; set; } = false;
        public bool DimWhenIdle { get; set; } = false;
        public int IdleTimeout { get; set; } = 30;
        public double IdleDimOpacity { get; set; } = 80;
        public string OverlayColorHex { get; set; } = "#000000";
        
        // プロセス自動切替ルール
        public System.Collections.ObjectModel.ObservableCollection<ProcessPresetRule> ProcessRules { get; set; } = new();

        [System.Text.Json.Serialization.JsonIgnore]
        public string ProcessRulesDisplay 
        {
            get
            {
                if (ProcessRules == null || ProcessRules.Count == 0) return "";
                return string.Join(", ", System.Linq.Enumerable.Select(ProcessRules, r => r.ProcessName));
            }
        }

        /// <summary>
        /// MonitorProfileの現在の設定値からプリセットを作成
        /// </summary>
        public static Preset FromProfile(MonitorProfile profile, string name)
        {
            return new Preset
            {
                Name = name,
                Opacity = profile.Opacity,
                Margin = profile.Margin,
                DelayDarken = profile.DelayDarken,
                DurationDarken = profile.DurationDarken,
                DurationBrighten = profile.DurationBrighten,
                ExcludeTaskbar = profile.ExcludeTaskbar,
                ExcludeTopmost = profile.ExcludeTopmost,
                UseTightFrame = profile.UseTightFrame,
                DimEntirelyWhenInactive = profile.DimEntirelyWhenInactive,
                DimDesktopOnly = profile.DimDesktopOnly,
                DimWhenIdle = profile.DimWhenIdle,
                IdleTimeout = profile.IdleTimeout,
                IdleDimOpacity = profile.IdleDimOpacity,
                OverlayColorHex = profile.OverlayColorHex
            };
        }

        /// <summary>
        /// プリセットの設定値をMonitorProfileに適用
        /// </summary>
        public void ApplyToProfile(MonitorProfile profile)
        {
            profile.Opacity = this.Opacity;
            profile.Margin = this.Margin;
            profile.DelayDarken = this.DelayDarken;
            profile.DurationDarken = this.DurationDarken;
            profile.DurationBrighten = this.DurationBrighten;
            profile.ExcludeTaskbar = this.ExcludeTaskbar;
            profile.ExcludeTopmost = this.ExcludeTopmost;
            profile.UseTightFrame = this.UseTightFrame;
            profile.DimEntirelyWhenInactive = this.DimEntirelyWhenInactive;
            profile.DimDesktopOnly = this.DimDesktopOnly;
            profile.DimWhenIdle = this.DimWhenIdle;
            profile.IdleTimeout = this.IdleTimeout;
            profile.IdleDimOpacity = this.IdleDimOpacity;
            profile.OverlayColorHex = this.OverlayColorHex;
        }
    }
}
