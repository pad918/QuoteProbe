using Newtonsoft.Json;

namespace TrashSearch.Data
{
    public class QuoteMetadata : ISerializable
    {
        [JsonProperty("episode_number")]
        public int EpisodeNumber { get; set; }
        [JsonProperty("quote_id")]
        public int QuoteId { get; set; }

        [JsonProperty("quote")]
        public Quote? Quote { get; set; }

        public QuoteMetadata(int episodeNumber, int quoteId, Quote quote)
        {
            EpisodeNumber = episodeNumber;
            QuoteId = quoteId;
            Quote = quote;
        }
        public QuoteMetadata() { }
        public override string ToString()
        {
            return Serialize();
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
