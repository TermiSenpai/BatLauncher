using System.Windows;
using System.Windows.Threading;
using BatLauncher.ViewModels;
using BatLauncher.Views;

namespace BatLauncher;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var vm = new MainViewModel();
        var toolbar = new ToolbarWindow(vm);
        MainWindow = toolbar;
        toolbar.Show();

        // Load data after the window is already visible
        Dispatcher.InvokeAsync(() => vm.LoadData(), DispatcherPriority.Background);
    }
}
