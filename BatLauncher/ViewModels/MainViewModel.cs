using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using BatLauncher.Models;
using BatLauncher.Services;

namespace BatLauncher.ViewModels;

public class MainViewModel : BaseViewModel
{
    private AppData _appData = new();
    private BatEntryViewModel? _selectedEntry;
    private string _searchText = string.Empty;
    private ObservableCollection<BatEntryViewModel> _filteredEntries = [];

    public ObservableCollection<BatEntryViewModel> AllEntries { get; } = [];

    public ObservableCollection<BatEntryViewModel> FilteredEntries
    {
        get => _filteredEntries;
        set => SetField(ref _filteredEntries, value);
    }

    public BatEntryViewModel? SelectedEntry
    {
        get => _selectedEntry;
        set => SetField(ref _selectedEntry, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetField(ref _searchText, value))
                ApplyFilter();
        }
    }

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand RunCommand { get; }

    public event Func<BatEntry?, BatEntry?>? OnEditRequested;

    public MainViewModel()
    {
        AddCommand = new RelayCommand(_ => AddEntry());
        EditCommand = new RelayCommand(_ => EditEntry(), _ => SelectedEntry != null);
        DeleteCommand = new RelayCommand(_ => DeleteEntry(), _ => SelectedEntry != null);
        RunCommand = new RelayCommand(p => RunEntry(p as BatEntryViewModel));
    }

    public void LoadData()
    {
        _appData = DataService.Load();

        foreach (var entry in _appData.Entries)
            AllEntries.Add(new BatEntryViewModel(entry));

        ApplyFilter();
    }

    private void AddEntry()
    {
        var newEntry = new BatEntry();
        var result = OnEditRequested?.Invoke(newEntry);
        if (result == null) return;

        _appData.Entries.Add(result);
        var vm = new BatEntryViewModel(result);
        AllEntries.Add(vm);
        SelectedEntry = vm;
        ApplyFilter();
        Save();
    }

    private void EditEntry()
    {
        if (SelectedEntry == null) return;

        var clone = new BatEntry
        {
            Id = SelectedEntry.Model.Id,
            Name = SelectedEntry.Model.Name,
            FilePath = SelectedEntry.Model.FilePath,
            IconPath = SelectedEntry.Model.IconPath,
            CreatedAt = SelectedEntry.Model.CreatedAt
        };

        var result = OnEditRequested?.Invoke(clone);
        if (result == null) return;

        SelectedEntry.Name = result.Name;
        SelectedEntry.FilePath = result.FilePath;
        SelectedEntry.IconPath = result.IconPath;

        var idx = _appData.Entries.FindIndex(e => e.Id == result.Id);
        if (idx >= 0) _appData.Entries[idx] = SelectedEntry.Model;

        ApplyFilter();
        Save();
    }

    private void DeleteEntry()
    {
        if (SelectedEntry == null) return;

        var result = MessageBox.Show(
            $"Delete \"{SelectedEntry.Name}\"?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        DataService.DeleteIcon(SelectedEntry.IconPath);
        _appData.Entries.RemoveAll(e => e.Id == SelectedEntry.Id);
        AllEntries.Remove(SelectedEntry);
        SelectedEntry = null;
        ApplyFilter();
        Save();
    }

    private void RunEntry(BatEntryViewModel? entry)
    {
        entry ??= SelectedEntry;
        if (entry == null) return;

        if (!File.Exists(entry.FilePath))
        {
            MessageBox.Show(
                $"File not found:\n{entry.FilePath}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{entry.FilePath}\"",
                WorkingDirectory = Path.GetDirectoryName(entry.FilePath) ?? "",
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error running script:\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            FilteredEntries = new ObservableCollection<BatEntryViewModel>(AllEntries);
        }
        else
        {
            var lower = _searchText.ToLowerInvariant();
            FilteredEntries = new ObservableCollection<BatEntryViewModel>(
                AllEntries.Where(e =>
                    e.Name.Contains(lower, StringComparison.CurrentCultureIgnoreCase) ||
                    e.FilePath.Contains(lower, StringComparison.CurrentCultureIgnoreCase)));
        }
    }

    public void AddEntryDirect(BatEntry entry)
    {
        _appData.Entries.Add(entry);
        var vm = new BatEntryViewModel(entry);
        AllEntries.Add(vm);
        SelectedEntry = vm;
        ApplyFilter();
        Save();
    }

    private void Save() => DataService.Save(_appData);
}
