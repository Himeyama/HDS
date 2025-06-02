using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Graphics;
using WinRT.Interop;


namespace HDS;

public sealed partial class MainWindow : Window
{
    public string appName = "";
    public string formalAppName = "";
    public string packageFilePath = "";
    public string version = "";
    public string installationPath = "";
    public string publisher = "";
    public int installFileSize = 0;

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetWindowSize(1200, 800);

        string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.zip");
        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            packageFilePath = file;
            Match match = Regex.Match(fileName, @"^([\w]+)-([\p{L}\p{N} ]+)-([\p{L}\p{N} ]+)-([\d]+\.[\d]+\.[\d]+)\.zip$");
            if (match.Success)
            {
                // TestApp-テスト アプリ-ひかり-1.0.0.zip
                appName = match.Groups[1].Value;
                formalAppName = match.Groups[2].Value;
                publisher = match.Groups[3].Value;
                version = match.Groups[4].Value;
                break;
            }
        }
        if (appName == "" || formalAppName == "" || publisher == "")
        {
            Content = new TextBlock
            {
                Text = "Error: Invalid package file name format. Expected format: '<AppName>-<FormalName>-<Publisher>-<Version>.zip'.",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 20,
                Margin = new Thickness(16, 24, 16, 16),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.Red)
            };
            return;
        }
        if (!CheckName(appName) || !CheckName(formalAppName) || !CheckName(publisher))
        {
            // Invalid app name or formal name
            Content = new TextBlock
            {
                Text = "Error: Invalid app name or formal name. Names must not be empty, longer than 50 characters, and must not contain invalid characters.",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 20,
                Margin = new Thickness(16, 24, 16, 16),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.Red)
            };
            return;
        }
        ContentFrame.Navigate(typeof(FirstPage), this, new DrillInNavigationTransitionInfo());
    }

    static bool CheckName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }
        if (name.Length > 50)
        {
            return false;
        }
        // Check for invalid characters: /:*?"'<>|\[]{};^=!#$%&`() 
        if (Regex.IsMatch(name, @"[\/:\*\?""'<>|\\\[\]\{\};\^=!#\$%&\(\)`]"))
        {
            return false;
        }
        return true;
    }

    void SetWindowSize(int width, int height)
    {
        IntPtr hWnd = WindowNative.GetWindowHandle(this);
        WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        AppWindow appWindow = AppWindow.GetFromWindowId(myWndId);
        appWindow.Resize(new SizeInt32(width, height));
    }

    async void ClickOpen(object sender, RoutedEventArgs e)
    {
        await FilePicker.Open(this);
    }

    async void ClickSave(object sender, RoutedEventArgs e)
    {
        await FilePicker.Save(this, "Save");
    }

    async void ClickSaveAs(object sender, RoutedEventArgs e)
    {
        await FilePicker.Save(this, "Save as");
    }

    void ClickZoomIn(object sender, RoutedEventArgs e)
    {
        Status.AddMessage($"Zoom In");
    }

    void ClickZoomOut(object sender, RoutedEventArgs e)
    {
        Status.AddMessage($"Zoom out");
    }

    void ClickRestoreDefaultZoom(object sender, RoutedEventArgs e)
    {
        Status.AddMessage($"Restore default zoom");
    }

    async void ClickAbout(object sender, RoutedEventArgs e)
    {
        await Dialog.Show(Content, "This app is an example app for Windows App SDK!", "About");
        Status.AddMessage($"Thank you for using this app!");
    }

    void ClickExit(object sender, RoutedEventArgs e)
    {
        Close();
    }

    public async void CancelButton_Click()
    {
        ContentDialogResult result = await Dialog.ShowOKCancel(
            Content,
            CancelConfirmText.Text,
            ApplicationSetupText.Text
        );
        if (result == ContentDialogResult.Primary)
        {
            Close();
        }
    }
}