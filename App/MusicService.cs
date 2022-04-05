using System.Text.RegularExpressions;
using System.Text.Json;
using System.Net.Http.Headers;

namespace SongsApp
{
    public class MusicService : IMusicService
    {

        static readonly HttpClient httpClient;
        static readonly HttpClient spotifyHttpClient;
        
        static MusicService(){
            httpClient = new () { Timeout = TimeSpan.FromSeconds(15) };
            spotifyHttpClient = new() {Timeout = TimeSpan.FromSeconds(15)};
            spotifyHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            spotifyHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Program.spotifyToken);
        }

        public MusicService()
        {           
        }

        public async Task<string> GetLyricsAsync(Song song)
        {
            song.Artist = Regex.Replace(song.Artist, @"\s+", "%20");
            song.Title = Regex.Replace(song.Title, @"\s+", "%20");
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
            lyrics = Regex.Replace(lyrics, @"\n{2}", "\n");
            return lyrics;
        }

        private class Lyrics
        {
            public string? lyrics { get; set; }
            public string? error { get; set; }
        };


        public async Task<string> GetSpotifyLinkAsync(string searchString)
        {
            searchString = Regex.Replace(searchString, @"\s+", "%20");
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
    }
  
}