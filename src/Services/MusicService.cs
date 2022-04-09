using System.Text.RegularExpressions;
using System.Text.Json;
using System.Net.Http.Headers;
using Azure.AI.TextAnalytics;
using Azure;
using System.Text;
using SongsApp.Models;

namespace SongsApp.Services
{
    public class MusicService : IMusicService
    {
        public bool TextServiceEnabled { get; }
        private readonly HttpClient httpClient;
        private readonly HttpClient spotifyHttpClient;
        private readonly TextAnalyticsClient? azureClient;

        public MusicService(string spotifyToken)
        {
            TextServiceEnabled = false;
            httpClient = new() { Timeout = TimeSpan.FromSeconds(15) };
            spotifyHttpClient = new() { Timeout = TimeSpan.FromSeconds(15) };
            spotifyHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            spotifyHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", spotifyToken);
        }

        public MusicService(string azurePrivateEndpoint, string azurePrivateKey, string spotifyToken) : this(spotifyToken)
        {
            try
            {
                azureClient = new TextAnalyticsClient(new Uri(azurePrivateEndpoint), new AzureKeyCredential(azurePrivateKey));
                TextServiceEnabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initiating azureClient (edit config). {ex.Message} ");
            }
        }


        //

        private readonly Regex apiCommentsToDeleteRegex = new (@"\([Tt]hank.*for.*\)|\([Mm]erci.*\)|Paroles de la chanson.*\n", RegexOptions.Compiled);
        private readonly Regex twoNewLinesRegex = new (@"\n{2}", RegexOptions.Compiled);
        private readonly Regex spaceRegex = new(@"\s+", RegexOptions.Compiled);
        private class Lyrics
        {
            public string? lyrics { get; set; }
            public string? error { get; set; }
        };
        
        public async Task<string> GetSongLyricsAsync(Song song)
        {
            song.Artist = spaceRegex.Replace(song.Artist, "%20");
            song.Title = spaceRegex.Replace(song.Title, "%20");
            string url = $"https://api.lyrics.ovh/v1/{song.Artist}/{song.Title}";
            
            var response = await httpClient.GetAsync(url);
            string content = await response.Content.ReadAsStringAsync();
            Lyrics? lyricsObj;

            try
            {
                lyricsObj = JsonSerializer.Deserialize<Lyrics>(content);
            }  catch {           
                return $"can't parse response.\n {content}";
            }

            if (lyricsObj.lyrics == null)
            {
                return lyricsObj.error;
            }
           
            string lyrics = lyricsObj.lyrics;
            
            lyrics = twoNewLinesRegex.Replace(lyrics, "\n");
            lyrics = apiCommentsToDeleteRegex.Replace(lyrics, "");

            return lyrics;
        }

        //

        public async Task<string> GetSongLinkAsync(string searchString)
        {
            searchString = spaceRegex.Replace(searchString, "%20");
            string url = $"https://api.spotify.com/v1/search?q={searchString}&type=track";

            var response = await spotifyHttpClient.GetAsync(url);
            string content = await response.Content.ReadAsStringAsync();
            
            int idx = content.IndexOf("https://open.spotify.com/track/");
            
            if (idx == -1)
            {
                return content;
            }

            int idxEnd = content.IndexOf("\"", idx);
            if (idxEnd == -1)
            {
                return content;
            }

            return content.Substring(idx, idxEnd - idx);
        }

        //

        private readonly Regex newLinesRegex = new (@"\n{2,}|(?<=[a-zA-Z])\n|\s+\n", RegexOptions.Compiled);
        public async Task<string> GetSongSummaryAsync(Song song)
        {
            if (!TextServiceEnabled)
            {
                return "";
            }

            Console.WriteLine("waiting for summary..\n");
            if (song.Lyrics is null || song.Lyrics.Length < 20)
            {
                return "can't get song summary (lyrics too short)";
            }
            
            if (azureClient is null)
            {
                return "can't get song summary (azureClient wasn't initiated)";
            }
            
            string document = newLinesRegex.Replace(song.Lyrics, ". ") + ".";
            StringBuilder summary = new();

            try
            {                
                var batchInput = new List<string> { document };
                TextAnalyticsActions actions = new()
                {
                    ExtractKeyPhrasesActions = new List<ExtractKeyPhrasesAction>() { new ExtractKeyPhrasesAction() },
                    ExtractSummaryActions = new List<ExtractSummaryAction>() { new ExtractSummaryAction() }
                };
                
                AnalyzeActionsOperation operation = await azureClient.StartAnalyzeActionsAsync(batchInput, actions);
                await operation.WaitForCompletionAsync();
                
                await foreach (AnalyzeActionsResult documentsInPage in operation.Value)
                {
                    IReadOnlyCollection<ExtractKeyPhrasesActionResult> keyPhrasesResults = documentsInPage.ExtractKeyPhrasesResults; 
                    IReadOnlyCollection<ExtractSummaryActionResult> summaryResults = documentsInPage.ExtractSummaryResults;

                    ExtractKeyPhrasesActionResult keyPhrasesActionResult = keyPhrasesResults.First();
                    ExtractKeyPhrasesResult documentPhrasesResults = keyPhrasesActionResult.DocumentsResults.First();
                    
                    summary.Append($"Key phrases ({documentPhrasesResults.KeyPhrases.Count}): \n");
                    
                    foreach (string keyPhrase in documentPhrasesResults.KeyPhrases)
                    {
                        summary.Append($"{keyPhrase},  ");
                    }

                    summary.Remove(summary.Length - 3, 3);
                    summary.Append($"\n\nSummary Result:");

                    ExtractSummaryActionResult summaryActionResults = summaryResults.First();
                    ExtractSummaryResult documentSummaryResults = summaryActionResults.DocumentsResults.First();

                    foreach (SummarySentence sentence in documentSummaryResults.Sentences)
                    {
                        summary.Append($"\n - {sentence.Text.Replace('\n', ' ')}");
                    }                    
                }
            }
            catch (Exception e)
            {
                summary.Append($"Error: {e.Message} \n");
            }
            
            return summary.ToString();
        }
    }
  
}
