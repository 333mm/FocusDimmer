using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace FocusDimmer
{
    public class ProcessItem
    {
        public string Name { get; set; } = "";
        public string ProcessName { get; set; } = "";
        public string Description { get; set; } = "";
        public BitmapSource? Icon { get; set; }
    }

    public partial class ProcessSelectionWindow : Components.FluentWindow
    {
        public FocusDimmer.Services.LocalizationService Strings { get; }
        public string SelectedProcessName { get; private set; } = "";
        public string SelectedProcessDescription { get; private set; } = "";
        private List<ProcessItem> _allProcesses = new();
        private System.Windows.Controls.Button? _anchorButton;

        public ProcessSelectionWindow(FocusDimmer.Services.LocalizationService strings, System.Windows.Controls.Button? anchorButton)
        {
            Strings = strings;
            _anchorButton = anchorButton;
            DataContext = this; // Ensure binding works
            
            InitializeComponent();
            Loaded += ProcessSelectionWindow_Loaded;
            
            // Apply Acrylic/Dark mode styles
            // FocusDimmer.Helpers.WindowHelper.ApplySystemBackdrop(this); // Disabled per user request (Opaque mode)
        }

        private async void ProcessSelectionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PositionWindow();
            RefreshProcessList();
        }

        private void PositionWindow()
        {
            if (_anchorButton == null) return;

            // Get DPI scale
            var source = PresentationSource.FromVisual(_anchorButton);
            double dpiX = 1.0;
            double dpiY = 1.0;
            if (source != null && source.CompositionTarget != null)
            {
                dpiX = source.CompositionTarget.TransformToDevice.M11;
                dpiY = source.CompositionTarget.TransformToDevice.M22;
            }

            // Get button position
            System.Windows.Point buttonPoint = _anchorButton.PointToScreen(new System.Windows.Point(0, 0));
            
            // Default: Below the button, aligned left
            // Convert device pixels (PointToScreen) to logical units
            double left = buttonPoint.X / dpiX;
            double top = (buttonPoint.Y / dpiY) + _anchorButton.ActualHeight + 5;

            // Screen bounds check
            var screenWidth = SystemParameters.WorkArea.Width;
            var screenHeight = SystemParameters.WorkArea.Height;

            // If goes off right edge, align right
            if (left + Width > screenWidth)
            {
                left = (buttonPoint.X / dpiX) + _anchorButton.ActualWidth - Width;
            }
            
            // If goes off bottom, show above button
            if (top + Height > screenHeight)
            {
                top = (buttonPoint.Y / dpiY) - Height - 5;
            }
            
            // Final safety clamp
            if (left < 0) left = 0;
            if (top < 0) top = 0;

            Left = left;
            Top = top;
        }

        private void RefreshProcessList()
        {
             _allProcesses = new List<ProcessItem>();
            var processes = Process.GetProcesses();

            foreach (var p in processes)
            {
                try
                {
                    if (p.MainWindowHandle == IntPtr.Zero) continue; // Only windowed apps
                    if (string.IsNullOrWhiteSpace(p.MainWindowTitle)) continue;

                    var icon = GetIcon(p);
                    string description = "";
                    try
                    {
                        if (p.MainModule?.FileVersionInfo != null)
                        {
                            description = p.MainModule.FileVersionInfo.FileDescription ?? "";
                        }
                    }
                    catch { }

                    if (string.IsNullOrEmpty(description)) description = p.ProcessName;

                    _allProcesses.Add(new ProcessItem
                    {
                        Name = p.MainWindowTitle,
                        ProcessName = p.ProcessName,
                        Description = description,
                        Icon = icon
                    });
                }
                catch { }
            }

            // Remove duplicates
            _allProcesses = _allProcesses.GroupBy(x => x.ProcessName).Select(g => g.First()).OrderBy(x => x.Name).ToList();
            
            ProcessList.ItemsSource = _allProcesses;
        }

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        static extern uint GetClassLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
        static extern IntPtr GetClassLong64(IntPtr hWnd, int nIndex);

        private static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8) return GetClassLong64(hWnd, nIndex);
            else return new IntPtr(GetClassLong32(hWnd, nIndex));
        }

        private const uint WM_GETICON = 0x007f;
        private const int GCLP_HICON = -14;
        private const int GCLP_HICONSM = -34;
        private static IntPtr ICON_SMALL = new IntPtr(0);
        private static IntPtr ICON_BIG = new IntPtr(1);

        private BitmapSource? GetIcon(Process p)
        {
            try
            {
                IntPtr hIcon = SendMessage(p.MainWindowHandle, WM_GETICON, ICON_SMALL, IntPtr.Zero);
                
                if (hIcon == IntPtr.Zero)
                    hIcon = SendMessage(p.MainWindowHandle, WM_GETICON, ICON_BIG, IntPtr.Zero);

                if (hIcon == IntPtr.Zero)
                    hIcon = GetClassLongPtr(p.MainWindowHandle, GCLP_HICONSM);

                if (hIcon == IntPtr.Zero)
                    hIcon = GetClassLongPtr(p.MainWindowHandle, GCLP_HICON);

                if (hIcon != IntPtr.Zero)
                {
                    var icon = Imaging.CreateBitmapSourceFromHIcon(
                        hIcon,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    
                    // Do not destroy icon if obtained from GetClassLong? 
                    // Actually, icons from SendMessage(WM_GETICON) or GetClassLong belong to the window/class, 
                    // we should NOT destroy them. CreateBitmapSourceFromHIcon creates a copy.
                    return icon;
                }
            }
            catch { }
            
            return null; 
        }



        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allProcesses == null) return;
            var query = SearchBox.Text.ToLower();
            var filtered = _allProcesses.Where(p => 
                p.Name.ToLower().Contains(query) || 
                p.ProcessName.ToLower().Contains(query)).ToList();
            ProcessList.ItemsSource = filtered;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshProcessList();
        }

        private void ProcessList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Select_Click(sender, e);
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessList.SelectedItem is ProcessItem item)
            {
                SelectedProcessName = item.ProcessName.ToLower();
                SelectedProcessDescription = item.Description;
                DialogResult = true;
                Close();
            }
        }

        private void Window_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    string file = files[0];
                    if (file.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        SelectedProcessName = System.IO.Path.GetFileNameWithoutExtension(file).ToLower();
                        DialogResult = true;
                        Close();
                    }
                }
            }
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            // Fallback C: Out-of-process file dialog using PowerShell.
            // This completely isolates the file dialog from our processes, avoiding ExplorerBlurMica.dll hooks entirely.
            try
            {
                var psCommand = "Add-Type -AssemblyName System.Windows.Forms; $d = New-Object System.Windows.Forms.OpenFileDialog; $d.Filter = 'Executable files (*.exe)|*.exe|All files (*.*)|*.*'; $d.Title = 'Select App (Safe Mode)'; if($d.ShowDialog() -eq 'OK'){ $d.FileName }";
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -Command \"{psCommand}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        var output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();

                        if (!string.IsNullOrWhiteSpace(output))
                        {
                            var path = output.Trim();
                            if (System.IO.File.Exists(path))
                            {
                                SelectedProcessName = System.IO.Path.GetFileNameWithoutExtension(path).ToLower();
                                try 
                                {
                                    var info = FileVersionInfo.GetVersionInfo(path);
                                    SelectedProcessDescription = info.FileDescription ?? SelectedProcessName;
                                }
                                catch 
                                {
                                    SelectedProcessDescription = SelectedProcessName;
                                }
                                DialogResult = true;
                                Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error opening safe file dialog: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch { }
        }
    }
}
