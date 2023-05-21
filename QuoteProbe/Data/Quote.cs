using Newtonsoft.Json;

namespace QuoteProbe.Data
{
    public class Quote
    {
        [JsonProperty("text")]
        public string Text { get; set; } = String.Empty;

        [JsonProperty("startTime")]
        public float StartTimeSeconds { get; set; }

        [JsonProperty("endTime")]
        public float EndTimeSeconds { get; set; }

        public override string ToString()
        {
            return $"{Text} ({StartTimeSeconds} - {EndTimeSeconds})";
        }
    }
}
