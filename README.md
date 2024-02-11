**This unfinished tool is no longer under active development. I highly recommend you use its replacement, [CCVTAC](https://github.com/codeconscious/ccvtac), which is a far more developed command line utility.**

-----

# YouTube Audio Downloader

This is a very simple UI wrapper for [yt-dlp](https://github.com/yt-dlp/yt-dlp) using [Avalonia](https://avaloniaui.net/) and C# that I created to get some practice with Avalonia. It provides a straightforward UI to more easily extract MP3 files from specified YouTube videos.

It's a work in (occasional) progress, is rather rough around the edges, and currently must be compiled manually, but works well for my own purposes.

<img width="800" alt="screenshot" src="https://user-images.githubusercontent.com/50596087/176416234-51373254-639d-4815-b17a-bcac0946b7ad.png">

Feel free to use it, but please do so responsibly!

## Prerequisites
- .NET 8 runtime
- [yt-dlp](https://github.com/yt-dlp/yt-dlp)

## Getting Started

Use `dotnet run` to start the program manually.

## TODO

- Better UI responsiveness during long operations ([issue #5](https://github.com/codeconscious/youtube-audio-downloader/issues/5))
- Check into Asian language support
