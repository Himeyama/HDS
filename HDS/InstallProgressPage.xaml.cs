using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Common;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Win32;

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
                string localAppData = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                string fullExtractPath = Path.GetFullPath(extractPath);

                if (!string.Equals(localAppData.TrimEnd(Path.DirectorySeparatorChar), fullExtractPath.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                {
                    if (Directory.Exists(fullExtractPath))
                    {
                        Directory.Delete(fullExtractPath, true);
                    }
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

            RegistApp(
                mainWindow.appName,
                mainWindow.formalAppName,
                mainWindow.publisher,
                mainWindow.installationPath,
                mainWindow.version
            );
        }
        catch (Exception ex)
        {
            _ = Dialog.ShowError(mainWindow.Content, ex.Message);
            return;
        }
    }

    void RegistApp(string appName, string formalAppName, string publisher, string installPath, string version)
    {
        string smPrograms = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
        string execFile = $"{appName}.exe";
        string date = DateTime.Now.ToString("yyyyMMdd");
        int size = mainWindow.installFileSize / 1024; // Estimated size in KB

        // ディレクトリ作成
        string programPath = Path.Combine(smPrograms, "Programs", formalAppName);
        Directory.CreateDirectory(programPath);

        // ショートカット作成
        CreateShortcut(Path.Combine(programPath, $"{formalAppName}.lnk"), Path.Combine(installPath, execFile));

        // レジストリ登録
        string registryKey = $@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{appName}";
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(registryKey);
        key.SetValue("DisplayName", formalAppName);
        key.SetValue("UninstallString", $"\"{installPath}\\Uninstall.exe\"");
        key.SetValue("Publisher", publisher);
        key.SetValue("DisplayIcon", $"{installPath}\\{execFile},0");
        key.SetValue("DisplayVersion", version);
        key.SetValue("InstallDate", date);
        key.SetValue("EstimatedSize", size, RegistryValueKind.DWord);
    }

    static void CreateShortcut(string shortcutPath, string targetPath)
    {
        // PowerShellスクリプトを動的に生成
        string script = $@"
            $WScriptShell = New-Object -ComObject WScript.Shell
            $Shortcut = $WScriptShell.CreateShortcut('{shortcutPath}')
            $Shortcut.TargetPath = '{targetPath}'
            $Shortcut.Save()";
        RunPowerShellScript(script);
    }
    
    static void RunPowerShellScript(string script)
    {
        ProcessStartInfo psi = new()
        {
            FileName = "powershell.exe",
            Arguments = $"-Command \"{script}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using Process process = Process.Start(psi);
        process.WaitForExit();
    }
}