using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using QuoteProbe.Data;
using Whisper.net;
using Whisper.net.Ggml;
using Newtonsoft.Json;

namespace QuoteProbe.Services
{
    public class TransScriberService
    {
        private string _uri;
        public TransScriberService(string url)
        {
            _uri = url;
        }

        public async Task<List<Quote>> Transcribe(Video video, string modelName = "small.en", int timesToTry = 3)
        {
            if (timesToTry == 0)
            {
                Console.WriteLine("Failed to transcribe the video");
                return new();
            }

            Console.WriteLine("Transcribing the video");

            if (!File.Exists(modelName))
            {
                using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.BaseEn);
                using var fileWriter = File.OpenWrite(modelName);
                await modelStream.CopyToAsync(fileWriter);
            }


            try
            {
                string responseString;
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(1, 0, 0); // 1h timeout
                    var values = new Dictionary<string, string>
                    {
                        { "fp", $"{video.Location}" }
                    };

                    var content = new FormUrlEncodedContent(values);

                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://localhost:1273")
                    {
                        Content = content
                    };

                    var response = await client.SendAsync(requestMessage);
                    responseString = await response.Content.ReadAsStringAsync();
                }

                Console.WriteLine("Quotes:\n" + responseString);
                var quotes = JsonConvert.DeserializeObject<List<Quote>>(responseString) ?? new();
                Console.WriteLine("Quotes");
                quotes.ForEach(q => Console.WriteLine($"\tQUOTE: {q}"));
                return quotes;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await Task.Delay(1000);
                return await Transcribe(video, modelName, timesToTry - 1);
            }

        }
    }
}
