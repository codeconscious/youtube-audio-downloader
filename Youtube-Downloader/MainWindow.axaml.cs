using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Youtube_Downloader
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;

            button.Content = "OK";
        }
    }
}