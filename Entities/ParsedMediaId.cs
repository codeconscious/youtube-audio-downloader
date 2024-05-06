namespace YouTubeDownloader.Entities;

public record ParsedMediaId(
    bool ParsedSuccessfully,
    string? Id
);
