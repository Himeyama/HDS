using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Common;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.System.Inventory;

namespace HDS;

public sealed partial class InstallProgressPage : Page
{
    MainWindow mainWindow;

    public InstallProgressPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // e.Parameter からデータを取得
        // 取得したデータを使用する
        if (e.Parameter is MainWindow window)
        {
            // 例えば、テキストボックスに表示する
            mainWindow = window;
            InstallingDescription.Text = string.Format(InstallingDescription.Text, window.formalAppName);

            // mainWindow.packageFilePath からインストールパッケージを展開 (zip)
            await InstallPackage();

            InstallProgressBar.Value = 100;
            InstallationFile.Text = "";

            // 次のページに遷移
            mainWindow.ContentFrame.Navigate(
                typeof(DonePage),
                mainWindow,
                new Microsoft.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo()
            );
        }
    }

    async Task InstallPackage()
    {
        try
        {
            string extractPath = mainWindow.installationPath;
            int totalFiles = 0;
            int currentFile = 0;

            using (ZipArchive archive = ZipFile.OpenRead(mainWindow.packageFilePath))
            {
                totalFiles = archive.Entries.Count;
            }

            await Task.Run(() =>
            {
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
                Directory.CreateDirectory(extractPath);
                using ZipArchive archive = ZipFile.OpenRead(mainWindow.packageFilePath);
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string filePath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));
                    if (entry.Name == "" || Path.EndsInDirectorySeparator(entry.FullName))
                    {
                        continue;
                    }
                    string directoryPath = Path.GetDirectoryName(filePath);
                    if (directoryPath != null && !Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath); // Creates all directories in the path recursively
                    }
                    entry.ExtractToFile(filePath, true);
                    currentFile++;

                    if (mainWindow.DispatcherQueue.HasThreadAccess)
                    {
                        InstallProgressBar.Value = (double)currentFile / totalFiles * 100;
                        InstallationFile.Text = filePath;
                    }
                    else
                    {
                        bool isQueued = mainWindow.DispatcherQueue.TryEnqueue(
                            Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
                            () =>
                            {
                                InstallProgressBar.Value = (double)currentFile / totalFiles * 100;
                                InstallationFile.Text = filePath;
                            }
                        );
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _ = Dialog.ShowError(mainWindow.Content, ex.Message);
            return;
        }
    }
}