using TrashSearch.Data;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace TrashSearch.Services
{
    public class AudioDownloaderService
    {
        public AudioDownloaderService() { }

        public async Task<Video> FetchVideo(string url)
        {
            var ytdl = new YoutubeDL();
            IProgress<DownloadProgress> progress = new Progress<DownloadProgress>(report => Console.WriteLine($"Downloading: {(int)(report.Progress * 100)}%"));
            IProgress<string> fetchProg = new Progress<string>(report => Console.WriteLine($"Downloading: {report}"));
            //var res = await ytdl.RunAudioDownload(url, AudioConversionFormat.Mp3, progress: progress);
            //var b = Enumerable.Range(0, 10).Select((a) => "#").Aggregate((a, b) => a+b);
            Console.WriteLine("DOWNLOADED FILE...");
            var fetch = await ytdl.RunVideoDataFetch(url);
			//File.Move(res.Data, "temp.mp3");
			//Console.WriteLine(res.Success?$"Success: {res.Data}":$"Fail: {res.ErrorOutput.ToList().Aggregate((a, b) => $"{a} {b}")}");
			
			VideoData video = fetch.Data;
            return new(video.Title, video.WebpageUrl, "", video.UploadDate);
        }

        public async Task<VideoData> FetchData(string url)
        {
            var ytdl = new YoutubeDL();
            var fetch = await ytdl.RunVideoDataFetch(url);
            return fetch.Data;
        }
    }
}
