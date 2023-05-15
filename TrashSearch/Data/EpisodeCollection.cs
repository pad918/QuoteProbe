using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace TrashSearch.Data
{
    public class EpisodeCollection : ISerializable
    {

        [JsonPropertyName("quote_origin")]
        public QuoteOrigin? QuoteOrigin { get; set; }

        [JsonPropertyName("quote_ids")]
        public List<string>? QuoteIds { get; set; }

        public EpisodeCollection(QuoteOrigin origin, List<string> ids)
        {
            QuoteIds = ids;
            QuoteOrigin = origin;
        }
        public EpisodeCollection() { }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
}
