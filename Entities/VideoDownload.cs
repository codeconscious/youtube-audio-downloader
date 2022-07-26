namespace YoutubeDownloader.Entities;

public sealed class VideoDownload : Download
{
    public string Name => "Video";
    public static string Pattern => @"^[\w|-]{11}$|(?<=v=)[\w|-]{11}|(?<=youtu\.be\/).{11}";
    public static string UrlBase => "https://www.youtube.com/watch?v=";

    public VideoDownload(string urlPart) : base(urlPart, Pattern)
    {
    }
}
