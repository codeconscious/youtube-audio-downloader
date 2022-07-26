using System.Text.RegularExpressions;

namespace YoutubeDownloader.Entities;

public abstract class Download
{
    public virtual string Name { get; }
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
