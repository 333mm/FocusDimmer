using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Services.Store;
using Windows.ApplicationModel;
using System.Runtime.InteropServices;
using Windows.Management.Deployment;

namespace FocusDimmer.Services
{
    public class StoreService
    {
        private StoreContext? _context;
        private StoreAppLicense? _appLicense;
        private const string ProUpgradeAddOnId = "9MWHG48NMCV0";
        private const string LegacyProPFN = "sanmiri.FocusDimmer_p3b9zhm3nac6p";

        private bool _isProSubscribed = false;
        private bool _isLegacyProDetected = false;

        public bool IsPro => _isProSubscribed || _isLegacyProDetected;

        public StoreService()
        {
            if (IsPackaged())
            {
                _context = StoreContext.GetDefault();
            }
        }

        private bool IsPackaged()
        {
            try
            {
                return Package.Current != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> InitializeAsync(FocusDimmer.Models.AppSettings settings, IntPtr windowHandle = default)
        {
            System.Diagnostics.Debug.WriteLine($"[StoreService] InitializeAsync called.");
            bool isPkg = IsPackaged();
            System.Diagnostics.Debug.WriteLine($"[StoreService] IsPackaged: {isPkg}");
            
            if (!isPkg) return false;
            bool saved = false;

            try
            {
                // 1. Check local Pro purchase cache / token first (allows instant Pro unlock & offline capability)
                if (settings.IsProPurchased || CheckProToken())
                {
                    _isProSubscribed = true;
                    if (!settings.IsProPurchased)
                    {
                        settings.IsProPurchased = true;
                        saved = true;
                    }
                    SaveProToken();
                    System.Diagnostics.Debug.WriteLine($"[StoreService] Pro purchase cached token found.");
                }

                System.Diagnostics.Debug.WriteLine($"[StoreService] Checking IsLegacyMigrated: {settings.IsLegacyMigrated}");
                
                // 2. Check if legacy Pro already migrated
                if (settings.IsLegacyMigrated)
                {
                    _isLegacyProDetected = true;
                    // Ensure token exists even if migrated (repair/restore)
                    SaveLegacyToken(); 
                    System.Diagnostics.Debug.WriteLine($"[StoreService] Already migrated. Legacy Pro assumed.");
                }
                else
                {
                    // Check local token (persistence against app uninstall)
                    if (CheckLegacyToken())
                    {
                        _isLegacyProDetected = true;
                        settings.IsLegacyMigrated = true;
                        saved = true;
                        System.Diagnostics.Debug.WriteLine($"[StoreService] Legacy Token found.");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[StoreService] Not migrated. Checking legacy installation...");
                        // Check for legacy installation
                        if (CheckLegacyProInstalled())
                        {
                            _isLegacyProDetected = true;
                            // Persist the migration so user can uninstall legacy app
                            settings.IsLegacyMigrated = true;
                            saved = true;
                            SaveLegacyToken(); // Create persistence token
                        }
                    }
                }

                // 3. Online Store Context verification
                if (_context != null)
                {
                    if (windowHandle != IntPtr.Zero)
                    {
                        try
                        {
                            WinRT.Interop.InitializeWithWindow.Initialize(_context, windowHandle);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[StoreService] InitializeWithWindow failed: {ex.Message}");
                        }
                    }

                    // アドオン購入状態のチェック
                    _appLicense = await _context.GetAppLicenseAsync();

                    // Add-on licenses are in the AddOnLicenses collection
                    if (_appLicense != null)
                    {
                        if (_appLicense.AddOnLicenses.TryGetValue(ProUpgradeAddOnId, out var license) && license.IsActive)
                        {
                            _isProSubscribed = true;
                            if (!settings.IsProPurchased)
                            {
                                settings.IsProPurchased = true;
                                saved = true;
                            }
                            SaveProToken();
                        }
                        else if (_appLicense.IsActive && !_appLicense.IsTrial && !string.IsNullOrEmpty(_appLicense.ExtendedJsonData))
                        {
                            // If base app license has full entitlement
                            _isProSubscribed = true;
                            settings.IsProPurchased = true;
                            SaveProToken();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StoreService] InitializeAsync error: {ex.Message}");
                // If local token or settings indicated Pro, keep it active even if online check threw an exception
                if (settings.IsProPurchased || CheckProToken())
                {
                    _isProSubscribed = true;
                }
            }
            return saved;
        }

        private bool CheckLegacyProInstalled()
        {
            try
            {
                var manager = new PackageManager();
                
                var currentPackage = Package.Current;
                var currentPfn = currentPackage.Id.FamilyName;

                var packages = manager.FindPackagesForUser("");

                foreach (var pkg in packages)
                {
                    try
                    {
                        // Check if it matches existing publisher ID
                        if (pkg.Id.PublisherId.ToLower() == "p3b9zhm3nac6p")
                        {
                            if (pkg.Id.FamilyName == currentPfn) continue;

                            if (pkg.Id.Name.Contains("FocusDimmer"))
                            {
                                return true;
                            }
                        }
                    }
                    catch
                    {
                        // Ignore individual package errors
                    }
                }
                return false;
            }
            catch(Exception)
            {
                return false;
            }
        }



        public async Task<(StorePurchaseStatus Status, Exception? Error)> RequestPurchaseAsync(IntPtr windowHandle)
        {
            if (!IsPackaged() || _context == null) return (StorePurchaseStatus.ServerError, new Exception("Context or Package missing"));

            try
            {
                // Initialize the StoreContext with the window handle (Required for desktop apps)
                WinRT.Interop.InitializeWithWindow.Initialize(_context, windowHandle);

                var result = await _context.RequestPurchaseAsync(ProUpgradeAddOnId);

                if (result.Status == StorePurchaseStatus.Succeeded || result.Status == StorePurchaseStatus.AlreadyPurchased)
                {
                    _isProSubscribed = true;
                    SaveProToken();
                }
                
                if (result.ExtendedError != null)
                {
                    return (result.Status, result.ExtendedError);
                }

                return (result.Status, null);
            }
            catch (Exception ex)
            {
                // 購入失敗
                return (StorePurchaseStatus.ServerError, ex);
            }
        }

        public void SaveProToken()
        {
            try
            {
                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string folder = System.IO.Path.Combine(docs, "FocusDimmer");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string path = System.IO.Path.Combine(folder, ".pro_token");
                if (!File.Exists(path))
                {
                    File.WriteAllText(path, "PRO_UNLOCKED_BY_STORE_PURCHASE");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StoreService] Failed to save pro token: {ex.Message}");
            }
        }

        public bool CheckProToken()
        {
            try
            {
                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string path = System.IO.Path.Combine(docs, "FocusDimmer", ".pro_token");
                return File.Exists(path);
            }
            catch
            {
                return false;
            }
        }

        private void SaveLegacyToken()
        {
            try
            {
                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string folder = System.IO.Path.Combine(docs, "FocusDimmer");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string path = System.IO.Path.Combine(folder, ".legacy_token");
                if (!File.Exists(path))
                {
                    File.WriteAllText(path, "PRO_UNLOCKED_BY_LEGACY_MIGRATION_V2");
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StoreService] Failed to save legacy token: {ex.Message}");
            }
        }

        private bool CheckLegacyToken()
        {
            try
            {
                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string path = System.IO.Path.Combine(docs, "FocusDimmer", ".legacy_token");
                return File.Exists(path);
            }
            catch
            {
                return false;
            }
        }
    }
}
