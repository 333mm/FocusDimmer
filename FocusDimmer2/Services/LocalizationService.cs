using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace FocusDimmer.Services
{
    public class LocalizationService : INotifyPropertyChanged
    {
        public string AppTitle { get; set; } = "Focus Dimmer";
        public string HeaderGlobalSettings { get; set; } = "Global Settings";
        public string SubHeaderGlobalSettings { get; set; } = "These settings apply to all connected monitors.";
        public string BadgeAllMonitors { get; set; } = "ALL MONITORS";
        public string HeaderOverlay { get; set; } = "OVERLAY SETTINGS";
        public string CheckDimEntirely { get; set; } = "Dim entire monitor when inactive";
        public string TooltipDimEntirely { get; set; } = "Prioritized over other brightness options.";
        public string CheckDimDesktopOnly { get; set; } = "Focus Mode: Display all windows brightly";
        public string TooltipDimDesktopOnly { get; set; } = "Only the desktop area will be dimmed. Margin function disabled.";
        public string CheckDimWhenIdle { get; set; } = "Dim entire monitor when idle";
        public string CheckDimOnIdle { get; set; } = "Dim automatically when idle";
        public string TooltipDimWhenIdle { get; set; } = "Prioritized over other brightness options.";
        public string LabelColor { get; set; } = "Overlay Color";
        public string LabelOpacity { get; set; } = "Opacity";
        public string LabelOpacityLong { get; set; } = "Overlay Opacity";
        public string LabelIdleOpacity { get; set; } = "Idle Opacity";
        public string LabelMargin { get; set; } = "Margin";
        public string LabelIdleTime { get; set; } = "Idle Time";
        public string HeaderAnimation { get; set; } = "ANIMATION SETTINGS";
        public string LabelDarken { get; set; } = "Fade In";
        public string LabelBrighten { get; set; } = "Fade Out";
        public string LabelWait { get; set; } = "Wait Time";
        public string TooltipWait { get; set; } = "Time from when the window becomes active until the animation starts.";
        public string LabelDuration { get; set; } = "Animation Time";
        public string HeaderExclusionLists { get; set; } = "EXCLUSION LISTS (Process)";
        public string CheckTaskbar { get; set; } = "Always keep Taskbar bright";
        public string CheckTopmost { get; set; } = "Always keep Picture-in-Picture bright";
        public string CheckTightFrame { get; set; } = "Fit dimming area tightly to window frame";
        public string LabelAppList { get; set; } = "Don't dim when active (Process):";
        public string LabelAlwaysBright { get; set; } = "Always bright (Process):";
        public string LabelAlwaysDark { get; set; } = "Always dark (Process):";
        public string BtnBrowse { get; set; } = "Browse";
        public string TipAppList { get; set; } = "* .exe selection auto-appends process name.";
        public string LabelToggle { get; set; } = "ON/OFF";
        public string LabelDarker { get; set; } = "Darken";
        public string LabelLighter { get; set; } = "Brighten";
        public string BtnClose { get; set; } = "Close to Tray";
        public string HeaderStartup { get; set; } = "Startup:";
        public string CheckAutoStart { get; set; } = "Auto Start";
        public string CheckAdmin { get; set; } = "Run as Admin";
        public string WindowSelectApp { get; set; } = "Select App";
        public string SearchPlaceholder { get; set; } = "Search processes...";
        public string BtnRefresh { get; set; } = "Refresh";
        public string BtnSelect { get; set; } = "Select";
        public string BtnCancel { get; set; } = "Cancel";
        public string MsgProRequired { get; set; } = "This feature requires the Pro version.\nWould you like to view it in the Store?";
        public string BannerText { get; set; } = "Get Pro Version to unlock Multi-Monitor & Color settings!";
        public string MigrationBannerText { get; set; } = "The new 'Unified' version is here! Pro users can migrate for free.";
        public string MigrationBannerLink { get; set; } = "Migrate for Free 🚀";
        public string MigrationGuideText { get; set; } = "";
        public string MigrationOpenStorePage { get; set; } = "Open Unified Version Store Page";
        
        public string BtnAddIgnore { get; set; } = "Add to Ignore List";
        public string BtnAddBright { get; set; } = "Add to Always Bright List";
        public string BtnAddDark { get; set; } = "Add to Always Dark List";
        public string DebugStatusClick { get; set; } = "Click to freeze";
        public string DebugStatusSelect { get; set; } = "Select from the list";
        
        // Debug overlay strings
        public string BtnDebug { get; set; } = "Debug";
        public string TooltipDebugMode { get; set; } = "Displays window info under mouse cursor to find cause of bright areas.";
        public string TooltipAlwaysDark { get; set; } = "Add processes causing bright spots (e.g., GeForce Experience).";
        public string DebugNoWindow { get; set; } = "No window detected";
        public string DebugProcess { get; set; } = "Process";
        public string DebugClass { get; set; } = "Class";
        public string DebugTitle { get; set; } = "Title";
        public string DebugHoleAnalysis { get; set; } = "Hole Analysis";
        public string DebugTaskbar { get; set; } = "Taskbar → Always Bright";
        public string DebugTopmost { get; set; } = "Topmost → May be PiP Bright";
        public string DebugMenu { get; set; } = "Menu/Popup → Always Bright";
        public string DebugDialog { get; set; } = "Dialog → Special handling";
        public string DebugToolWindow { get; set; } = "Tool Window";
        public string DebugStandardWindow { get; set; } = "Standard window - check if in Always Bright/Dark list";
        public string DebugAddToDarkList { get; set; } = "Add '{0}' to Always Dark list to dim this window.";
        
        // Preset strings
        public string HeaderPreset { get; set; } = "PRESETS";
        public string TooltipEditPreset { get; set; } = "Edit preset name";
        public string TooltipAddPreset { get; set; } = "Save current settings as new preset";
        public string TooltipDeletePreset { get; set; } = "Delete selected preset";
        public string LabelProcessSwitch { get; set; } = "Auto-switch when process is active:";
        public string LabelAssociatedProcesses { get; set; } = "Associated Processes:";
        public string BtnAddProcessRule { get; set; } = "Add Process Rule";
        public string DefaultPresetName { get; set; } = "Default";
        public string NewPresetName { get; set; } = "New Preset";
        public string MsgConfirmDeletePreset { get; set; } = "Delete this preset?";
        public string MsgEnterPresetName { get; set; } = "Enter preset name:";
        public string TooltipDebugInspector { get; set; } = "Debug window inspector";
        public string LabelDefault { get; set; } = "(Default)";
        public string TooltipSetAsDefault { get; set; } = "Set as Default Preset";
        public string MenuRename { get; set; } = "Rename";
        
        // Navigation & New Sections
        public string NavMonitors { get; set; } = "Monitors";
        public string NavExclusions { get; set; } = "App Exclusions";
        public string NavHotkeys { get; set; } = "Shortcuts";
        public string NavPresets { get; set; } = "Presets";
        public string NavGeneral { get; set; } = "General";
        public string HeaderAdvancedBehavior { get; set; } = "Advanced Behavior";
        public string SubHeaderHotkeys { get; set; } = "Configure keyboard shortcuts to quickly control dimming.";
        public string HeaderLanguage { get; set; } = "Language";
        public string SubHeaderLanguage { get; set; } = "Choose display language";
        public string SubHeaderGeneral { get; set; } = "App startup, behavior and language settings.";
        public string BtnMigrationInfo { get; set; } = "Migration Info";
        public string MsgStartupDisabledByUser { get; set; } = "Startup is disabled in Windows Settings or Task Manager.\nPlease enable Focus Dimmer in Windows 'Startup Apps' settings.";
        public string CheckCloseToTray { get; set; } = "Minimize to system tray on close (×)";
        public string LabelLinkMonitors { get; set; } = "Link All Displays";
        public string TooltipLinkMonitors { get; set; } = "When enabled, changes to one display will automatically apply to all displays.";
        public string HeaderMultiMonitorPro { get; set; } = "Multi-Display Dimming";
        public string DescMultiMonitorPro { get; set; } = "Upgrade to Pro to dim and customize multiple displays simultaneously.";
        public string BtnUpgradePro { get; set; } = "Upgrade to Pro";
        
        // Indexer for accessing properties by string name

        public string? this[string propertyName]
        {
            get
            {
                var prop = this.GetType().GetProperty(propertyName);
                return prop?.GetValue(this)?.ToString();
            }
        }

        public void UpdateLanguage(string langCode)
        {
            SetDefaultEnglish();
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            string langPath = System.IO.Path.Combine(exePath, "Languages", $"{langCode}.json");
            if (File.Exists(langPath))
            {
                try
                {
                    var options = new JsonSerializerOptions { AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip };
                    string json = File.ReadAllText(langPath);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json, options);
                    if (dict != null)
                    {
                        var properties = this.GetType().GetProperties();
                        foreach (var kvp in dict)
                        {
                            var prop = properties.FirstOrDefault(p => p.Name == kvp.Key);
                            if (prop != null && prop.CanWrite) prop.SetValue(this, kvp.Value);
                        }
                    }
                }
                catch { }
            }
            else if (langCode == "ja") SetDefaultJapanese();
            OnProp(null);
        }
        private void SetDefaultEnglish()
        {
            AppTitle = "Focus Dimmer";
            HeaderGlobalSettings = "Global Settings";
            SubHeaderGlobalSettings = "These settings apply to all connected monitors.";
            BadgeAllMonitors = "ALL MONITORS";
            HeaderOverlay = "OVERLAY SETTINGS";
            CheckDimEntirely = "Dim entire monitor when inactive";
            TooltipDimEntirely = "Prioritized over other brightness options.";
            CheckDimDesktopOnly = "Display all windows brightly";
            TooltipDimDesktopOnly = "Only the desktop area will be dimmed. Margin function disabled.";
            CheckDimWhenIdle = "Dim entire monitor when idle";
            CheckDimOnIdle = "Dim automatically when idle";
            TooltipDimWhenIdle = "Prioritized over other brightness options.";
            LabelColor = "Overlay Color";
            LabelOpacity = "Dim Level"; 
            LabelOpacityLong = "Overlay Opacity";
            LabelIdleOpacity = "Idle Opacity";
            LabelMargin = "Margin";
            LabelIdleTime = "Idle Time";
            HeaderAnimation = "ANIMATION SETTINGS";
            LabelDarken = "Fade In";
            LabelBrighten = "Fade Out";
            LabelWait = "Wait Time";
            TooltipWait = "Time from when the window becomes active until the animation starts.";
            LabelDuration = "Animation Time";
            HeaderExclusionLists = "EXCLUSION LISTS (Process)";
            CheckTaskbar = "Always keep Taskbar bright";
            CheckTopmost = "Always keep Picture-in-Picture bright";
            CheckTightFrame = "Fit dimming area tightly to window frame";
            LabelAppList = "Don't dim when active (Process):";
            LabelAlwaysBright = "Always bright (Process):";
            LabelAlwaysDark = "Always dark (Process):";
            TipAppList = "* .exe selection auto-appends process name.";
            LabelToggle = "ON/OFF";
            LabelDarker = "Darken";
            LabelLighter = "Brighten";
            BtnClose = "Close to Tray";
            HeaderStartup = "Startup:";
            CheckAutoStart = "Auto Start";
            CheckAdmin = "Run as Admin";
            WindowSelectApp = "Select App";
            SearchPlaceholder = "Search processes...";
            BtnRefresh = "Refresh";
            BtnBrowse = "Browse...";
            BtnSelect = "Select";
            BtnCancel = "Cancel";
            MsgProRequired = "This feature requires the Pro version.\nWould you like to view it in the Store?";
            BannerText = "Get Pro Version to unlock Multi-Monitor & Color settings!";
            MigrationBannerText = "We've unified the app to continue providing updates. \nExisting Pro users will automatically keep their Pro status in the new version.";
            MigrationBannerLink = "Migrate to Latest Version 🚀";
            MigrationGuideText = "Thank you for purchasing this app. To ensure smoother updates in the future, we have decided to unify the app structure into a \"Unified Version\".\n\nFuture Pro features will be provided as an \"Add-on (In-App Purchase)\" in the Unified Version. However, if you have already purchased this Pro version, you can continue to use Pro features for free by following these steps:\n\n1. Install the new \"Unified Version\" while keeping this app (Pro version) installed.\n\n2. Confirm that the Pro features are automatically unlocked in the Unified Version.\n\n3. After confirming the unlock, you can safely uninstall this app.\n\nYou can continue to use this app as is, but the store page will be hidden from non-purchasers, and updates will stop. To access the latest features, we strongly recommend migrating to the Unified Version.\n\nWe apologize for any inconvenience and appreciate your continued support.";
            MigrationOpenStorePage = "Open Unified Version Store Page";
            BtnAddIgnore = "Add to Ignore List";
            BtnAddBright = "Add to Always Bright List";
            BtnAddDark = "Add to Always Dark List";
            DebugStatusClick = "Click to freeze";
            DebugStatusSelect = "Select from the list";
            
            // Preset strings
            HeaderPreset = "PRESETS";
            TooltipEditPreset = "Edit preset name";
            TooltipAddPreset = "Save current settings as new preset";
            TooltipDeletePreset = "Delete selected preset";
            LabelProcessSwitch = "Auto-switch when process is active:";
            LabelAssociatedProcesses = "Associated Processes:";
            BtnAddProcessRule = "Add Process Rule";
            DefaultPresetName = "Default";
            NewPresetName = "New Preset";
            MsgConfirmDeletePreset = "Delete this preset?";
            MsgEnterPresetName = "Enter preset name:";
            TooltipDebugInspector = "Debug window inspector";
            LabelDefault = "(Default)";
            TooltipSetAsDefault = "Set as Default Preset";
            MenuRename = "Rename";
            NavMonitors = "Monitors";
            NavExclusions = "App Exclusions";
            NavHotkeys = "Shortcuts";
            NavPresets = "Presets";
            NavGeneral = "General";
            HeaderAdvancedBehavior = "Advanced Behavior";
            SubHeaderHotkeys = "Configure keyboard shortcuts to quickly control dimming.";
            HeaderLanguage = "Language";
            SubHeaderLanguage = "Choose display language";
            SubHeaderGeneral = "App startup, behavior and language settings.";
            BtnMigrationInfo = "Migration Info";
            MsgStartupDisabledByUser = "Startup is disabled in Windows Settings or Task Manager.\nPlease enable Focus Dimmer in Windows 'Startup Apps' settings.";
            CheckCloseToTray = "Minimize to system tray on close (×)";
            LabelLinkMonitors = "Link All Displays";
            TooltipLinkMonitors = "When enabled, changes to one display will automatically apply to all displays.";
            HeaderMultiMonitorPro = "Multi-Display Dimming";
            DescMultiMonitorPro = "Upgrade to Pro to dim and customize multiple displays simultaneously.";
            BtnUpgradePro = "Upgrade to Pro";
        }

        private void SetDefaultJapanese()
        {
            AppTitle = "Focus Dimmer";
            HeaderGlobalSettings = "共通設定";
            SubHeaderGlobalSettings = "これらの設定はすべてのモニターに適用されます。";
            BadgeAllMonitors = "モニター共通";
            HeaderOverlay = "オーバーレイ設定";
            CheckDimEntirely = "このモニタが非アクティブ時は全体を暗くする";
            TooltipDimEntirely = "他の明るくなるオプションより優先されます。";
            CheckDimDesktopOnly = "全てのウィンドウを明るく表示する";
            TooltipDimDesktopOnly = "デスクトップエリアのみが暗くなります。余白機能が無効になります。";
            CheckDimWhenIdle = "操作がない状態の時に全体を暗くする";
            CheckDimOnIdle = "操作がない時に自動で暗くする";
            TooltipDimWhenIdle = "他の明るくなるオプションより優先されます。";
            LabelColor = "オーバーレイの色";
            LabelOpacity = "減光レベル";
            LabelOpacityLong = "オーバーレイの不透明度";
            LabelIdleOpacity = "アイドル時の不透明度";
            LabelMargin = "余白";
            LabelIdleTime = "無操作時間";
            HeaderAnimation = "アニメーション設定";
            LabelDarken = "フェードイン";
            LabelBrighten = "フェードアウト";
            LabelWait = "待機時間";
            TooltipWait = "ウィンドウがアクティブになってからアニメーションが開始されるまでの時間です。";
            LabelDuration = "アニメーション時間";
            HeaderExclusionLists = "除外設定 (プロセス)";
            CheckTaskbar = "タスクバーを常に明るくする";
            CheckTopmost = "ピクチャー・イン・ピクチャーを常に明るくする";
            CheckTightFrame = "ウィンドウの枠に減光範囲をぴったり合わせる";
            LabelAppList = "アクティブ時に減光させないプロセス:";
            LabelAlwaysBright = "常に明るくするプロセス:";
            LabelAlwaysDark = "常に暗くするプロセス:";
            TipAppList = "※ .exe を選択するとプロセス名が追加されます";
            LabelToggle = "ON/OFF";
            LabelDarker = "暗くする";
            LabelLighter = "明るくする";
            BtnClose = "タスクトレイに閉じる";
            HeaderStartup = "起動設定:";
            CheckAutoStart = "自動起動";
            CheckAdmin = "管理者権限";
            WindowSelectApp = "アプリ選択";
            SearchPlaceholder = "プロセスを検索...";
            BtnRefresh = "更新";
            BtnBrowse = "ファイル参照...";
            BtnSelect = "選択";
            BtnCancel = "キャンセル";
            MsgProRequired = "この機能はPro版のみ使用可能です。\nストアでPro版を確認しますか？";
            BannerText = "Pro版を取得してマルチモニターや色設定を解除！";
            MigrationBannerText = "今後のアップデートを継続するため、アプリ構成を統合しました。\n現在のPro版ユーザー様は、本統合版をインストール後、古いFocusDimmerはアンインストールしてください。";
            MigrationBannerLink = "統合版(最新)へ無料移行 🚀";
            MigrationGuideText = "この度は本アプリをご購入いただき、誠にありがとうございます。 今後、よりスムーズにアップデートを継続していくため、勝手ながらアプリの構成を「統合版」へと一本化させていただくこととなりました。\n\n今後のPro版機能は、統合版における「アドオン（アプリ内課金）」として提供されますが、すでに本Pro版をご購入いただいている皆様は、以下の手順で引き続きPro機能を無料でご利用いただけます。\n\n1. 本アプリ（Pro版）をインストールした状態のまま、新しい「統合版」をインストールしてください。\n\n2. 統合版にてPro機能が自動的にアンロックされていることを確認してください。\n\n3. アンロックの確認後は、本アプリをアンインストールしていただいて問題ありません。\n\nなお、本アプリをそのまま使い続けることも可能ですが、今後のストアページはご購入者様以外には非公開となり、ページの更新も停止いたします。最新の機能をご利用いただくためにも、ぜひ統合版への移行をお願い申し上げます。\n\nご不便をおかけいたしますが、今後ともよろしくお願いいたします。";
            MigrationOpenStorePage = "統合版のストアページを開く";
            BtnAddIgnore = "除外リストに追加する";
            BtnAddBright = "常に明るくするリストに追加する";
            BtnAddDark = "常に暗くするリストに追加する";
            DebugStatusClick = "クリックしてフリーズ";
            DebugStatusSelect = "リストから選択してください";
            
            // Preset strings (Japanese)
            HeaderPreset = "プリセット";
            TooltipEditPreset = "プリセット名を編集";
            TooltipAddPreset = "現在の設定を新規プリセットとして保存";
            TooltipDeletePreset = "選択したプリセットを削除";
            LabelProcessSwitch = "プロセス実行時に自動切替:";
            LabelAssociatedProcesses = "関連付けられたプロセス:";
            BtnAddProcessRule = "プロセスルールを追加";
            DefaultPresetName = "デフォルト";
            NewPresetName = "新規プリセット";
            MsgConfirmDeletePreset = "このプリセットを削除しますか？";
            MsgEnterPresetName = "プリセット名を入力:";
            TooltipDebugInspector = "デバッグウィンドウ検査";
            LabelDefault = "(既定)";
            TooltipSetAsDefault = "既定のプリセットに設定";
            MenuRename = "名前を変更";
            NavMonitors = "モニター";
            NavExclusions = "アプリ除外";
            NavHotkeys = "ショートカット";
            NavPresets = "プリセット";
            NavGeneral = "一般設定";
            HeaderAdvancedBehavior = "高度な動作設定";
            SubHeaderHotkeys = "減光を素早く切り替えるキーボードショートカットを設定します。";
            HeaderLanguage = "言語 / Language";
            SubHeaderLanguage = "表示言語を選択します";
            SubHeaderGeneral = "アプリの起動、動作、言語の設定です。";
            BtnMigrationInfo = "移行情報";
            MsgStartupDisabledByUser = "自動起動が Windows の設定またはタスクマネージャーによって無効化されています。\nWindows の「スタートアップ アプリ」設定を開いて Focus Dimmer を有効にしてください。";
            CheckCloseToTray = "閉じるボタン（×）でタスクトレイに最小化する";
            LabelLinkMonitors = "全ディスプレイ連動";
            TooltipLinkMonitors = "有効にすると、すべてのディスプレイの設定が同時に連動して変更されます。";
            HeaderMultiMonitorPro = "マルチモニター減光制御";
            DescMultiMonitorPro = "接続された複数のディスプレイを個別に減光・連動制御するにはPRO版が必要です。";
            BtnUpgradePro = "PROにアップグレード";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnProp([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
