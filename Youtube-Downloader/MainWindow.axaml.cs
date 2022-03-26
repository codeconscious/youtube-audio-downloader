using System.Diagnostics;
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
            var urlTextBox = this.FindControl<TextBox>("Url");
            var url = urlTextBox.Text.Trim();
            if (url.Length < 11)
            {
                button.Content = "Too short!";
                return;
            }
            else if (url.Length > 11)
            {
                button.Content = "Too long!";
                return;
            }

            button.Content = "Working...";

            // Adapted from https://stackoverflow.com/a/1469790/11767771:
            // var process = new System.Diagnostics.Process();
            // var startInfo = new System.Diagnostics.ProcessStartInfo();
            // startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            // startInfo.FileName = "cmd.exe"; // Windows only
            // startInfo.Arguments = $"/C yt-dlp --extract-audio --audio-format mp3 --audio-quality 0 \"https://www.youtube.com/watch?v={url}\"";
            // process.StartInfo = startInfo;
            // process.Start();
            // process.WaitForExit();
            // var exitCode = process.ExitCode;
            // button.Content = $"Done (code {exitCode})";

            // Mac-friendly version from https://stackoverflow.com/a/65676526/11767771:
            var processInfo = new ProcessStartInfo()
            {
                FileName = "yt-dlp",
                Arguments = $"--extract-audio --audio-format mp3 --audio-quality 0 \"https://www.youtube.com/watch?v={url}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = "/Users/jd/Downloads/Music"
            };

            var process = Process.Start(processInfo);
            process.WaitForExit();
            button.Content = process.ExitCode == 0
                ? "Done (OK)"
                : $"Done (ERROR: {process.ExitCode})";
        }
    }
}