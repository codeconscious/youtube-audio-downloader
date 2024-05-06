namespace YouTubeDownloader.Entities;

public sealed class VideoDownload : Download
{
    public new static string Name => "Video";
    public new static string Pattern => @"^[\w|-]{11}$|(?<=v=)[\w|-]{11}|(?<=youtu\.be\/).{11}";
    public new static string UrlBase => "https://www.youtube.com/watch?v=";

    public VideoDownload(string urlPart) : base(urlPart, Pattern) { }
}
