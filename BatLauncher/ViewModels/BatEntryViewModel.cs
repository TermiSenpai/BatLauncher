using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BatLauncher.Models;

namespace BatLauncher.ViewModels;

public class BatEntryViewModel : BaseViewModel
{
    public BatEntry Model { get; }

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
        set { Model.IconPath = value; OnPropertyChanged(); OnPropertyChanged(nameof(Icon)); OnPropertyChanged(nameof(HasIcon)); }
    }

    public bool HasIcon => !string.IsNullOrEmpty(IconPath) && File.Exists(IconPath);

    public bool FileExists => File.Exists(FilePath);

    public ImageSource? Icon
    {
        get
        {
            try
            {
                if (string.IsNullOrEmpty(IconPath) || !File.Exists(IconPath))
                    return null;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(IconPath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 48;
                bitmap.EndInit();
                bitmap.Freeze();
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
