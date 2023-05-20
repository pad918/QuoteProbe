using System.Linq.Expressions;
using TrashSearch.Components;
using TrashSearch.Data;
using YoutubeExplode;
using YoutubeDLSharp;
using static System.Reflection.Metadata.BlobBuilder;
using static TrashSearch.Services.IndexerService;

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

            public DateTime? StartTime { get; private set; }

            public enum IndexingState { Waiting, Indexing, Done, Failed }

            public Exception? exception { get; set; }
            public IndexingState State { get; set; } = IndexingState.Waiting;

            public void StartIndexing()
            {
                State = IndexingState.Indexing;
				StartTime = DateTime.Now;
			}

            public TimeSpan TimeRunning()
            {
                if(StartTime!=null) return DateTime.Now.Subtract((DateTime)StartTime);
                else return TimeSpan.Zero;
            }

            public TimeSpan EstimatedTimeLeft()
            {
                if (Progress == 0)
                {
                    return TimeSpan.MaxValue;
                }
                var running = TimeRunning();
                return running.Divide(Progress).Subtract(running);
            }

            public IndexerJob(string id, string url, int episodeNumber)
            {
                Id = id;
                Url = url;
                EpisodeNumber = episodeNumber;
            }
        }

        public bool TryLockIndexer()
        {
            lock (_isIndexing)
            {
                bool didLock = !IsIndexing;
                if (didLock)
                    _isIndexing.Value = true;
                return didLock;
            }
        }
        class TreadBool
        {
            public bool Value { get; set; } = false;
        }
        private TreadBool _isIndexing = new();

        // Is this how you make things threadsafe?
        public bool IsIndexing
        {
            get { return _isIndexing.Value; }
            private set { lock (_isIndexing) _isIndexing.Value = value; }
        }

        public List<IndexerJob> Jobs { get; private set; } = new();

        private AudioDownloaderService downloaderService = new();
        private FileTranscriberService transScriberService = new();
        private DatabaseService? _database;

        public string mainCollectionName => "TrashTaste_1";
        public string metadataCollectionName => "TrashTasteMetaData_1";

        public IndexerService()
        {

        }

        public IndexerService SetDatabase(DatabaseService databaseService)
        {
			_database = databaseService;
            return this;
		}

        public async Task IndexVideos(IEnumerable<Tuple<int, string>> episodes, int maxThreads=4)
        {
            Jobs = episodes.Select((e) => new IndexerJob(e.Item1.ToString(), e.Item2, e.Item1)).ToList();

			// create a SemaphoreSlim object with a count of 4
			SemaphoreSlim semaphore = new SemaphoreSlim(maxThreads);

			// execute your function in parallel with a maximum of 4 threads
			await Task.WhenAll(Jobs.Select(async job =>
			{
				await semaphore.WaitAsync();
				try
				{
                    job.StartIndexing();
					await IndexVideo(job.Url, job.EpisodeNumber, new((_, p) => job.Progress = p));
                    job.State = IndexerJob.IndexingState.Done;
				}
				finally
				{
					semaphore.Release();
				}
			}));

			//Threadsafe set?
			IsIndexing = false;

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
            Console.WriteLine($"Found {quotes.Count} quotes, uploading...");
            await UploadQuotes(quotes, episodeNumber, video, progressCallback);

        }

        private async Task UploadQuotes(List<Quote> quotes, int episodeNumber, Video video, EventHandler<float>? progressCallback)
        {
			//Upload quotes
			try
            {
                //Create collection if it does not exist
                if (true || !await _database.DoesCollectionExist(mainCollectionName))
                {
                    await _database!.CreateCollection(mainCollectionName);
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

                            if (progressCallback != null) progressCallback(this, (i / (float)quotes.Count));

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
