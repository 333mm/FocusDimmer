# 実装計画: プリセット自動保存、除外設定の保存不具合修正、およびレイアウト変更

ユーザーによる各種設定変更を、現在アクティブなプリセットに即時自動保存し、除外リストの保存バグを修正します。さらに、UIレイアウトを整理し、「プリセット」セクションを「ホットキー」の下、「起動設定」の上に移動します。

## ユーザーレビューが必要な項目
特になし（操作性の向上と、期待される保存挙動に修正する安全な変更です）。

## 提案される変更

### 1. UI（XAML）レイアウトの変更

#### [MODIFY] [MainWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml)
右側カラム（共通設定部）のセクション順序を以下のように変更します。
- 旧順序: 1. プリセット, 2. 除外設定, 3. ホットキー, 4. 起動設定
- 新順序: 1. 除外設定, 2. ホットキー, **3. プリセット**, 4. 起動設定

具体的には、`MainWindow.xaml` の「Section 1: Presets」の `Border`要素（約130行分）を切り取り、「Section 3: Hotkeys」の `Border`要素の直後（「Section 4: Startup & Close」の直前）に挿入します。

---

### 2. 除外設定の保存不具合の修正

#### [MODIFY] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)
`SaveSettingsActual()` メソッド内で、新しく生成される `AppSettings` オブジェクトに対して `IgnoreList`, `AlwaysBrightList`, `AlwaysDarkList` の代入が漏れていたため、これを追加します。

```csharp
                var settings = new AppSettings 
                { 
                    Profiles = new List<MonitorProfile>(MonitorProfiles), 
                    AutoStart = AutoStartCheck.IsChecked == true, 
                    RunAsAdmin = false,
                    WindowWidth = w,
                    WindowHeight = h,
                    WindowLeft = l,
                    WindowTop = t,
                    IsLegacyMigrated = _appSettings.IsLegacyMigrated,
                    IsLegacyBannerDismissed = _appSettings.IsLegacyBannerDismissed,
                    Presets = new List<Preset>(GlobalPresets),
                    SelectedPresetId = SelectedGlobalPresetId,
                    DefaultPresetId = DefaultPresetId,
                    
                    // 【修正】除外設定プロパティを保存対象に追加
                    IgnoreList = IgnoreList,
                    AlwaysBrightList = AlwaysBrightList,
                    AlwaysDarkList = AlwaysDarkList
                };
```

---

### 3. 設定変更時のプリセットへの自動保存（同期）

#### [MODIFY] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)
- モニタープロフィール（`MonitorProfile`）のプロパティ値が変更された際、現在適用中のプリセット（`SelectedGlobalPreset`）にその変更を即時反映し、他の接続中モニターの対応するプロパティにも同じ値を伝播（同期）するロジックを実装します。
- 同期の循環や無限ループを防ぐため、適用中フラグ `_isApplyingPreset` を使用した制御を行います。

**追加するプロパティ・メソッド等:**
```csharp
        private bool _isApplyingPreset = false;

        private void SyncProfileToPreset(MonitorProfile profile, string propertyName)
        {
            if (_isApplyingPreset || SelectedGlobalPreset == null) return;
            
            var preset = SelectedGlobalPreset;
            
            switch (propertyName)
            {
                case nameof(MonitorProfile.Opacity):
                    if (preset.Opacity != profile.Opacity) { preset.Opacity = profile.Opacity; PropagateToOtherMonitors(p => p.Opacity = profile.Opacity, profile); }
                    break;
                case nameof(MonitorProfile.Margin):
                    if (preset.Margin != profile.Margin) { preset.Margin = profile.Margin; PropagateToOtherMonitors(p => p.Margin = profile.Margin, profile); }
                    break;
                case nameof(MonitorProfile.DelayDarken):
                    if (preset.DelayDarken != profile.DelayDarken) { preset.DelayDarken = profile.DelayDarken; PropagateToOtherMonitors(p => p.DelayDarken = profile.DelayDarken, profile); }
                    break;
                case nameof(MonitorProfile.DurationDarken):
                    if (preset.DurationDarken != profile.DurationDarken) { preset.DurationDarken = profile.DurationDarken; PropagateToOtherMonitors(p => p.DurationDarken = profile.DurationDarken, profile); }
                    break;
                case nameof(MonitorProfile.DurationBrighten):
                    if (preset.DurationBrighten != profile.DurationBrighten) { preset.DurationBrighten = profile.DurationBrighten; PropagateToOtherMonitors(p => p.DurationBrighten = profile.DurationBrighten, profile); }
                    break;
                case nameof(MonitorProfile.ExcludeTaskbar):
                    if (preset.ExcludeTaskbar != profile.ExcludeTaskbar) { preset.ExcludeTaskbar = profile.ExcludeTaskbar; PropagateToOtherMonitors(p => p.ExcludeTaskbar = profile.ExcludeTaskbar, profile); }
                    break;
                case nameof(MonitorProfile.ExcludeTopmost):
                    if (preset.ExcludeTopmost != profile.ExcludeTopmost) { preset.ExcludeTopmost = profile.ExcludeTopmost; PropagateToOtherMonitors(p => p.ExcludeTopmost = profile.ExcludeTopmost, profile); }
                    break;
                case nameof(MonitorProfile.UseTightFrame):
                    if (preset.UseTightFrame != profile.UseTightFrame) { preset.UseTightFrame = profile.UseTightFrame; PropagateToOtherMonitors(p => p.UseTightFrame = profile.UseTightFrame, profile); }
                    break;
                case nameof(MonitorProfile.DimEntirelyWhenInactive):
                    if (preset.DimEntirelyWhenInactive != profile.DimEntirelyWhenInactive) { preset.DimEntirelyWhenInactive = profile.DimEntirelyWhenInactive; PropagateToOtherMonitors(p => p.DimEntirelyWhenInactive = profile.DimEntirelyWhenInactive, profile); }
                    break;
                case nameof(MonitorProfile.DimDesktopOnly):
                    if (preset.DimDesktopOnly != profile.DimDesktopOnly) { preset.DimDesktopOnly = profile.DimDesktopOnly; PropagateToOtherMonitors(p => p.DimDesktopOnly = profile.DimDesktopOnly, profile); }
                    break;
                case nameof(MonitorProfile.DimKeepAspect): // 存在しないプロパティの場合は省略可能ですが、念のためPresetとMonitorProfileで一致している項目のみ
                    break;
                case nameof(MonitorProfile.DimWhenIdle):
                    if (preset.DimWhenIdle != profile.DimWhenIdle) { preset.DimWhenIdle = profile.DimWhenIdle; PropagateToOtherMonitors(p => p.DimWhenIdle = profile.DimWhenIdle, profile); }
                    break;
                case nameof(MonitorProfile.IdleTimeout):
                    if (preset.IdleTimeout != profile.IdleTimeout) { preset.IdleTimeout = profile.IdleTimeout; PropagateToOtherMonitors(p => p.IdleTimeout = profile.IdleTimeout, profile); }
                    break;
                case nameof(MonitorProfile.IdleDimOpacity):
                    if (preset.IdleDimOpacity != profile.IdleDimOpacity) { preset.IdleDimOpacity = profile.IdleDimOpacity; PropagateToOtherMonitors(p => p.IdleDimOpacity = profile.IdleDimOpacity, profile); }
                    break;
                case nameof(MonitorProfile.OverlayColorHex):
                    if (preset.OverlayColorHex != profile.OverlayColorHex) { preset.OverlayColorHex = profile.OverlayColorHex; PropagateToOtherMonitors(p => p.OverlayColorHex = profile.OverlayColorHex, profile); }
                    break;
            }
        }

        private void PropagateToOtherMonitors(Action<MonitorProfile> action, MonitorProfile sourceProfile)
        {
            _isApplyingPreset = true;
            try
            {
                foreach (var p in MonitorProfiles)
                {
                    if (p != sourceProfile)
                    {
                        action(p);
                    }
                }
            }
            finally
            {
                _isApplyingPreset = false;
            }
        }
```

**修正箇所 (`InitializeMonitors` 内の PropertyChanged 登録部分):**
```csharp
                profile.PropertyChanged += (s, e) => 
                {
                    if (s is MonitorProfile mp && e.PropertyName != null)
                    {
                        SyncProfileToPreset(mp, e.PropertyName);
                    }
                    RequestSave();
                };
```

**修正箇所 (`ApplyGlobalPresetToAllMonitors` メソッド):**
```csharp
        private void ApplyGlobalPresetToAllMonitors(Preset preset)
        {
            _isApplyingPreset = true;
            try
            {
                foreach (var profile in MonitorProfiles)
                {
                    preset.ApplyToProfile(profile);
                }
            }
            finally
            {
                _isApplyingPreset = false;
            }
        }
```

---

## 検証計画

### 自動ビルドテスト
- `dotnet build` を実行し、コンパイルエラーが出ないことを確認します。

### 手動検証
1. 右側カラムのレイアウトが上から「除外設定」「ホットキー」「プリセット」「起動設定」の順に並んでいることを確認します。
2. 除外設定に入力した値が再起動後も正しく保持されることを確認します。
3. プリセットA適用中に設定を変更し、別のプリセットに切り替えてから再度プリセットAに戻した際、変更が保存されていることを確認します。
