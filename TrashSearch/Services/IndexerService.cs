using TrashSearch.Components;
using TrashSearch.Data;
using YoutubeDLSharp;
using static System.Reflection.Metadata.BlobBuilder;

namespace TrashSearch.Services
{
    /*
        A singleton for indexing new videos. It is a singleton since only one type of indexing job
        can be done at a given time. Indexing should also not be bound to a user or a single request.
     */
    public class IndexerService
    {
        public class IndexerJob
        {
            public string Id { get; set; }
            public string Url { get; set; }
            public int EpisodeNumber { get; set; }
            public float Progress { get; set; } = 0;

            public enum IndexingState { Waiting, Indexing, Done, Failed}

            public Exception? exception { get; set; }
            public IndexingState State { get; set; } = IndexingState.Waiting;

            public IndexerJob(string id, string url, int episodeNumber)
            {
                Id = id;
                Url = url;
                EpisodeNumber = episodeNumber;
            }
        }
        public bool IsIndexing { get; private set; }

        public List<IndexerJob> Jobs { get; private set; } = new();

        private AudioDownloaderService downloaderService    = new();
        private FileTranscriberService transScriberService  = new();
        private DatabaseService        _database            = new();

        private string mainCollectionName     => "Test10";
        private string metadataCollectionName => "Test10_Meta";

        public IndexerService() { 
        
        }

        public async Task IndexVideos(IEnumerable<Tuple<int, string>> episodes)
        {
            foreach(var e in episodes) {
                var job = new IndexerJob(e.Item1.ToString(), e.Item2, e.Item1);
                job.State = IndexerJob.IndexingState.Indexing;
                Jobs.Add(job);
                EventHandler<float> eventHandler = new((_, p) => job.Progress = p);
                await IndexVideo(e.Item2, e.Item1, eventHandler);
                job.State = IndexerJob.IndexingState.Done;
            }
        }

        private async Task IndexVideo(string url, int episodeNumber, EventHandler<float>? progressCallback = null)
        {

            //Get video
            
            Video video;
            try
            {
                video = await downloaderService.FetchVideo(url);
            }
            catch (Exception)
            {
                throw new($"Failed to download video, exiting...");
            }

            List<Quote> quotes;
            try
            {
                var captionsDownloader = new CaptionsDownloaderService();
                var captionPath = await captionsDownloader.Download(url);
                quotes = transScriberService.GetRawQuoteVtt(captionPath);
            }
            catch (Exception e)
            {
                throw new($"Failed to download subtitles: {e.Message}");
            }
            await UploadQuotes(quotes, episodeNumber, video, progressCallback);

        }

        private async Task UploadQuotes(List<Quote> quotes, int episodeNumber, Video video, EventHandler<float>? progressCallback)
        {
            //Upload quotes
            try
            {
                //Create collection if it does not exist
                if (!await _database.DoesCollectionExist(mainCollectionName))
                {
                    await _database.CreateCollection(mainCollectionName);
                }

                //Check if episode already exists in the database
                var result = await _database.SearchById(mainCollectionName, episodeNumber.ToString());
                if (result != null)
                {
                    throw new("Episode already exists");
                }

                //Upload quotes and reference to video
                List<string> quoteIds = new();
                int i = 0;
                foreach (var q in quotes)
                {
                    string id = $"{episodeNumber}_{i}";
                    quoteIds.Add(id);
                    var metadata = new QuoteMetadata(episodeNumber, i, q).Serialize();

                    //Try to upload at max 10 times. Wait 10 seconds between attempts.
                    for (int t = 0; t < 10; t++)
                    {
                        try
                        {
                            await _database.Add(mainCollectionName, q.Text, id, metadata);

                            //FUNGERAR DET SÅHÄR?
                            progressCallback?.BeginInvoke(this, (i / (float)quotes.Count), (a) => { }, null);
                            i++;
                            break;
                        }
                        catch (Exception)
                        {
                            await Task.Delay(10000);
                        }
                        if (t >= 9)
                            throw new("Could not upload to database");
                    }
                }

                //Add metadata about the episode
                var episodeMetadata = new EpisodeCollection(new QuoteOrigin(video, episodeNumber), quoteIds);
                await _database.Add(metadataCollectionName, "", episodeNumber.ToString(), episodeMetadata.Serialize()!);

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
