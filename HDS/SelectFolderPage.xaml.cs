using System;
using System.IO;
using Common;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace HDS;

public sealed partial class SelectFolderPage : Page
{
    MainWindow mainWindow;

    public SelectFolderPage()
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
            SelectFolderDescription.Text = string.Format(SelectFolderDescription.Text, mainWindow.formalAppName);
            InstallationPathTextBox.Text = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), mainWindow.appName);

            if (File.Exists(mainWindow.packageFilePath))
            {
                FileInfo fileInfo = new(mainWindow.packageFilePath);
                double requiredGiB = fileInfo.Length / (1024.0 * 1024);
                mainWindow.installFileSize = (int)fileInfo.Length;
                RequiredSize.Text = $"{requiredGiB:F1} MiB";
            }

            UpdateDriveSpaceInfo(InstallationPathTextBox.Text);
        }
    }

    void UpdateDriveSpaceInfo(string path)
    {
        try
        {
            DriveInfo driveInfo = new(Path.GetPathRoot(path));
            long availableFreeSpace = driveInfo.AvailableFreeSpace;
            long totalSize = driveInfo.TotalSize;
            double freeSpaceGiB = availableFreeSpace / (1024.0 * 1024 * 1024);
            FreeSpaceSize.Text = $"{freeSpaceGiB:F1} GiB";
        }
        catch (Exception ex)
        {
            FreeSpaceSize.Text = "Error retrieving free space";
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    void NextButton_Click(object sender, RoutedEventArgs e)
    {
        string parentPath = Path.GetDirectoryName(InstallationPathTextBox.Text);
        if (Directory.Exists(parentPath))
        {
            // インストール先フォルダが存在する場合は、確認ダイアログを表示
            mainWindow.installationPath = InstallationPathTextBox.Text;
        }
        else
        {
            // インストール先フォルダが存在しない場合は、次のページに遷移
            _ = Dialog.ShowError(mainWindow.Content, mainWindow.InvalidPath.Text);
            return;
        }

        // 次のページに遷移
        mainWindow.ContentFrame.Navigate(
            typeof(InstallProgressPage),
            mainWindow,
            new Microsoft.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo()
        );
    }

    void BackButton_Click(object sender, RoutedEventArgs e)
    {
        // 前のページに戻る
        mainWindow.ContentFrame.Navigate(
            typeof(LicenseAgreementPage),
            mainWindow,
            new Microsoft.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo()
        );
    }

    void CancelButton_Click(object sender, RoutedEventArgs e) => mainWindow.CancelButton_Click();

    async void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        string installationPath = InstallationPathTextBox.Text;
        string defaultInstallPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (installationPath != "" && Directory.Exists(installationPath))
        {
            defaultInstallPath = installationPath;
        }

        // Create a folder picker
        FolderPicker openPicker = new()
        {
            SuggestedStartLocation = PickerLocationId.ComputerFolder,
            FileTypeFilter = { "*" } // Allow all file types,
                                     // Set the default folder path
                                     // Note: The FolderPicker does not support setting a default folder path directly.
                                     // but you can set the initial folder by using the SuggestedStartLocation.
        };

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        nint hWnd = WindowNative.GetWindowHandle(mainWindow);

        // Initialize the folder picker with the window handle (HWND).
        InitializeWithWindow.Initialize(openPicker, hWnd);

        // Open the picker for the user to pick a folder
        StorageFolder folder = await openPicker.PickSingleFolderAsync();

        if (folder != null)
        {
            InstallationPathTextBox.Text = folder.Path;
            UpdateDriveSpaceInfo(InstallationPathTextBox.Text);
        }
    }
}