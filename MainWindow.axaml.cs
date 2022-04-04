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
        // public string SaveFolder { get; set; } = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            // this.DataContext = this;
        }

        private void OnDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var urlPartTextBox = this.FindControl<TextBox>("Url");
            var log = this.FindControl<TextBlock>("Log");
            log.Text = string.Empty;
            // var log = this.FindControl<TextBox>("Log");
            // LogText += $"Started at {System.DateTime.Now}...\n";

            // Extract the video ID from user input. IDs or entire URLs are accepted.
            const string pattern = @"^[\w|-]{11}$|(?<=v=)[\w|-]{11}|(?<=youtu\.be\/).{11}";
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
            var videoId = match.Value;
            log.Text += "Video ID parsed OK: " + videoId + "\n";
            // LogText += "Video ID parsed OK: " + match.Value + "\n";

            var downloadExitCodeOrNull = DownloadVideo(videoId);
            if (downloadExitCodeOrNull is null)
            {
                log.Text += "ERROR: An unexpected error occurred.";
            }
            if (downloadExitCodeOrNull == 0) // Success
            {
                log.Text += "Saved OK!";
                urlPartTextBox.Text = string.Empty;
            }
            else
            {
                log.Text += $"ERROR: Could not download the video (error code {downloadExitCodeOrNull.ToString() ?? "NULL"})\n\n";
            }
        }

        private int? DownloadVideo(string videoId)
        {
            var log = this.FindControl<TextBlock>("Log"); // TODO: Do correctly.
            var fullUrl = $"\"https://www.youtube.com/watch?v={videoId}\"";

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

            var saveFolderTextBox = this.FindControl<TextBox>("SaveFolder");
            string directory;
            if (string.IsNullOrWhiteSpace(saveFolderTextBox.Text))
            {
                log.Text += "ERROR: You must enter a folder path.";
                return null;
            }
            if (!Directory.Exists(saveFolderTextBox.Text.Trim()))
            {
                log.Text += $"ERROR: Could not find directory \"{saveFolderTextBox.Text.Trim()}\"";
                return null;
            }
            directory = saveFolderTextBox.Text.Trim();
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
                return null;
            }
            process.WaitForExit();
            log.Text += $"Done in {stopwatch.ElapsedMilliseconds:#,##0}ms\n";
            return process.ExitCode;
        }

        private void RenameFile(string videoId, string path)
        {
            //var a = System.IO.Directory.EnumerateFiles(path, $"*{videoId}*");
        }
    }
}