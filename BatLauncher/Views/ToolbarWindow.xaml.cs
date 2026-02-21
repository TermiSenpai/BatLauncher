using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BatLauncher.Models;
using BatLauncher.Services;
using BatLauncher.ViewModels;

namespace BatLauncher.Views;

public partial class ToolbarWindow : Window
{
    private readonly MainViewModel _vm;
    private MainWindow? _managementWindow;

    public ToolbarWindow(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;

        Loaded += OnLoaded;
        StateChanged += OnStateChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        PositionAboveTaskbar();
        UpdateSeparatorVisibility();
        _vm.AllEntries.CollectionChanged += (_, _) => UpdateSeparatorVisibility();
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Normal)
            PositionAboveTaskbar();
    }

    private void PositionAboveTaskbar()
    {
        // Wait for layout to calculate size
        UpdateLayout();

        var workArea = SystemParameters.WorkArea;
        Left = workArea.Left + (workArea.Width - ActualWidth) / 2;
        Top = workArea.Bottom - ActualHeight - 4;
    }

    private void UpdateSeparatorVisibility()
    {
        Separator1.Visibility = _vm.AllEntries.Count > 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void Bar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
            DragMove();
    }

    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is ScrollViewer sv)
        {
            sv.ScrollToHorizontalOffset(sv.HorizontalOffset - e.Delta);
            e.Handled = true;
        }
    }

    private void ScriptIcon_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is BatEntryViewModel entry)
        {
            RunScript(entry);
        }
    }

    private void RunScript(BatEntryViewModel entry)
    {
        if (!File.Exists(entry.FilePath))
        {
            MessageBox.Show($"File not found:\n{entry.FilePath}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            var ext = Path.GetExtension(entry.FilePath).ToLowerInvariant();
            var psi = ext is ".bat" or ".cmd"
                ? new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{entry.FilePath}\"",
                    WorkingDirectory = Path.GetDirectoryName(entry.FilePath) ?? "",
                    UseShellExecute = true
                }
                : new ProcessStartInfo
                {
                    FileName = entry.FilePath,
                    WorkingDirectory = Path.GetDirectoryName(entry.FilePath) ?? "",
                    UseShellExecute = true
                };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        var entry = new BatEntry();
        var dlg = new EditDialog(entry, true);
        if (dlg.ShowDialog() == true && dlg.Result != null)
        {
            _vm.AddEntryDirect(dlg.Result);
        }
    }

    private void Manage_Click(object sender, RoutedEventArgs e)
    {
        if (_managementWindow == null || !_managementWindow.IsLoaded)
        {
            _managementWindow = new MainWindow(_vm);
            _managementWindow.Closed += (_, _) => _managementWindow = null;
            _managementWindow.Show();
        }
        else
        {
            _managementWindow.Activate();
            if (_managementWindow.WindowState == WindowState.Minimized)
                _managementWindow.WindowState = WindowState.Normal;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        _managementWindow?.Close();
        Close();
    }
}
