using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Youtube_Downloader
{
    public partial class MainWindow : Window
    {
        // public string LogText { get; set; } = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            // this.DataContext = this;
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;
            var urlPartTextBox = this.FindControl<TextBox>("Url");
            var log = this.FindControl<TextBlock>("Log");
            // var log = this.FindControl<TextBox>("Log");
            // LogText += $"Started at {System.DateTime.Now}...\n";

            // Extract the video ID from user input. IDs or entire URLs are accepted.
            const string pattern = @"^[\w|-]{11}$|(?<=v=)[\w|-]{11}";
            var urlPart = urlPartTextBox.Text?.Trim() ?? string.Empty;
            var match = Regex.Match(urlPart, pattern, RegexOptions.Compiled);
            if (!match.Success)
            {
                // button.Background = new SolidColorBrush(Colors.Red);
                // button.Content = "Invalid URL!";
                log.Text += $"ERROR: Video ID could not be parsed from \"{urlPart}\".\n";
                // LogText += "ERROR: Video ID could not be parsed.\n";
                // System.Threading.Tasks.Task.Delay(2000);
                // button.Background = new SolidColorBrush(Colors.Black);
                // button.Content = "Download";
                return;
            }

            log.Text += "Video ID parsed OK: " + match.Value + "\n";
            // LogText += "Video ID parsed OK: " + match.Value + "\n";

            var fullUrl = $"\"https://www.youtube.com/watch?v={match.Value}\"";

            var baseArgs = "--extract-audio --audio-format mp3 --audio-quality 0";

            var splitChapters = false; // TODO: Add to the UI.
            if (splitChapters)
                baseArgs += " --split-chapters";

            var playlist = false; // TODO: Add to the UI.
            if (playlist)
                baseArgs += " --yes-playlist";

            log.Text += $"Command to run: {baseArgs} {fullUrl}\n";

            // TODO: Should be selectable, maybe saveable.
            string directory;
            if (Directory.Exists("/Users/jd/Downloads/Music"))
                directory = "/Users/jd/Downloads/Music";
            else if (Directory.Exists("/home/jx/Downloads/music"))
                directory = "/home/jx/Downloads/music";
            else
                throw new DirectoryNotFoundException();
            log.Text += $"Saving to directory \"{directory}\"\n";

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
                Arguments = $"{baseArgs} {fullUrl}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = directory
            };

            var process = Process.Start(processInfo)
                          ?? throw new InvalidDataException();
            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                log.Text += "Saved OK!\n\n";
            }
            else
            {
                log.Text += $"ERROR: Could not process the file (error code {process.ExitCode})\n\n";
            }
        }
    }
}