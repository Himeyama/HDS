using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Win32;

namespace HDS;

public sealed partial class Uninstall : Page
{
    MainWindow mainWindow;
    string uninstallationPath = "";
    string formalAppName = "";
    string version = "";

    public Uninstall()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // e.Parameter からデータを取得
        // 取得したデータを使用する
        if (e.Parameter is MainWindow window)
        {
            // 例えば、テキストボックスに表示する
            mainWindow = window;

            if (mainWindow.uninstallAppName == "")
            {
                UninstallPageDescription.Text = "Application name is not set!!!";
                UninstallButton.IsEnabled = false;
                return;
            }
            uninstallationPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                mainWindow.uninstallAppName
            );
            if (!Directory.Exists(uninstallationPath))
            {
                UninstallTitle.Text = "Uninstallation Error";
                UninstallPageDescription.Text = "Uninstallation path does not exist.";
                UninstallButton.IsEnabled = false;
                return;
            }

            try
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    $@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{mainWindow.uninstallAppName}");
                if (key != null)
                {
                    formalAppName = key.GetValue("DisplayName") as string ?? "";
                    version = key.GetValue("DisplayVersion") as string ?? "";
                }
                else
                {
                    UninstallButton.IsEnabled = false;
                    UninstallTitle.Text = "Uninstallation Error";
                    UninstallPageDescription.Text = "The application is not installed or the registry key is missing.";
                    return;
                }
            }
            catch
            {
                UninstallPageDescription.Text += "\nError accessing registry.";
                UninstallButton.IsEnabled = false;
                return;
            }

            UninstallTitle.Text = string.Format(UninstallTitle.Text, formalAppName, version);
            UninstallPageDescription.Text = string.Format(UninstallPageDescription.Text, formalAppName, version);
        }
    }

    async void UninstallButton_Click(object sender, RoutedEventArgs e)
    {
        UninstallTitle.Text = UninstallDescription.Text;
        UninstallPageDescription.Text = "";

        string smPrograms = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
        string programPath = Path.Combine(smPrograms, "Programs", formalAppName);
        if (!string.IsNullOrEmpty(formalAppName) && Directory.Exists(programPath))
        {
            try
            {
                Directory.Delete(programPath, true);
            }
            catch (Exception ex)
            {
                UninstallPageDescription.Text = $"Error deleting Start Menu shortcut: {ex.Message}";
                return;
            }
        }

        // UninstallPageDescription.Text = mainWindow.installationPath;
        if (Directory.Exists(uninstallationPath))
        {
            try
            {
                await Task.Run(() => Directory.Delete(uninstallationPath, true));
            }
            catch (Exception ex)
            {
                UninstallPageDescription.Text = $"Error during uninstallation: {ex.Message}";
            }
        }
        else
        {
            UninstallPageDescription.Text = "Installation path does not exist.";
        }

        if (mainWindow.uninstallAppName != "")
        {
            InstallProgressPage.DeleteRegistryKey(mainWindow.uninstallAppName);
        }

        ContentDialog contentDialog = new()
        {
            XamlRoot = mainWindow.Content.XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = UninstallDoneDescription.Text,
            PrimaryButtonText = "OK",
            DefaultButton = ContentDialogButton.Primary,
            Content = "",
        };

        await contentDialog.ShowAsync();
        if (sender is Button button)
            button.Visibility = Visibility.Collapsed;

        UninstallTitle.Text = UninstallDoneDescription.Text;
    }
}