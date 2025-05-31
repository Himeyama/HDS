using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HDS;

public sealed partial class FirstPage : Page
{
    MainWindow mainWindow;

    public FirstPage()
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
            SetupTitle.Text = string.Format(SetupTitle.Text, mainWindow.formalAppName, mainWindow.version);  
            FirstPageDescription.Text = string.Format(FirstPageDescription.Text,
                mainWindow.formalAppName, mainWindow.version
            );
        }
    }

    void NextButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // 次のページに遷移
        mainWindow.ContentFrame.Navigate(
            typeof(LicenseAgreementPage),
            mainWindow,
            new Microsoft.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo()
        );
    }

    void CancelButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) => mainWindow.CancelButton_Click();
}