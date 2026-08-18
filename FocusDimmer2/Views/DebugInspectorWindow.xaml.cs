using System.Windows;

namespace FocusDimmer.Views
{
    public class WindowData
    {
        public int Index { get; set; }
        public IntPtr Hwnd { get; set; }
        public string ProcessName { get; set; } = "";
        public string Title { get; set; } = "";
        public string ClassName { get; set; } = "";
        public string RectString { get; set; } = "";
        public string Flags { get; set; } = "";

        public string DisplayHeader => $"[{Index}] {ProcessName}";
        public string DisplayDetails => $"Title: {Title}\nClass: {ClassName}\nRect: {RectString}\nFlags: {Flags}";
    }

    public partial class DebugInspectorWindow : Components.FluentWindow
    {
        public event EventHandler<WindowData>? WindowSelected;

        public DebugInspectorWindow()
        {
            InitializeComponent();
        }

        public void UpdateList(System.Collections.Generic.IEnumerable<WindowData> windows)
        {
            WindowList.ItemsSource = windows;
        }

        public void UpdateStatus(string text)
        {
            StatusText.Text = text;
        }

        private void WindowList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (WindowList.SelectedItem is WindowData data)
            {
                WindowSelected?.Invoke(this, data);
            }
        }
    }
}
