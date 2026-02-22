using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BatLauncher.Models;

namespace BatLauncher.ViewModels;

public class BatEntryViewModel : BaseViewModel
{
    public BatEntry Model { get; }
    private ImageSource? _cachedIcon;
    private string? _cachedIconPath;

    public string Id => Model.Id;

    public string Name
    {
        get => Model.Name;
        set { Model.Name = value; OnPropertyChanged(); }
    }

    public string FilePath
    {
        get => Model.FilePath;
        set { Model.FilePath = value; OnPropertyChanged(); OnPropertyChanged(nameof(FileExists)); }
    }

    public string? IconPath
    {
        get => Model.IconPath;
        set
        {
            Model.IconPath = value;
            _cachedIcon = null;
            _cachedIconPath = null;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Icon));
            OnPropertyChanged(nameof(HasIcon));
        }
    }

    public bool HasIcon => !string.IsNullOrEmpty(IconPath) && File.Exists(IconPath);

    public bool FileExists => File.Exists(FilePath);

    public ImageSource? Icon
    {
        get
        {
            if (string.IsNullOrEmpty(IconPath) || !File.Exists(IconPath))
                return null;

            if (_cachedIcon != null && _cachedIconPath == IconPath)
                return _cachedIcon;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(IconPath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 36;
                bitmap.EndInit();
                bitmap.Freeze();
                _cachedIcon = bitmap;
                _cachedIconPath = IconPath;
                return bitmap;
            }
            catch
            {
                return null;
            }
        }
    }

    public BatEntryViewModel(BatEntry model)
    {
        Model = model;
    }
}
