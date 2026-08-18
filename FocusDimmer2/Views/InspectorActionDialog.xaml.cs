using System.Windows;
using FocusDimmer.Services;

namespace FocusDimmer.Views
{
    public partial class InspectorActionDialog : Components.FluentWindow
    {
        public string ActionType { get; private set; } = "";
        public string ProcessName { get; }
        public string WindowTitle { get; }
        public LocalizationService Strings { get; }

        public InspectorActionDialog(string processName, string windowTitle, LocalizationService strings)
        {
            InitializeComponent();
            ProcessName = processName;
            WindowTitle = windowTitle;
            Strings = strings;
            DataContext = this;
        }

        private void Action_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement el)
            {
                ActionType = el.Tag?.ToString() ?? "";
                DialogResult = ActionType != "Cancel";
                Close();
            }
        }
    }
}
