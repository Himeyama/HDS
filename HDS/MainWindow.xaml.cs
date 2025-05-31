using System;
using System.IO;
using System.Text.RegularExpressions;
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
            Match match = Regex.Match(fileName, @"^([\w]+)-(\d+\.\d+\.\d+)\.zip$");
            if (match.Success)
            {
                appName = match.Groups[1].Value;
                formalAppName = match.Groups[1].Value;
                version = match.Groups[2].Value;
                break;
            }
            match = Regex.Match(fileName, @"^([\w]+)-([\w\s]+)-([\d\.]+)\.zip$");
            if (match.Success)
            {
                appName = match.Groups[1].Value;
                formalAppName = match.Groups[2].Value;
                version = match.Groups[3].Value;
                break;
            }
        }
        // ContentFrame.Content = new FirstPage();
        ContentFrame.Navigate(typeof(FirstPage), this, new DrillInNavigationTransitionInfo());

        // Navigate(typeof(HomePage));
        // SetTitleBar(AppTitleBar);
        // Title = AppTitleTextBlock.Text;

        // Status.statusBar = StatusBar;
        // Status.dispatcherQueue = DispatcherQueue;
        // ZoomIn.KeyboardAcceleratorTextOverride = ZoomInText.Text;
        // ZoomOut.KeyboardAcceleratorTextOverride = ZoomOutText.Text;
    }

    void SetWindowSize(int width, int height)
    {
        IntPtr hWnd = WindowNative.GetWindowHandle(this);
        WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        AppWindow appWindow = AppWindow.GetFromWindowId(myWndId);
        appWindow.Resize(new SizeInt32(width, height));
    }

    // void AutoSave_Toggled(object sender, RoutedEventArgs e)
    // {
    //     try{
    //         EnableAutoSave.Visibility = AutoSave.IsOn ? Visibility.Visible : Visibility.Collapsed;
    //         DisableAutoSave.Visibility = AutoSave.IsOn ? Visibility.Collapsed : Visibility.Visible;
    //     }catch(Exception ex){
    //         Status.AddMessage(ex.Message);
    //     }
    // }

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