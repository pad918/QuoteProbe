using QuoteProbe.Data;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace QuoteProbe.Services
{
    public class CaptionsDownloaderService
    {
        public async Task<string> Download(string uri)
        {
            var ytdl = new YoutubeDL();
            var options = new OptionSet() { 
                AllSubs = true, ConvertSubs = "vtt", SkipDownload = true, 
                RestrictFilenames = true, NoPlaylist = true 
            };

            //Download all lyrics using the ytdl:

            Console.WriteLine("DOWNLOADING...");
            var progress = new Progress<DownloadProgress>(
                (a) => Console.WriteLine($"{(int)(a.Progress * 100.0f)}%")
                );
            //var result = await ytdl.RunAudioDownload(uri, progress: progress, overrideOptions: options);
            var result = await ytdl.RunWithOptions(new string[] { uri }, options, new());

            result.Data.ToList().ForEach(x => Console.WriteLine(x));
            var path = result.Data.ToList().Find((l) => l.StartsWith("[download] Destination: "))!.Substring("[download] Destination: ".Length);
            Console.WriteLine($"DONE: {path}");
			return path;
        }
    }
}
