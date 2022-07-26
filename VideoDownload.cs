using System.Text.RegularExpressions;

namespace Youtube_Downloader;

// TODO: Separate classes to various files.

public record ParsedMediaId(
    bool ParsedSuccessfully, string? Id
);

public abstract class Download
{
    public string Name { get; }
    public string UrlBase { get; }
    public string FullUrl => $"\"{UrlBase}{ParsedData.Id}\"";
    public ParsedMediaId ParsedData { get; }
    public static string Pattern { get; }

    public Download(string urlPart, string pattern)
    {
        var match = Regex.Match(urlPart.Trim(), pattern, RegexOptions.Compiled);
        ParsedData = match.Success
            ? new ParsedMediaId(true, match.Value)
            : new ParsedMediaId(false, null);
    }
}

public sealed class VideoDownload : Download
{
    public string Name => "Video";
    public static string Pattern => @"^[\w|-]{11}$|(?<=v=)[\w|-]{11}|(?<=youtu\.be\/).{11}";
    public static string UrlBase => "https://www.youtube.com/watch?v=";

    public VideoDownload(string urlPart) : base(urlPart, Pattern)
    {
    }
}

public sealed class PlaylistDownload : Download
{
    public string Name => "Playlist";
    public static string Pattern => @"(?<=list=)[\w]+";
    public static  string UrlBase => "https://www.youtube.com/playlist?list=";

    public PlaylistDownload(string urlPart) : base(urlPart, Pattern)
    {
    }
}
