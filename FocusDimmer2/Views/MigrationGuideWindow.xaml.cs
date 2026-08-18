using System.Diagnostics;
using System.Windows;
using FocusDimmer.Helpers;

namespace FocusDimmer.Views
{
    public partial class MigrationGuideWindow : Components.FluentWindow
    {
        private Services.LocalizationService _strings;
        private const string UnifiedAppId = "9NXHXPNJL79X";

        public string MigrationGuideText => _strings?.MigrationGuideText ?? "";
        public string MigrationOpenStorePage => _strings?.MigrationOpenStorePage ?? "";

        public MigrationGuideWindow(Services.LocalizationService strings)
        {
            InitializeComponent();
            _strings = strings;
            DataContext = this;
        }

        private void OpenStore_Click(object sender, RoutedEventArgs e)
        {
            try 
            { 
                 Process.Start(new ProcessStartInfo($"ms-windows-store://pdp/?productid={UnifiedAppId}") { UseShellExecute = true }); 
            } 
            catch { }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
