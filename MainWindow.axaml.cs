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
using YoutubeDownloader.Entities;

// Technically, the external download tool handles far more than just YouTube.
// I might add wider support at some point, but I have no need right now.
namespace YoutubeDownloader
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
                    var saveFolderTextBox = GetControl<TextBox>("SaveFolder");
                    saveFolderTextBox.Text = pathData;
                }
            }
        }

        /// <summary>
        /// Gets a control, given its name and type; otherwise, throws.
        /// </summary>
        private T GetControl<T>(string name) where T : class, IControl
        {
            return this.FindControl<T>(name)
                ?? throw new InvalidOperationException(
                    $"Could not find control \"{name}\" of type {typeof(T).FullName}.");
        }

        private async void OnDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var urlPartTextBox = GetControl<TextBox>("Url");
            var log = GetControl<TextBlock>("Log");
            log.Text = string.Empty;

            var saveFolderTextBox = GetControl<TextBox>("SaveFolder");
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

            var playlistControl = GetControl<CheckBox>("DownloadPlaylist");
            Download downloadInfo = playlistControl!.IsChecked == true
                ? new PlaylistDownload(urlPart)
                : new VideoDownload(urlPart);

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

            // Rename, if requested.
            var newFileName = GetControl<TextBox>("FileName");
            if (string.IsNullOrWhiteSpace(newFileName?.Text))
                return;

            RenameFile(downloadInfo.ParsedData.Id!, saveFolderTextBox.Text, newFileName.Text);
            newFileName.Text = string.Empty;
        }

        /// <summary>
        /// Arranges download settings, then calls the external program
        /// to download media data locally.
        /// </summary>
        /// <returns>A return code from the external program.</returns>
        private async Task<int?> DownloadMediaAsync(Download downloadData)
        {
            var log = GetControl<TextBlock>("Log");

            var args = "--extract-audio --audio-format mp3 --audio-quality 0";

            var splitChapters = GetControl<CheckBox>("SplitChapters");
            if (splitChapters!.IsChecked == true)
            {
                args += " --split-chapters";
                log.Text += "Split Chapters is ON\n";
            }

            var playlist = GetControl<CheckBox>("DownloadPlaylist");
            if (playlist!.IsChecked == true)
            {
                args += " --yes-playlist";
                log.Text += "Download Playlist is ON\n";
            }

            string directory;
            var saveFolderTextBox = GetControl<TextBox>("SaveFolder");
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
            log.Text += $"Command to run: {processFileName} {args} {downloadData.FullUrl}\n";
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

        /// <summary>
        /// Renames a single download file as specified.
        /// Does nothing (yet) if there are multiple matching files.
        /// </summary>
        /// <param name="mediaId"></param>
        /// <param name="directory"></param>
        /// <param name="newFileName"></param>
        private void RenameFile(string mediaId, string directory, string newFileName)
        {
            GuardClauses();

            var log = GetControl<TextBlock>("Log");

            log.Text += $"Renaming file with video ID \"{mediaId}\" to \"{newFileName}\"...\n";

            var foundFiles = Directory.EnumerateFiles(directory, $"*{mediaId}*").ToList();

            if (foundFiles.Count == 0)
            {
                log.Text += $"No file to rename was found in \"{directory}\"\n";
                return;
            }

            if (foundFiles.Count > 1)
            {
                log.Text += "ERROR: Cannot rename multiple files (yet).\n";
                log.Text += $"{foundFiles.Count} files containing \"{mediaId}\" in their names were found:\n";
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
                 log.Text += $"RENAMING ERROR: {ex.Message}\n";
                 return;
            }

            log.Text += "Rename OK!\n";

            void GuardClauses()
            {
                if (string.IsNullOrWhiteSpace(mediaId))
                {
                    throw new InvalidOperationException(
                        "A media ID must be provided."
                    );
                }

                if (string.IsNullOrWhiteSpace(directory))
                {
                    throw new InvalidOperationException(
                        "A directory must be provided."
                    );
                }

                if (string.IsNullOrWhiteSpace(newFileName))
                {
                    throw new InvalidOperationException(
                        "A new file name must be provided."
                    );
                }
            }
        }
    }
}
