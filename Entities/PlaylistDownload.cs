using System.Text.RegularExpressions;

namespace YoutubeDownloader.Entities;

public sealed class PlaylistDownload : Download
{
    public string Name => "Playlist";
    public static string Pattern => @"(?<=list=)[\w\-]+";
    public static  string UrlBase => "https://www.youtube.com/playlist?list=";

    public PlaylistDownload(string urlPart) : base(urlPart, Pattern)
    {
    }
}
