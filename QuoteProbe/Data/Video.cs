using Newtonsoft.Json;
using System.Text;

namespace QuoteProbe.Data
{
    public class Video : ISerializable
    {
        public string Title { get; set; } = "";
        public string SourceUrl { get; set; } = "";
        public string Location { get; set; } = "";
        public DateTime? ReleaseDate { get; set; } = default(DateTime)!;
        public Video(string title, string url, string location, DateTime? releaseDate)
        {
            this.SourceUrl = url;
            this.Title = title;
            this.Location = location;
            ReleaseDate = releaseDate;
        }
        public Video() { }

        public override string ToString()
        {
            var properties = new List<string>() { Title, SourceUrl, Location, ReleaseDate?.ToString() ?? "No date" };
            return string.Join(", ", properties);
        }
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
