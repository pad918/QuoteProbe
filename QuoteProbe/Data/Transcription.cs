using Newtonsoft.Json;
using System.Text.Json.Nodes;

namespace TrashSearch.Data
{
    public class Transcription
    {
        public class Sentance
        {
            public TimeOnly StartTime { get; internal set; }
            public TimeOnly EndTime { get; internal set; }
            public string Text { get; internal set; } = String.Empty;
        }

        public List<Sentance> Sentances { get; internal set; }

        public Transcription()
        {
            Sentances = new List<Sentance>();
        }

        public static Transcription CreateFrom(JsonNode node)
        {
            var trans = new Transcription();

            return trans;
        }

    }
}
