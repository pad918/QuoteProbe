using Microsoft.SemanticKernel.Connectors.Memory.Qdrant;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using TrashSearch.Data;
using Newtonsoft.Json;

namespace TrashSearch.Services
{
    public class DatabaseService
    {
        private IKernel _kernel;
        private QdrantMemoryStore _memoyStore;
        private QdrantVectorDbClient _client;
        public DatabaseService()
        {
            string QdrantKey = Environment.GetEnvironmentVariable("QDRANT_API_KEY")!;
            string Endpoint = "http://localhost:6333";//Environment.GetEnvironmentVariable("QDRANT_ENDPOINT")!;
            var EmbeddingKey = System.Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new Exception("No api key found!");

            Console.WriteLine($"Using keys: \n\t{QdrantKey}\n\t{Endpoint}\n\t{EmbeddingKey}");

            //Create a client for qdrant
            var HTTPClient = new HttpClient();
            HTTPClient.DefaultRequestHeaders.Add("api-key", QdrantKey);
            _client = new QdrantVectorDbClient(Endpoint, 1536, 6333, HTTPClient);
            _memoyStore = new QdrantMemoryStore(_client);

            /*
			//Create a kernel
			_kernel = new KernelBuilder().Configure(c =>
			{
				c.AddOpenAITextEmbeddingGenerationService("ada", "text-embedding-ada-002", EmbeddingKey);
			})
			.WithMemoryStorage(new VolatileMemoryStore())//(_memoyStore)
			.Build();
			*/

            _kernel = new KernelBuilder().Configure(c =>
            {
                c.AddOpenAITextEmbeddingGenerationService("ada", "text-embedding-ada-002", EmbeddingKey);
            }).WithMemoryStorage(_memoyStore)
              .Build();
        }

        public async Task Add(string collectionName, string text, string id, string description)
        {
            await _kernel.Memory.SaveInformationAsync(collectionName, id: id, text: text, description: description); //description causes error
        }

        public async Task Remove(string collectionName, string id)
        {
            await _kernel.Memory.RemoveAsync(collectionName, id);
        }

        public async Task CreateCollection(string name)
        {
            await _memoyStore.CreateCollectionAsync(name);
        }

        public async Task<bool> DoesCollectionExist(string name)
        {
            return await _memoyStore.DoesCollectionExistAsync(name);
        }

        public async Task<List<MemoryQueryResult>> Search(string collectionName, string query)
        {
            var results = _kernel.Memory.SearchAsync(collectionName, query, limit: 10);
            return await results.AsAsyncEnumerable().ToListAsync();
        }

        public async Task<MemoryRecord?> SearchById(string collectionName, string id)
        {
            return await _memoyStore.GetAsync(collectionName, id); // OSÄKER OM RÄTT...
        }

        public async Task DeleteCollection(string collectionName)
        {
            await _memoyStore.DeleteCollectionAsync(collectionName);
        }

        public async Task ClearCollection(string collectionName)
        {
            await DeleteCollection(collectionName);
            await CreateCollection(collectionName);
        }

        public async Task<List<string>> GetAllCollections()
        {
            return await _memoyStore.GetCollectionsAsync().ToListAsync();
        }

        public async Task RemoveAll(string collectionName, List<string> pointIds)
        {
			await _memoyStore.RemoveBatchAsync(collectionName, pointIds);
		}

        public async Task RemoveEpisode(string collectionName, string metaCollectionName, EpisodeCollection episode)
        {
			var pointIds = episode.QuoteIds!.Select(p => p).ToList();
            Console.WriteLine($"Removing {pointIds.Count} quotes");
            //await RemoveAll(collectionName, pointIds);

            //The batch method did not work, so we do it one by one
            pointIds.ForEach(async (id) => {
                Console.WriteLine($"Removing id {id}");
                await _memoyStore.RemoveWithPointIdAsync(collectionName, id); 
            });
            Console.WriteLine($"Removing metadata");
			await _memoyStore.RemoveWithPointIdAsync(metaCollectionName, episode.QuoteOrigin!.EpisodeNumber.ToString());
            try
            {
                await _memoyStore.RemoveAsync(metaCollectionName, episode.QuoteOrigin!.EpisodeNumber.ToString());
            }
            catch (Exception){
                Console.WriteLine("Failed to remove async?");
            }
            Console.WriteLine("Done");
        }

        public async Task<EpisodeCollection?> GetEpisodeMetadata(string collectionName, int episodeNumber)
        {
			var record = await _memoyStore.GetAsync(collectionName, episodeNumber.ToString());
            var serialized = record?.Metadata.Description ?? "";
			return JsonConvert.DeserializeObject<EpisodeCollection>(serialized);
		}

        // SLOW, SHOULD NOT BE DONE ONE BY ONE!
        public async Task<List<MemoryRecord>> GetAllPointsInCollection(string collectionName, int episodeNumber, int bathSize = 100)
        {
            //return new();
            List<MemoryRecord> records = new();
            var abc = DoesCollectionExist(collectionName);
            abc.Wait();

			if (! abc.Result)
                return records;
            bool hasReachedEnd = false;
            for(int i = 0; !hasReachedEnd; i++)
            {
                List<string> pointIds = new();
				for (int j = 0; j < bathSize; j++)
                {
                    pointIds.Add($"{episodeNumber}_{i*bathSize+j}");
                }

				var batch = _memoyStore.GetBatchAsync(collectionName, pointIds);
				await foreach(var r in batch)
                {
                    if (r != null)
                        records.Add(r);
                    else
                        hasReachedEnd = true;
				}
			}

            return records;
		}

    }
}
