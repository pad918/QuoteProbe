using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace QuoteProbe.Data
{
    public class QuoteOrigin : ISerializable
    {
        [JsonPropertyName("episode_number")]
        public int EpisodeNumber { get; set; }

        [JsonPropertyName("video")]
        public Video? Video { get; set; }

        public QuoteOrigin(Video video, int episodeNumber)
        {
            Video = video;
            EpisodeNumber = episodeNumber;
        }

        public QuoteOrigin() { }
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
