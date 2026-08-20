using System;
using System.Collections.Generic;
using System.Linq;
using FocusDimmer.Models;

namespace FocusDimmer.Services
{
    public class PresetService
    {
        private bool _isApplyingPreset = false;

        public bool IsApplyingPreset => _isApplyingPreset;

        public void ApplyPresetToProfiles(Preset preset, IEnumerable<MonitorProfile> profiles)
        {
            _isApplyingPreset = true;
            try
            {
                foreach (var profile in profiles)
                {
                    preset.ApplyToProfile(profile);
                }
            }
            finally
            {
                _isApplyingPreset = false;
            }
        }

        public void SyncProfileToPreset(MonitorProfile profile, string propertyName, Preset? activePreset, IEnumerable<MonitorProfile> allProfiles, bool areMonitorsLinked)
        {
            if (_isApplyingPreset) return;

            // リンクモード有効時は他モニターに伝播
            if (areMonitorsLinked)
            {
                PropagateToLinkedProfiles(profile, propertyName, allProfiles);
            }

            if (activePreset == null) return;

            switch (propertyName)
            {
                case nameof(MonitorProfile.Opacity): activePreset.Opacity = profile.Opacity; break;
                case nameof(MonitorProfile.Margin): activePreset.Margin = profile.Margin; break;
                case nameof(MonitorProfile.DelayDarken): activePreset.DelayDarken = profile.DelayDarken; break;
                case nameof(MonitorProfile.DurationDarken): activePreset.DurationDarken = profile.DurationDarken; break;
                case nameof(MonitorProfile.DurationBrighten): activePreset.DurationBrighten = profile.DurationBrighten; break;
                case nameof(MonitorProfile.ExcludeTaskbar): activePreset.ExcludeTaskbar = profile.ExcludeTaskbar; break;
                case nameof(MonitorProfile.ExcludeTopmost): activePreset.ExcludeTopmost = profile.ExcludeTopmost; break;
                case nameof(MonitorProfile.UseTightFrame): activePreset.UseTightFrame = profile.UseTightFrame; break;
                case nameof(MonitorProfile.DimEntirelyWhenInactive): activePreset.DimEntirelyWhenInactive = profile.DimEntirelyWhenInactive; break;
                case nameof(MonitorProfile.DimDesktopOnly): activePreset.DimDesktopOnly = profile.DimDesktopOnly; break;
                case nameof(MonitorProfile.DimWhenIdle): activePreset.DimWhenIdle = profile.DimWhenIdle; break;
                case nameof(MonitorProfile.IdleTimeout): activePreset.IdleTimeout = profile.IdleTimeout; break;
                case nameof(MonitorProfile.IdleDimOpacity): activePreset.IdleDimOpacity = profile.IdleDimOpacity; break;
                case nameof(MonitorProfile.OverlayColorHex): activePreset.OverlayColorHex = profile.OverlayColorHex; break;
            }
        }

        public void PropagateToLinkedProfiles(MonitorProfile source, string propertyName, IEnumerable<MonitorProfile> allProfiles)
        {
            _isApplyingPreset = true;
            try
            {
                foreach (var p in allProfiles)
                {
                    if (p == source) continue;
                    switch (propertyName)
                    {
                        case nameof(MonitorProfile.Opacity): p.Opacity = source.Opacity; break;
                        case nameof(MonitorProfile.Margin): p.Margin = source.Margin; break;
                        case nameof(MonitorProfile.DelayDarken): p.DelayDarken = source.DelayDarken; break;
                        case nameof(MonitorProfile.DurationDarken): p.DurationDarken = source.DurationDarken; break;
                        case nameof(MonitorProfile.DurationBrighten): p.DurationBrighten = source.DurationBrighten; break;
                        case nameof(MonitorProfile.ExcludeTaskbar): p.ExcludeTaskbar = source.ExcludeTaskbar; break;
                        case nameof(MonitorProfile.ExcludeTopmost): p.ExcludeTopmost = source.ExcludeTopmost; break;
                        case nameof(MonitorProfile.UseTightFrame): p.UseTightFrame = source.UseTightFrame; break;
                        case nameof(MonitorProfile.DimEntirelyWhenInactive): p.DimEntirelyWhenInactive = source.DimEntirelyWhenInactive; break;
                        case nameof(MonitorProfile.DimDesktopOnly): p.DimDesktopOnly = source.DimDesktopOnly; break;
                        case nameof(MonitorProfile.DimWhenIdle): p.DimWhenIdle = source.DimWhenIdle; break;
                        case nameof(MonitorProfile.IdleTimeout): p.IdleTimeout = source.IdleTimeout; break;
                        case nameof(MonitorProfile.IdleDimOpacity): p.IdleDimOpacity = source.IdleDimOpacity; break;
                        case nameof(MonitorProfile.OverlayColorHex): p.OverlayColorHex = source.OverlayColorHex; break;
                    }
                }
            }
            finally
            {
                _isApplyingPreset = false;
            }
        }

        public void PropagateAllToLinkedProfiles(MonitorProfile source, IEnumerable<MonitorProfile> allProfiles)

        {
            _isApplyingPreset = true;
            try
            {
                foreach (var p in allProfiles)
                {
                    if (p == source) continue;
                    p.Opacity = source.Opacity;
                    p.Margin = source.Margin;
                    p.DelayDarken = source.DelayDarken;
                    p.DurationDarken = source.DurationDarken;
                    p.DurationBrighten = source.DurationBrighten;
                    p.ExcludeTaskbar = source.ExcludeTaskbar;
                    p.ExcludeTopmost = source.ExcludeTopmost;
                    p.UseTightFrame = source.UseTightFrame;
                    p.DimEntirelyWhenInactive = source.DimEntirelyWhenInactive;
                    p.DimDesktopOnly = source.DimDesktopOnly;
                    p.DimWhenIdle = source.DimWhenIdle;
                    p.IdleTimeout = source.IdleTimeout;
                    p.IdleDimOpacity = source.IdleDimOpacity;
                    p.OverlayColorHex = source.OverlayColorHex;
                }
            }
            finally
            {
                _isApplyingPreset = false;
            }
        }


        public string? FindMatchingPresetId(string processName, IEnumerable<Preset> presets, string defaultPresetId, string currentSelectedId)
        {
            if (string.IsNullOrEmpty(processName)) return null;

            foreach (var preset in presets)
            {
                if (preset.ProcessRules.Any(r => r.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase)))
                {
                    return preset.Id;
                }
            }

            // マッチしなかった場合、デフォルトプリセットがあればフォールバック
            if (!string.IsNullOrEmpty(defaultPresetId) && presets.Any(p => p.Id == defaultPresetId))
            {
                return defaultPresetId;
            }

            return null;
        }
    }
}
