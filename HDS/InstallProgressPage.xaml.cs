using System;
using System.IO;
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

    protected override void OnNavigatedTo(NavigationEventArgs e)
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
            InstallPackage();
        }
    }

    async void InstallPackage()
    {
        try
        {
            string extractPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(extractPath);
            await Task.Run(() => System.IO.Compression.ZipFile.ExtractToDirectory(mainWindow.packageFilePath, extractPath));
            InstallProgressBar.Value = 50;

            string destinationPath = mainWindow.installationPath;
            if (Directory.Exists(destinationPath))
            {
                Directory.Delete(destinationPath, true);
            }
            Directory.CreateDirectory(destinationPath);
            // foreach (string dirPath in Directory.GetDirectories(extractPath, "*", SearchOption.AllDirectories))
            // {
            //     Directory.CreateDirectory(dirPath.Replace(extractPath, destinationPath));
            // }
            // string[] files = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories);

            // int count = 0;
            // foreach (string filePath in files)
            // {
            //     File.Copy(filePath, filePath.Replace(extractPath, destinationPath), true);
            //     count++;
            //     // InstallProgressBar.Value = 50 + count / files.Length * 50;
            // }
        }
        catch (Exception ex)
        {
            _ = Dialog.ShowError(mainWindow.Content, ex.Message);
            return;
        }
    }
}