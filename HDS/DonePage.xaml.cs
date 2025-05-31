using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HDS;

public sealed partial class DonePage : Page
{
    MainWindow mainWindow;

    public DonePage()
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
        }
    }

    void DoneButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        App.Current.Exit();
    }
}