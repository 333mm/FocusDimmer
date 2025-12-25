using System.Windows;

namespace FocusDimmer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    // ここで "Application" ではなく "System.Windows.Application" と明記して区別します
    public partial class App : System.Windows.Application
    {
        public static bool IsAutoStart { get; private set; } = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Check command line args (Unpackaged / Fallback)
            if (e.Args != null)
            {
                foreach (var arg in e.Args)
                {
                    if (arg.ToLower().Contains("autostart"))
                    {
                        IsAutoStart = true;
                        break;
                    }
                }
            }

            // Check Activation Kind (Packaged)
            if (!IsAutoStart)
            {
                try
                {
                    // Safe check if packaged
                    bool isPackaged = false;
                    try { isPackaged = global::Windows.ApplicationModel.Package.Current != null; } catch { }

                    if (isPackaged)
                    {
                        var args = global::Windows.ApplicationModel.AppInstance.GetActivatedEventArgs();
                        if (args != null && args.Kind == global::Windows.ApplicationModel.Activation.ActivationKind.StartupTask)
                        {
                            IsAutoStart = true;
                        }
                    }
                }
                catch { }
            }

            base.OnStartup(e);

            var w = new MainWindow();
            
            // Initialization is handled in MainWindow constructor/Loaded
            // We just decide whether to Show() or not.

            if (IsAutoStart)
            {
                // Do NOT Show() the window.
                // Do NOT set WindowState=Minimized or ShowInTaskbar=false here as it causes restoration issues.
                // Just let it be hidden by default (since we haven't called Show()).
                
                w.ShowStartupNotification();
            }
            else
            {
                w.Show();
            }
        }
    }
}