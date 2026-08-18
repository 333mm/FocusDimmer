using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using FocusDimmer.Helpers;

namespace FocusDimmer.Components
{
    public class FluentWindow : Window
    {
        public FluentWindow()
        {
            // Default properties for Fluent Design
            WindowStyle = WindowStyle.None;
            AllowsTransparency = false; // Mica/Acrylic usually requires this to be false for modern API, or true for old fallback. WindowHelper handles it.
            // Actually, for WPF with WindowChrome, AllowsTransparency=true can be problematic with modern backdrops in some versions.
            // Let's stick to standard practice: 
            // - For modern Mica/Acrylic (Win11), AllowsTransparency=false + DwmSetWindowAttribute.
            // - For BlurBehind (Win10), AllowsTransparency=true + SetWindowCompositionAttribute often used, OR AllowsTransparency=false + AccentPolicy.
            // The existing WindowHelper seems to handle both. MainWindow uses AllowsTransparency=false (judging by WindowStyle=None + WindowChrome but no Transparency explicit in top tag, defaulting to false, but background is transparent key?). 
            // Wait, MainWindow has Background="#99101010".
            
            Background = System.Windows.Media.Brushes.Transparent; // Important for backdrop to show through
            ResizeMode = ResizeMode.CanResize;

            // Bind standard window commands
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindow, OnCanResizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow, OnCanMinimizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow, OnCanResizeWindow));
        }

        protected override void OnSourceInitialized(System.EventArgs e)
        {
            base.OnSourceInitialized(e);
            WindowHelper.ApplySystemBackdrop(this);
        }

        private void OnCloseWindow(object target, ExecutedRoutedEventArgs e) => SystemCommands.CloseWindow(this);
        private void OnMaximizeWindow(object target, ExecutedRoutedEventArgs e) => SystemCommands.MaximizeWindow(this);
        private void OnMinimizeWindow(object target, ExecutedRoutedEventArgs e) => SystemCommands.MinimizeWindow(this);
        private void OnRestoreWindow(object target, ExecutedRoutedEventArgs e) => SystemCommands.RestoreWindow(this);

        private void OnCanResizeWindow(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip;
        private void OnCanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = ResizeMode != ResizeMode.NoResize;
    }
}
