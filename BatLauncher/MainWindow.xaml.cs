using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using BatLauncher.Models;
using BatLauncher.ViewModels;
using BatLauncher.Views;

namespace BatLauncher;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        _vm.OnEditRequested += OnEditRequested;
        _vm.FilteredEntries.CollectionChanged += FilteredEntries_Changed;
        DataContext = _vm;
        UpdateEmptyState();
        UpdateStatus();
    }

    protected override void OnClosed(EventArgs e)
    {
        _vm.OnEditRequested -= OnEditRequested;
        _vm.FilteredEntries.CollectionChanged -= FilteredEntries_Changed;
        base.OnClosed(e);
    }

    private BatEntry? OnEditRequested(BatEntry? entry)
    {
        if (entry == null) return null;
        bool isNew = string.IsNullOrEmpty(entry.Name);
        var dlg = new EditDialog(entry, isNew) { Owner = this };
        return dlg.ShowDialog() == true ? dlg.Result : null;
    }

    private void FilteredEntries_Changed(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateEmptyState();
        UpdateStatus();
    }

    private void UpdateEmptyState()
    {
        EmptyState.Visibility = _vm.FilteredEntries.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void UpdateStatus()
    {
        StatusText.Text = $"{_vm.FilteredEntries.Count} script{(_vm.FilteredEntries.Count != 1 ? "s" : "")}";
    }

    private void EntryList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_vm.SelectedEntry != null)
            _vm.RunCommand.Execute(_vm.SelectedEntry);
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            ToggleMaximize();
        else
            DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void MaximizeRestore_Click(object sender, RoutedEventArgs e) => ToggleMaximize();

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void ToggleMaximize()
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
            MaxRestoreBtn.Content = "\uE739";
        }
        else
        {
            WindowState = WindowState.Maximized;
            MaxRestoreBtn.Content = "\uE923";
        }
    }
}
