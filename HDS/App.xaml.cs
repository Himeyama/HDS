using Microsoft.UI.Xaml;

namespace HDS;
public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Window mainWindow = new MainWindow();
        mainWindow.Activate();
    }
}