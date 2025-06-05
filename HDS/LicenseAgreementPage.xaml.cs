using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HDS;


public sealed partial class LicenseAgreementPage : Page
{
    MainWindow mainWindow;

    public LicenseAgreementPage()
    {
        InitializeComponent();
    }

    static Dictionary<string, string> ReadLicenseFile(string filePath)
    {
        Dictionary<string, string> licenseDict = new Dictionary<string, string>();

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"指定されたファイルが見つかりません: {filePath}");
        }

        string content = File.ReadAllText(filePath);
        content = NormalizeLineEndings(content);

        Match enMatch = Regex.Match(content, @"---\s*en\s*(.*?)\s*---", RegexOptions.Singleline);
        Match jaMatch = Regex.Match(content, @"---\s*ja\s*(.*?)\s*$", RegexOptions.Singleline);

        if (enMatch.Success) licenseDict["en"] = enMatch.Groups[1].Value.Trim();
        if (jaMatch.Success) licenseDict["ja"] = jaMatch.Groups[1].Value.Trim();

        return licenseDict;
    }

    static string NormalizeLineEndings(string text)
    {
        return text.Replace("\r\n", "\n").Replace("\r", "\n");
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
            LicenseInstructions.Text = string.Format(
                LicenseInstructions.Text,
                window.formalAppName);

            string licenseFile = Path.Combine("LICENSE.md");
            if (File.Exists(licenseFile))
            {
                Dictionary<string, string> licenseData = ReadLicenseFile(licenseFile);
                bool isLicenseFound = false;
                foreach (KeyValuePair<string, string> entry in licenseData)
                {
                    if (mainWindow.Lang.Text == entry.Key)
                    {
                        LicenseText.Text = entry.Value;
                        isLicenseFound = true;
                        break;
                    }
                }
                if (!isLicenseFound)
                {
                    LicenseText.Text = licenseData.ContainsKey("en") ? licenseData["en"] : "License not found in the specified language.";
                }
            }
        }
    }

    void NextButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // 次のページに遷移
        mainWindow.ContentFrame.Navigate(
            typeof(SelectFolderPage),
            mainWindow,
            new Microsoft.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo()
        );
    }

    void BackButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // 前のページに戻る
        mainWindow.ContentFrame.Navigate(
            typeof(FirstPage),
            mainWindow,
            new Microsoft.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo()
        );
    }

    void CancelButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) => mainWindow.CancelButton_Click();
}