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
            //var button = (Button) sender;
            //button.IsEnabled = false;

            var urlPartTextBox = this.FindControl<TextBox>("Url");
            var log = this.FindControl<TextBlock>("Log");
            log.Text = string.Empty;
            // var log = this.FindControl<TextBox>("Log");
            // LogText += $"Started at {System.DateTime.Now}...\n";

            // Extract the video ID from user input. IDs or entire URLs are accepted.
            const string pattern = @"^[\w|-]{11}$|(?<=v=)[\w|-]{11}";
            var urlPart = urlPartTextBox.Text;

            if (urlPart is null)
            {
                log.Text += "ERROR: A URL or video ID must be entered\n";
                return;
            }

            var match = Regex.Match(urlPart.Trim(), pattern, RegexOptions.Compiled);
            if (!match.Success)
            {
                log.Text += $"ERROR: Video ID could not be parsed from \"{urlPart}\"\n";
                return;
            }

            log.Text += "Video ID parsed OK: " + match.Value + "\n";
            // LogText += "Video ID parsed OK: " + match.Value + "\n";

            var fullUrl = $"\"https://www.youtube.com/watch?v={match.Value}\"";

            var args = "--extract-audio --audio-format mp3 --audio-quality 0";

            var splitChapters = this.FindControl<CheckBox>("SplitChapters");
            if (splitChapters?.IsChecked == true)
            {
                args += " --split-chapters";
                log.Text += "Split Chapters is ON\n";
            }

            var playlist = this.FindControl<CheckBox>("DownloadPlaylist");
            if (playlist?.IsChecked == true)
            {
                args += " --yes-playlist";
                log.Text += "Download Playlist is ON\n";
            }

            // TODO: Should be selectable, maybe saveable.
            string directory;
            if (Directory.Exists("/Users/jd/Downloads/Music"))
            {
                directory = "/Users/jd/Downloads/Music";
            }
            else if (Directory.Exists("/home/jx/Downloads/music"))
            {
                directory = "/home/jx/Downloads/music";
            }
            else
            {
                log.Text += $"ERROR: Couldn't find a save directory\n";
                return;
            }
            log.Text += $"Will save to directory \"{directory}\"\n";

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

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Mac-friendly version from https://stackoverflow.com/a/65676526/11767771:
            const string processFileName = "yt-dlp";
            log.Text += $"Command to run: {processFileName} {args} {fullUrl}\n";
            var processInfo = new ProcessStartInfo()
            {
                FileName = processFileName,
                Arguments = $"{args} {fullUrl}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = directory
            };

            var process = Process.Start(processInfo);
            if (process is null)
            {
                log.Text += $"ERROR: Could not start process {processFileName} -- is it installed?\n\n";
                return;
            }
            process.WaitForExit();
            log.Text += $"Done in {stopwatch.ElapsedMilliseconds:#,##0}ms";
            if (process.ExitCode == 0)
            {
                log.Text += "Saved OK!\n\n";
                urlPartTextBox.Text = string.Empty;
            }
            else
            {
                log.Text += $"ERROR: Could not download the video (error code {process.ExitCode})\n\n";
            }
        }
    }
}