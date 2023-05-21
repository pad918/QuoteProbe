using QuoteProbe.Data;

namespace QuoteProbe.Services
{
    public class FileTranscriberService
    {
        public FileTranscriberService()
        {

        }

        public List<Quote> Transcribe(string filePath)
        {
            var rawQuotes = GetRawQuotesSrt(File.ReadAllText(filePath));
            return rawQuotes;
        }

        public List<Quote> GetRawQuotesSrt(string file)
        {
            var lines = file.Split("\n").ToList().Skip(1).ToList();
            // Parse the lines into quotes using the following format:
            // 4000	6000	Oh, this is gonna be used, isn't it?
            var quotes = lines.Select(line =>
            {
                var lineSplit = line.Split("\t");
                if (lineSplit.Length != 3)
                {
                    return null;
                }
                var startTime = lineSplit[0];
                var endTime = lineSplit[1];
                var text = lineSplit[2];
                var q = new Quote()
                {
                    Text = text,
                    StartTimeSeconds = (float)(int.Parse(startTime) / 1000.0),
                    EndTimeSeconds = (float)(int.Parse(endTime) / 1000.0)
                };
                return q;
            });
            var nonNullQuotes = quotes!.Where(q => q != null).Select((q) => q!);
            return nonNullQuotes.ToList();
        }

        public List<Quote> GetRawQuoteVtt(string filePath)
        {
            var file = File.ReadAllText(filePath);
            var lines = file.Split("\n\n").ToList().Skip(1).ToList();
            var qutotes = lines.Select((line) =>
            {
                var parts = line.Split("\n");
                if (parts.Length < 2 || parts[0].Length < 17 + 12)
                    return null;
                var startTime = parts[0].Substring(0, 12);
                var endTime = parts[0].Substring(17, 12);
                var text = string.Join(" ", parts.ToList().Skip(1));
                return new Quote()
                {
                    Text = text,
                    StartTimeSeconds = (float)TimeOnly.Parse(startTime).ToTimeSpan().TotalSeconds,
                    EndTimeSeconds = (float)TimeOnly.Parse(endTime).ToTimeSpan().TotalSeconds
                };
            });
            var nonNullQuotes = qutotes.Where((i) => { return i != null; }).ToList();
            return nonNullQuotes!;
        }

    }
}
