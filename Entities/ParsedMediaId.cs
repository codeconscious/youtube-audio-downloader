namespace YoutubeDownloader.Entities;

public record ParsedMediaId(
    bool ParsedSuccessfully,
    string? Id
);
