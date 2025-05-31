using System;
using System.IO;
using System.Text.Json.Serialization;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using YamlDotNet.Serialization;

namespace HDS;

public class I18nLicense
{
    [JsonPropertyName("en")]
    [YamlMember(Alias = "en")]
    public string English { get; set; }

    [JsonPropertyName("ja")]
    [YamlMember(Alias = "ja")]
    public string Japanese { get; set; }

    [JsonPropertyName("zh")]
    [YamlMember(Alias = "zh")]
    public string Chinese { get; set; }

    [JsonPropertyName("ko")]
    [YamlMember(Alias = "ko")]
    public string Korean { get; set; }

    [JsonPropertyName("fr")]
    [YamlMember(Alias = "fr")]
    public string French { get; set; }

    [JsonPropertyName("de")]
    [YamlMember(Alias = "de")]
    public string German { get; set; }

    [JsonPropertyName("es")]
    [YamlMember(Alias = "es")]
    public string Spanish { get; set; }

    [JsonPropertyName("pt")]
    [YamlMember(Alias = "pt")]
    public string Portuguese { get; set; }
}


public sealed partial class LicenseAgreementPage : Page
{
    MainWindow mainWindow;

    public LicenseAgreementPage()
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
            LicenseInstructions.Text = string.Format(
                LicenseInstructions.Text,
                window.formalAppName);

            string licenseFile = Path.Combine("LICENSE.yaml");
            if (File.Exists(licenseFile))
            {
                string licenseYaml = File.ReadAllText(licenseFile);
                I18nLicense yaml;
                try
                {
                    IDeserializer deserializer = new DeserializerBuilder().Build();
                    yaml = deserializer.Deserialize<I18nLicense>(licenseYaml);
                }
                catch (Exception)
                {
                    LicenseText.Text = "Failed to load LICENSE.yaml";
                    return;
                }
                switch (mainWindow.Lang.Text)
                {
                    case "ja":
                        LicenseText.Text = yaml.Japanese;
                        break;
                    case "en":
                        LicenseText.Text = yaml.English;
                        break;
                    case "zh":
                        LicenseText.Text = yaml.Chinese;
                        break;
                    case "ko":
                        LicenseText.Text = yaml.Korean;
                        break;
                    case "fr":
                        LicenseText.Text = yaml.French;
                        break;
                    case "de":
                        LicenseText.Text = yaml.German;
                        break;
                    case "es":
                        LicenseText.Text = yaml.Spanish;
                        break;
                    case "pt":
                        LicenseText.Text = yaml.Portuguese;
                        break;
                    default:
                        LicenseText.Text = yaml.English;
                        break;
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