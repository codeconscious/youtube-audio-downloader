namespace YoutubeDownloader.Entities;

public sealed class PlaylistDownload : Download
{
    public new static string Name => "Playlist";
    public new static string Pattern => @"(?<=list=)[\w\-]+";
    public new static string UrlBase => "https://www.youtube.com/playlist?list=";

    public PlaylistDownload(string urlPart) : base(urlPart, Pattern) { }
}
