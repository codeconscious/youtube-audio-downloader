using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.Threading.Tasks;
using YouTubeDownloader.Entities;
using System.Text;

namespace YouTubeDownloader
{
    public partial class MainWindow : Window
    {
        // TODO: Get binding working.
        // public string LogText { get; set; } = string.Empty;
        // public string SaveFolder { get; set; } = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            // this.DataContext = this;

            // Auto-populate the save (output) directory, if available.
            const string fileContainingSavePath = "default-output-folder.txt";
            if (File.Exists(fileContainingSavePath))
            {
                var pathData = File.ReadAllText(fileContainingSavePath).Trim();
                if (Directory.Exists(pathData))
                {
                    var saveFolderTextBox = SaveFolder;
                    saveFolderTextBox.Text = pathData;
                }
            }
        }

        private async void OnDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var urlPartTextBox = Url;
            var log = Log;
            log.Text = string.Empty;

            var saveFolderTextBox = SaveFolder;
            if (string.IsNullOrWhiteSpace(saveFolderTextBox.Text))
            {
                log.Text += "ERROR: An output directory must be entered\n";
                return;
            }

            var urlPart = urlPartTextBox.Text;

            if (urlPart is null)
            {
                log.Text += "ERROR: A URL or media ID must be entered\n";
                return;
            }

            Download downloadInfo = new VideoDownload(urlPart);

            if (!downloadInfo.ParsedData.ParsedSuccessfully)
            {
                log.Text += $"ERROR: {downloadInfo.Name} media ID could not be parsed from \"{urlPart}\"\n";
                return;
            }

            log.Text += $"{downloadInfo.Name} media ID parsed OK: " + downloadInfo.ParsedData.Id + "\n";

            var downloadExitCodeOrNull = await DownloadMediaAsync(downloadInfo);
            if (downloadExitCodeOrNull is null)
            {
                log.Text += "ERROR: An unexpected error occurred.";
                return;
            }
            if (downloadExitCodeOrNull == 0) // Success
            {
                log.Text += "Saved OK!\n";
                urlPartTextBox.Text = string.Empty;
            }
            else
            {
                log.Text += $"ERROR: Could not download the video (error code {downloadExitCodeOrNull.ToString() ?? "unknown"}).\n\n";
                return;
            }
        }

        /// <summary>
        /// Arranges download settings, then calls the external program
        /// to download media data locally.
        /// </summary>
        /// <returns>A return code from the external program.</returns>
        private async Task<int?> DownloadMediaAsync(Download downloadData, bool audioOnly = false)
        {
            var log = Log;
            StringBuilder args = new();

            if (audioOnly)
            {
                args.Append("--extract-audio --audio-format mp3 --audio-quality 0");
            }
            else
            {
                args.Append("""-f "bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best" """);
            }

            var splitChapters = SplitChapters;
            if (splitChapters!.IsChecked == true)
            {
                args.Append(" --split-chapters");
                log.Text += "Split Chapters is ON\n";
            }

            // TODO: Make this automatic.
            // if (playlist!.IsChecked == true)
            // {
            //     args.Append(" --yes-playlist");
            //     log.Text += "Download Playlist is ON\n";
            // }

            string directory;
            var saveFolderTextBox = SaveFolder;
            if (string.IsNullOrWhiteSpace(saveFolderTextBox.Text))
            {
                log.Text += "ERROR: You must enter a folder path.\n";
                return null;
            }
            directory = saveFolderTextBox.Text.Trim();
            if (!Directory.Exists(directory))
            {
                log.Text += $"ERROR: Could not find directory \"{saveFolderTextBox.Text.Trim()}\"\n";
                return null;
            }
            log.Text += $"Will save to directory \"{directory}\"\n";

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            const string processFileName = "yt-dlp";
            log.Text += $"Running command: {processFileName} {args} {downloadData.FullUrl}\n";
            var processInfo = new ProcessStartInfo()
            {
                FileName = processFileName,
                Arguments = $"{args} {downloadData.FullUrl}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = directory
            };

            var process = await Task.Run(() => Process.Start(processInfo));
            if (process is null)
            {
                log.Text += $"ERROR: Could not start process {processFileName} -- is it installed?\n\n";
                return null;
            }
            process.WaitForExit();
            log.Text += $"Done in {stopwatch.ElapsedMilliseconds:#,##0}ms\n";
            return process.ExitCode;
        }
    }
}
