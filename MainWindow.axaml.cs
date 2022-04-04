using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
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
                log.Text += "Saved OK!\n";
                urlPartTextBox.Text = string.Empty;
            }
            else
            {
                log.Text += $"ERROR: Could not download the video (error code {downloadExitCodeOrNull.ToString() ?? "NULL"})\n\n";
            }

            var newFileName = this.FindControl<TextBox>("FileName");
            var saveFolderTextBox = this.FindControl<TextBox>("SaveFolder");
            if (string.IsNullOrWhiteSpace(newFileName?.Text))
                return;
            RenameFile(videoId, saveFolderTextBox.Text, newFileName.Text);
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

        private void RenameFile(string videoId, string directory, string newFileName)
        {
            GuardClauses();

            var log = this.FindControl<TextBlock>("Log");

            log.Text += $"Renaming file with video ID \"{videoId}\" to \"{newFileName}\"...\n";

            var foundFiles = Directory.EnumerateFiles(directory, $"*{videoId}*").ToList();

            if (foundFiles.Count == 0)
            {
                log.Text += $"No file to rename was found in \"{directory}\"\n";
                return;
            }

            if (foundFiles.Count > 1)
            {
                // Error, and print filenames
                log.Text += $"{foundFiles.Count} files containing \"{videoId}\" in their names were found:\n";
                foundFiles.ForEach(f => log.Text += "- " + f + "\n");
                return;
            }

            try
            {
                File.Move(foundFiles[0],
                          Path.Combine(directory, newFileName) + Path.GetExtension(foundFiles[0]),
                          overwrite: false);
            }
            catch (Exception ex)
            {
                 log.Text += $"RENAMING ERROR: " + ex.Message + "\n";
                 return;
            }

            log.Text += "Rename OK!\n";

            void GuardClauses()
            {
                if (string.IsNullOrWhiteSpace(videoId))
                    throw new InvalidOperationException();
            }
        }
    }
}